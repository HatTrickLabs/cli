# HatTrick.CommandLine

A .NET 9 library for building structured, validated CLI applications. Covers argument parsing, type conversion, constraint validation, and handler dispatch.

---

## Quick Start

```csharp
var registry = DefinitionRegistry.GetInstance();

registry.Add(new NamespaceDefinition("htl", "HatTrick Labs namespace"));

var cmdDef = new CommandDefinition("htl.guid");
cmdDef.Help = "Generates one or many GUID values.";
cmdDef.AddOption<string>(key: "format", defaultArg: "D", help: "GUID format specifier.", (terse: "-f", verbose: "--format"));
cmdDef["format"].AcceptedValues("N", "D", "B", "P", "X");
cmdDef.AddOption<int>(key: "count", defaultArg: 1, help: "Number of GUIDs to generate.", (terse: "-c", verbose: "--count"));
cmdDef["count"].ApplyConstraint<int>(cnt => cnt > 0 && cnt <= 100, "Allowed Range", "1..100.");
cmdDef.Handler = (cmd) =>
{
    int count = cmd["count"].GetValue<int>();
    string format = cmd["format"].GetValue<string>();
    for (int i = 0; i < count; i++)
        Console.WriteLine(Guid.NewGuid().ToString(format));
};
registry.Add(cmdDef);

Command cmd = CommandBuilder.Build(args);
CommandExecutor executor = registry.GetCommandExecutor(cmd);
executor.Execute();
```

---

## Commands and Namespaces

A command name is a simple identifier or a dot-separated path. When dots are present, each segment except the last is a namespace:

```
guid --format D              # no namespace
netsh.wlan.connect --name MyNetwork   # two namespaces, one command
```

Namespaces are registered separately via `NamespaceDefinition` and require a name and help text. The full dotted string is the command identifier — there is no positional subcommand chaining.

---

## Options and Flags

All command input — flagged or positional — resolves to an option. The library does not distinguish between traditional "flags" (boolean switches) and "options" (value-bearing parameters) — all are options.

**Terse flags** — single dash, single character:

```
-f  -o  -v
```

**Verbose flags** — double dash, more than one character:

```
--from  --overwrite  --verbose
```

An option definition requires a verbose flag. The terse flag is optional. Either form is accepted at the command line when both are defined.

### Positional Arguments

Arguments can be passed without flags, matched to options by their definition order. Positional passing is valid until a position is skipped — at that point the remaining arguments must be flagged.

Given a `copy` command with options defined in order `from`, `to`, `silent`, `recursive`:

```
copy d:\tmp\doc.txt d:\tmp2\doc.txt                               # fully positional
copy --from d:\tmp\doc.txt --to d:\tmp2\doc.txt                   # equivalent

copy d:\tmp\doc.txt d:\tmp2\doc.txt --recursive true              # positional until skip, then flagged
copy --from d:\tmp\doc.txt --to d:\tmp2\doc.txt --recursive true  # equivalent ('silent' uses default in both)
```

### Boolean Options

A boolean option does not require an explicit argument. The presence of the flag implies `true`:

```
myapp.copy --overwrite
# equivalent to:
myapp.copy --overwrite true
```

### Terse Flag Chaining

Terse boolean flags can be chained Unix-style. The last flag in the chain may take an argument; all preceding flags are treated as boolean `true`. The following three forms are equivalent:

```
myapp.copy -osf c:\tmp\hello.txt -t c:\tmp2\hello.txt
myapp.copy -o -s -f c:\tmp\hello.txt -t c:\tmp2\hello.txt
myapp.copy --overwrite --silent --from c:\tmp\hello.txt --to c:\tmp2\hello.txt
```

---

## Parsing Pipeline

Input goes through three sequential stages:

| Stage | Input | Output | Role |
|:---|:---|:---|:---|
| `Scanner` | `string` | `string[]` | Splits raw input into tokens |
| `Tokenizer` | `string[]` | `Token[]` | Classifies tokens (command, terse flag, verbose flag, compound terse, argument, explicit-assign) |
| `Parser` | `Token[]` | `Command` | Assembles tokens into a `Command` object and `Option` objects |

`CommandBuilder` is the static entry point that chains all three stages:

```csharp
Command cmd = CommandBuilder.Build(args);        // string[]
Command cmd = CommandBuilder.Build("myapp.copy --from c:\\tmp\\a.txt --to c:\\tmp\\b.txt");
```

---

## Option Types

