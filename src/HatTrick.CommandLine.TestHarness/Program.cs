using System;
using System.CodeDom.Compiler;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using HatTrick.CommandLine;

namespace HatTrick.CommandLine.TestHarness
{
    internal class Program
    {
        static void Main(string[] args)
        {
            RegisterCommands();

            Command cmd = Parser.Parse(args);
            DefinitionRegistry.GetInstance().ExecuteCommand(cmd);
        }

        static void RegisterCommands()
        {
            CommandDefinition cmd = new("guid");
            cmd.Help = "Generates new globaly unique identifiers.";
            cmd.Handler = (c) => { Console.WriteLine(Guid.NewGuid().ToString()); };
            cmd.AddOption<string>(key: "format", help: "Output format.", (terse: "-f", verbose: "--format"));
            cmd["format"].ApplyConstraint<string>((arg) => false, "constraint name", "Constraint description.");
            DefinitionRegistry.GetInstance().Add(cmd);
        }
    }
}