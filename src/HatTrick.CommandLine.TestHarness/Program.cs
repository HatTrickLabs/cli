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
            User u = new User() { FirstName = "Charlie", LastName = "Brown" };


            Mapper.MapCommand(null).To<User>().Then(UserService.SaveUser);
            Mapper.MapCommand(null).ToSignature<Action<string, string>>().Then(UserService.SaveUser);



            return;
            var registry = DefinitionRegistry.GetInstance();

            RegisterCommands(registry);

            Command cmd = CommandParser.Parse(args);
            registry.ExecuteCommand(cmd);

            Console.ReadLine();
        }

        static void RegisterCommands(DefinitionRegistry registry)
        {
            registry.Add(new NamespaceDefinition("htl", "HatTrick Labs"));

            var cmdDef = new CommandDefinition(name: "htl.guid");

            //cmdDef.Handler = (cmd) => GenerateGuids(cmd["count"].Value, cmd["format"].Value);

            cmdDef.Handler = Mapper.MapCommand(cmdDef).ToSignature<Action<int, string>>().Then(GenerateGuids);

            cmdDef.AddOption<string>(key: "format", defaultArg: "D", help: "The Guid format specifier.", ("-f", "--format"));
            cmdDef["format"].AcceptedValues("N", "D", "B", "P", "X");

            cmdDef.AddOption<int>(key: "count", defaultArg: 1, help: "The number of Guids to generate.", (null, "--count"));
            cmdDef["count"].ApplyConstraint<int>((cnt) => cnt > 0 && cnt <= 100, "allowed range", "arg must be within range 1..100.");

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

    public class UserService
    {
        public static void SaveUser(string firstName, string lastName)
        {
        }

        public static void SaveUser(User user)
        {
        }
    }

    public class User
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}