Options are generic — `OptionDefinition<T>`. Built-in conversion is handled by `OptionTypeMap` for:

- Primitives: `string`, `int`, `double`, `bool`, `char`
- Nullables: `int?`, `bool?`, `DateTime?`, etc.
- Common BCL types: `DateTime`, `DateOnly`, `TimeOnly`, `DateTimeOffset`, `Guid`, `TimeSpan`

For any other type — arrays, custom domain types, etc. — a `Func<string, T>` converter must be provided:

```csharp
// Static default
cmdDef.AddOption<string>(key: "format", defaultArg: "D", help: "GUID format specifier.", (terse: "-f", verbose: "--format"));

// Dynamic default (evaluated at execution time)
cmdDef.AddOption<DateOnly>(key: "day", defaultArg: (() => DateOnly.FromDateTime(DateTime.Now), "Current date."), help: "Date of time entry.", (terse: "-d", verbose: "--day"));

// Terse flag omitted (verbose is required, terse is optional)
cmdDef.AddOption<string>(key: "period", defaultArg: "all", help: "Snapshot period filter.", (terse: null, verbose: "--period"));

// Custom converter for arrays
Func<string, int[]> split = v =>
{
    var parts = v.Split(',', StringSplitOptions.RemoveEmptyEntries);
    var nums = new int[parts.Length];
    for (int i = 0; i < parts.Length; i++)
        nums[i] = OptionTypeMap.ParseOptionArgument<int>(parts[i]);
    return nums;
};
cmdDef.AddOption<int[]>(key: "values", help: "Comma-delimited integers.", split, (terse: "-v", verbose: "--values"));

// Custom converter for enums and other BCL types
cmdDef.AddOption<FileMode>(key: "mode", help: "File open mode.", arg => Enum.Parse<FileMode>(arg, true), (terse: "-m", verbose: "--mode"));
```

---

## Constraints

Constraints run after parsing, before handler invocation.

### Option-Level Constraints

Applied to a single option's value:

| Constraint | Behavior |
|:---|:---|
| `MustAssign` | Option must be present in input |
| `Default` | Supplies a value if the option is absent |
| `AcceptedValues` | Restricts the option to a whitelist of values |
| `CustomArgumentConstraint` | User-supplied predicate: `Func<T, bool>` |

### Command-Level Constraints

Applied across the full set of parsed options:

| Constraint | Behavior |
|:---|:---|
| `MustAssignOneOf` | At least one option from a named set must be present |
| `MutuallyExclusiveSet` | At most one option from a named set may be present |
| `CustomCommandConstraint` | User-supplied predicate over the full `ICommand` |

Constraint execution order: defaults first (may promote `EmptyOption` to `DefaultOption`), then option-level constraints, then command-level constraints.

```csharp
// Accepted values
cmdDef["format"].AcceptedValues("N", "D", "B", "P", "X");
cmdDef["period"].AcceptedValues("all", "day", "wtd", "mtd", "ytd");

// Range check
cmdDef["count"].ApplyConstraint<int>(cnt => cnt > 0 && cnt <= 100, "Allowed Range", "1..100.");

// Half-hour increment
cmdDef["hours"].ApplyConstraint<double>(
    h => h > 0 && 2 * h == (int)(2 * h),
    "Valid Increment",
    "Must be a positive multiple of 0.5."
);

// Path validation
cmdDef["path"].ApplyConstraint<string>(
    Path.IsPathFullyQualified,
    "Fully Qualified Path",
    "Must be a valid fully qualified path."
);

// Non-empty string
cmdDef["comment"].ApplyConstraint<string>(
    s => !string.IsNullOrWhiteSpace(s),
    "Not Whitespace",
    "Must contain a non-whitespace value."
);

// Command-level: cross-option validation
cmdDef.ApplyConstraint(
    cmd => cmd["age"].GetValue<int>() >= 18,
    "Age Restriction",
    "Must be 18 or older."
);

// At least one of a set must be assigned
cmdDef.MustAssignOneOf("json", "xml", "csv");

// At most one of a set may be assigned
cmdDef.MutuallyExclusiveSet("json", "xml", "csv");
```

---

## Handlers

A `CommandDefinition` has either a synchronous or asynchronous handler:

```csharp
// Synchronous
cmdDef.Handler = (cmd) =>
{
    string path = cmd["path"].GetValue<string>();
    // ...
};

// Asynchronous
cmdDef.AsyncHandler = async (cmd) =>
{
    string path = cmd["path"].GetValue<string>();
    await DoWorkAsync(path);
};
```

