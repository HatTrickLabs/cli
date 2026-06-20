# HatTrick.CommandLine

[![NuGet](https://img.shields.io/nuget/v/HatTrick.CommandLine.svg)](https://www.nuget.org/packages/HatTrick.CommandLine/)
[![License: Apache-2.0](https://img.shields.io/badge/license-Apache--2.0-blue.svg)](LICENSE)

Argument parsing, type conversion, constraint validation, and handler dispatch for building structured, validated .NET 9 CLI applications.

**[Full documentation](https://hattricklabs.com/docs/cli/)** | **[hattricklabs.com](https://hattricklabs.com)**

---

## Installation

```bash
dotnet add package HatTrick.CommandLine
```

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

## Features

- Namespaces and commands, with naming-collision validation at registration
- Options and flags — positional arguments, boolean flags, terse flag chaining
- Type conversion for built-in and custom types, including `nint`/`nuint`
- Option-level and command-level constraints, including mutually-exclusive sets
- Handlers, pre-execution hooks, and a default-command/help/version pipeline
- Masked input for interactive prompts
- `MapTo<T>` / `MapToSignature<T>` to map parsed options directly to POCOs or method calls
- A typed exception model (`CommandDefinitionException`, `CommandInputException`, `CommandParseException`, `OptionArgumentException`, `CommandExecutionException`)

See the [full documentation](https://hattricklabs.com/docs/cli/) for all of the above in depth.

---

## License

Apache-2.0 — see [LICENSE](LICENSE).
