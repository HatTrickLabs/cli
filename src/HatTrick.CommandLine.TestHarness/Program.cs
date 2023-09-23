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
            Registry.GetInstance().ExecuteCommand(cmd);
        }

        static void RegisterCommands()
        {
            CommandDefinition cmd = new("guid");
            cmd.Help = "Generates new globaly unique identifiers.";
            cmd.Handler = (c) => { Console.WriteLine(Guid.NewGuid().ToString()); };
            Registry.GetInstance().Add(cmd);
        }
    }
}