`CommandExecutor.Execute()` and `CommandExecutor.ExecuteAsync()` dispatch to the appropriate handler after constraint validation.

---

## Pre-Execution Hooks

`OnPreEnsure` fires after parsing but before constraint evaluation. Use it to manipulate raw argument strings before type conversion runs:

```csharp
cmdDef.OnPreEnsure = (cmd) =>
{
    var dayOpt = cmd.GetOption(o => o.Flag == "--day");
    if (int.TryParse(dayOpt.Argument, out int shift))
        dayOpt.OverrideArgument(DateTime.Today.AddDays(shift).ToString("yyyy-MM-dd"));
};
```

---

## Help System

Generated help output is accessible by passing the `-h|-?|--help` option to the default command. The namespace or command name is passed as the argument to scope the output.

```
myapp --help                        # root help (no argument = default command)
myapp --help netsh                  # namespace help
myapp --help netsh.wlan.connect     # command help
myapp --help netsh.*                # all commands under a namespace
```

Help output is template-rendered using embedded resources. The `Help` string on each `CommandDefinition` and `NamespaceDefinition` feeds into the rendered output. Individual options can be excluded via `Hide()`.

---

## Object Mapping

`MapTo<T>` and `MapToSignature<T>` hydrate domain objects or invoke typed delegates from parsed option values, as an alternative to writing handler body logic manually.

```csharp
// Map options to a POCO by explicit correlation (option key → property name)
cmdDef.AddOption<string>(key: "first", help: "First name.", ("-f", "--first"));
cmdDef.AddOption<string>(key: "last",  help: "Last name.",  ("-l", "--last"));
cmdDef.AddOption<int>   (key: "age",   help: "Age.",        ("-a", "--age"));

cmdDef.MapTo<Person>(
    ("first", nameof(Person.FirstName)),
    ("last",  nameof(Person.LastName)),
    ("age",   nameof(Person.Age))
).Then(person => PersonService.Save(person));

// If option keys match property names exactly, correlation tuples can be omitted
cmdDef.MapTo<Person>().Then(person => PersonService.Save(person));

// Use "~" to exclude an option from mapping
cmdDef.MapTo<Person>(
    ("gender", "~")
).Then(person => PersonService.Save(person));

// Map to a method signature (option key → parameter name)
cmdDef.MapToSignature<Action<string, string, int>>(
    ("first", "firstName"),
    ("last",  "lastName"),
    ("age",   "age")
).Then(PersonService.Save);
```

---

## Interactive Loop

`CommandLoopHandler` supports a continuous command execution mode — the application accepts repeated commands without restarting. Three built-in commands are reserved: `exit` and `bye` terminate the loop; `cls` clears the screen.

```
> netsh.wlan.connect --name MyNetwork
> netsh.wlan.disconnect
> cls
> exit
```

---

## Definition and Registry

`DefinitionRegistry` is a singleton. All `CommandDefinition` and `NamespaceDefinition` objects are registered before execution begins. The registry validates naming collisions between commands and namespaces.

```csharp
var registry = DefinitionRegistry.GetInstance();

registry.Add(new NamespaceDefinition("netsh", "Network shell commands"));
registry.Add(new NamespaceDefinition("netsh.wlan", "Wireless LAN commands"));

var connectDef = new CommandDefinition("netsh.wlan.connect");
connectDef.Help = "Connect to a wireless network.";
connectDef.AddOption<string>(key: "name", help: "Network name", flags: ("-n", "--name"));
connectDef["name"].MustAssign();
connectDef.Handler = (cmd) => { /* ... */ };
registry.Add(connectDef);

Command cmd = CommandBuilder.Build(args);
CommandExecutor executor = registry.GetCommandExecutor(cmd);
executor.Execute();
```

---

## Exception Model

| Exception | Thrown when |
|:---|:---|
| `CommandDefinitionException` | Invalid or conflicting definition at registration time |
| `CommandInputException` | User-provided input violates a constraint |
| `CommandParseException` | Input could not be parsed into a valid command structure |
| `OptionArgumentException` | Option argument cannot be converted to the declared type |
| `CommandExecutionException` | Handler threw during execution |

---

## Custom Collection

`SetOf<T>` is the internal collection type used throughout the library. It replaces `List<T>` with pre-sized capacity buckets (4, 8, 16 … 1,048,576) and ref-based access, which constraint execution uses to replace option instances in-place during the constraint pass.
