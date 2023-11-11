using System;
using System.Runtime.InteropServices;
using System.Text;

namespace HatTrick.CommandLine.TestHarness
{
    internal class Program
    {
        //string input = "htl.guid -u \"100 00\" --format X --silent:true    -abc=\"d:\\tmp\"  -xyz=abc -efg:-b   --quiet=true --force:true -p \"d:\tmp\abcdefg xyz\" ";
        static void Main(string[] args)
        {
            var registry = DefinitionRegistry.GetInstance();
            RegisterCommands(registry);

            CommandExecutor exe = registry.GetCommandExecutor(args);
            exe.Execute();
#if DEBUG
            Console.ReadLine();
#endif
        }

        static void RegisterCommands(DefinitionRegistry registry)
        {
            registry.Add(new NamespaceDefinition("htl", "HatTrick Labs"));

            var cmdDef = new CommandDefinition(name: "htl.guid");   
            cmdDef.Handler = Mapper.MapCommand(cmdDef).ToSignature<Action<int, string>>().Then(GenerateGuids);
            cmdDef.AddOption<string>(key: "format", defaultArg: "D", help: "The Guid format specifier.", (terse: "-f", verbose: "--format"));
            cmdDef["format"].AcceptedValues("N", "D", "B", "P", "X");
            cmdDef.AddOption<int>(key: "count", defaultArg: 1, help: "The number of Guids to generate.", (terse: "-c", verbose: "--count"));
            cmdDef["count"].ApplyConstraint<int>((cnt) => cnt > 0 && cnt <= 100, "allowed range", "arg must be within range 1..100.");
            registry.Add(cmdDef);

            cmdDef = new CommandDefinition(name: "htl.base64");
            cmdDef.Handler = (cmd) => 
            {
                string result = cmd["reverse"].Value == true
                ? Encoding.UTF8.GetString(Convert.FromBase64String(cmd["value"].Value))
                : Convert.ToBase64String(Encoding.UTF8.GetBytes(cmd["value"].Value));

                Console.WriteLine(result); 
            };
            cmdDef.AddOption<string>(key: "value", help: "The value to base 64 encode or decode", (terse: "-v", verbose: "--verbose"));
            cmdDef.AddOption<bool>(key: "reverse", defaultArg: false, help: "Reverse the base 64 encoding (decode).", (terse: "-r", verbose: "--reverse"));
            registry.Add(cmdDef);

        }

        static void GenerateGuids(int count, string format)
        {
            for (int i = 0; i < count; i++)
            {
                Console.WriteLine(Guid.NewGuid().ToString(format));
            }
        }
    }
}