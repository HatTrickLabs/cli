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

When an explicit argument is provided, the following literals are accepted (case-insensitive):

| Value | Interpretation |
|:---|:---|
| `true` `yes` `y` `1` | `true` |
| `false` `no` `n` `0` | `false` |

### Terse Flag Chaining

Terse flags can be chained Unix-style. The last flag in the chain may take an argument; all preceding flags must be boolean options and are resolved as `true`. The following three forms are equivalent:

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

- Any type implementing `IConvertible`: all numeric primitives (`byte`, `sbyte`, `short`, `ushort`, `int`, `uint`, `long`, `ulong`, `nint`, `nuint`, `float`, `double`, `decimal`), `char`, `bool`, `string`, `DateTime`, and nullable variants of each
- BCL types with dedicated parsers: `DateOnly`, `TimeOnly`, `DateTimeOffset`, `TimeSpan`, `Guid`, and nullable variants of each

For any other type — arrays, enums, custom domain types — a `Func<string, T>` converter must be provided:

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

Constraints run after parsing, before handler invocation. Execution order: defaults first (may promote `EmptyOption` to `DefaultOption`), then option-level constraints, then command-level constraints.

### Option-Level Constraints

Applied to a single option's value:

| Constraint | Behavior |
|:---|:---|
| `MustAssign` | Option must be present in input |
| `Default` | Supplies a value if the option is absent |
| `AcceptedValues` | Restricts the option to a whitelist of values |
| `CustomArgumentConstraint` | User-supplied predicate: `Func<T, bool>` |

```csharp
// MustAssign
cmdDef.AddOption<string>(key: "path", help: "Target path.", ("-p", "--path"));
cmdDef["path"].MustAssign();
```

```csharp
// AcceptedValues
cmdDef.AddOption<string>(key: "format", defaultArg: "D", help: "GUID format specifier.", ("-f", "--format"));
cmdDef["format"].AcceptedValues("N", "D", "B", "P", "X");
```

```csharp
// CustomArgumentConstraint
cmdDef.AddOption<int>(key: "count", defaultArg: 1, help: "Number of items.", ("-c", "--count"));
cmdDef["count"].ApplyConstraint<int>(cnt => cnt > 0 && cnt <= 100, "Allowed Range", "1..100.");

cmdDef.AddOption<double>(key: "hours", help: "Hours logged.", ("-h", "--hours"));
cmdDef["hours"].ApplyConstraint<double>(
    h => h > 0 && 2 * h == (int)(2 * h),
    "Valid Increment",
    "Must be a positive multiple of 0.5."
);

cmdDef.AddOption<string>(key: "path", help: "Target path.", ("-p", "--path"));
cmdDef["path"].ApplyConstraint<string>(
    Path.IsPathFullyQualified,
    "Fully Qualified Path",
    "Must be a valid fully qualified path."
);

cmdDef.AddOption<string>(key: "comment", help: "Entry comment.", ("-c", "--comment"));
cmdDef["comment"].ApplyConstraint<string>(
    s => !string.IsNullOrWhiteSpace(s),
    "Not Whitespace",
    "Must contain a non-whitespace value."
);
```

### Command-Level Constraints

Applied across the full set of parsed options:

| Constraint | Behavior |
|:---|:---|
| `MustAssignOneOf` | At least one option from a named set must be present |
| `MutuallyExclusiveSet` | At most one option from a named set may be present |
| `CustomCommandConstraint` | User-supplied predicate over the full `ICommand` |

```csharp
// CustomCommandConstraint
cmdDef.AddOption<int>(key: "age", help: "Applicant age.", ("-a", "--age"));
cmdDef.ApplyConstraint(
    cmd => cmd["age"].GetValue<int>() >= 18,
    "Age Restriction",
    "Must be 18 or older."
);
```

```csharp
// MustAssignOneOf
cmdDef.AddOption<bool>(key: "json", help: "Output as JSON.", (null, "--json"));
cmdDef.AddOption<bool>(key: "xml", help: "Output as XML.", (null, "--xml"));
cmdDef.AddOption<bool>(key: "csv", help: "Output as CSV.", (null, "--csv"));
cmdDef.MustAssignOneOf("json", "xml", "csv");
```

```csharp
// MutuallyExclusiveSet
cmdDef.AddOption<bool>(key: "json", help: "Output as JSON.", (null, "--json"));
cmdDef.AddOption<bool>(key: "xml", help: "Output as XML.", (null, "--xml"));
cmdDef.AddOption<bool>(key: "csv", help: "Output as CSV.", (null, "--csv"));
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
//if cmd takes a date, command designer could allow an integer to avoid the user having to type out a full date: 0: today, -1: yesterday, 1: tomorrow, etc...
cmdDef.OnPreEnsure = (cmd) =>
{
    var dayOpt = cmd.GetOption(o => o.Flag == "--day");
    if (int.TryParse(dayOpt.Argument, out int shift))
        dayOpt.OverrideArgument(DateTime.Today.AddDays(shift).ToString("yyyy-MM-dd"));
};
```

---

## Default Command

The default command runs when no command name is passed. It provides three built-in options, which are mutually exclusive — only one may be provided per invocation:

| Option | Flags | Behavior |
|:---|:---|:---|
| `help` | `-h` \| `-?` \| `--help` | Renders help output scoped to a command or namespace |
| `version` | `-v` \| `--version` | Displays assembly version information |
| `run` | `-r` \| `--run` | Starts an interactive command loop |

### Help

The argument scopes the output to a command, namespace, or wildcard:

```
myapp --help                        # root help (no argument = default command)
myapp --help netsh                  # namespace help
myapp --help netsh.wlan.connect     # command help
myapp --help netsh.*                # all commands under a namespace
```

Help output is template-rendered using embedded resources. The `Help` string on each `CommandDefinition` and `NamespaceDefinition` feeds into the rendered output. Individual options can be excluded via `Hide()`.

### Version

Reflects over the entry assembly and all referenced assemblies, excluding system assemblies. Each assembly name and version is printed in `Major.Minor.Build` format, with names dot-padded for alignment:

```
myapp --version
```

```
MyApp........................1.0.0
HatTrick.CommandLine.........2.4.1
```

### Run

Starts a continuous command execution loop — the application accepts repeated commands without exiting.

```
myapp --run
```

```
myapp> netsh.wlan.connect --name MyNetwork
myapp> netsh.wlan.disconnect
myapp> cls
myapp> exit
```

Three built-in loop commands are reserved: `exit` and `bye` terminate the loop; `cls` clears the screen.

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

// Map to a method signature — T inferred when the target is unambiguous
// static void Save(string firstName, string lastName, int age) { ... }
cmdDef.MapToSignature(PersonService.Save,
    ("first", "firstName"),
    ("last",  "lastName")
).Go();

// When the target is overloaded, provide the Action type to select the overload
// static void Save(Person person) { ... }
// static void Save(string firstName, string lastName, int age) { ... }
cmdDef.MapToSignature<Action<string, string, int>>(PersonService.Save,
    ("first", "firstName"),
    ("last",  "lastName")
).Go();
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
