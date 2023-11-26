using System;
using System.Text;
using HatTrick.CommandLine.Extensions;

namespace HatTrick.CommandLine.TestHarness
{
    internal class Program
    {
        //string input = "htl.guid -u \"100 00\" --format X --silent:true    -abc=\"d:\\tmp\"  -xyz=abc -efg:-b   --quiet=true --force:true -p \"d:\tmp\abcdefg xyz\" ";
        //static void Main(string[] args)
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            RegisterCommandDefinitions();
            Command cmd = CommandBuilder.Build(args);
            CommandExecutor exe = DefinitionRegistry.GetInstance().GetCommandExecutor(cmd);
            //exe.Execute();
            await exe.ExecuteAsync();

#if DEBUG
            Console.ReadLine();
#endif
        }

        static void RegisterCommandDefinitions()
        {
            RegisterNamespaces();
            RegisterGuidCommand();
            RegisterBase64Command();
            RegisterFakePersonCommand();
        }

        static void RegisterNamespaces()
        {
            DefinitionRegistry.GetInstance().Add(new NamespaceDefinition("htl", "HatTrick Labs namespace"));
        }

        static void RegisterGuidCommand()
        {
            var cmdDef = new CommandDefinition(name: "htl.guid");
            cmdDef.Help = "Generate one or many GUID values.";

            cmdDef.Handler = (cmd) =>
            {
                for (int i = 0; i < cmd["count"].Value; i++)
                    Console.WriteLine(Guid.NewGuid().ToString(cmd["format"].Value));
            };
            cmdDef.AddOption<string>(key: "format", defaultArg: "D", help: "The Guid format specifier.", (terse: "-f", verbose: "--format"));
            cmdDef["format"].AcceptedValues("N", "D", "B", "P", "X");
            cmdDef.AddOption<int>(key: "count", defaultArg: 1, help: "The number of Guids to generate.", (terse: "-c", verbose: "--count"));
            cmdDef["count"].ApplyConstraint<int>((cnt) => cnt > 0 && cnt <= 100, "allowed range", "1..100.");
            DefinitionRegistry.GetInstance().Add(cmdDef);
        }

        static void RegisterBase64Command()
        {
            var cmdDef = new CommandDefinition(name: "htl.base64");
            cmdDef.Handler = (cmd) =>
            {
                string result = cmd["reverse"].Value == true
                ? Encoding.UTF8.GetString(Convert.FromBase64String(cmd["value"].Value))
                : Convert.ToBase64String(Encoding.UTF8.GetBytes(cmd["value"].Value));

                Console.WriteLine(result);
            };
            cmdDef.AddOption<string>(key: "value", help: "The value to base 64 encode or decode", (terse: "-v", verbose: "--verbose"));
            cmdDef.AddOption<bool>(key: "reverse", defaultArg: false, help: "Reverse the base 64 encoding (decode).", (terse: "-r", verbose: "--reverse"));
            DefinitionRegistry.GetInstance().Add(cmdDef);
        }

        static void RegisterFakePersonCommand()
        {
            var reg = DefinitionRegistry.GetInstance();
            var cmdDef = new CommandDefinition("add-person");
            cmdDef.AddOption<string>(key: "first", help: "Person's first name.", ("-f", "--first"));
            cmdDef.AddOption<string>(key: "last", help: "Person's first name.", ("-l", "--last"));
            cmdDef.AddOption<int>(key: "age", help: "Person's first name.", ("-a", "--age"));
            cmdDef.MapTo<Person>(
                ("first", "FirstName"),
                ("last", "LastName"),
                ("age", "Age"))
                .Then(Person.SavePerson);


            cmdDef.MapTo<Person>(
                ("first", "FirstName"),
                ("last", "LastName"),
                ("age", "Age")
            ).ThenAsync(Person.SavePersonAsync);

            cmdDef.MapToSignature<Func<string, string, int, System.Threading.Tasks.Task>>(
                ("first", "firstName"), 
                ("last", "lastName")
            ).ThenAsync(Person.SavePersonAsync);

            cmdDef.MapToSignature<Action<string, string, int>>(
                ("first", "firstName"),
                ("last", "lastName")
            ).Then(Person.SavePerson);

            cmdDef.Handler += (cmd) =>
            {
                string first = cmd["first"].Value;
                string last = cmd["last"].Value;
                int age = cmd["age"].Value;

                Person.SavePerson(new Person() { FirstName = first, LastName = last, Age = age });
            };

            reg.Add(cmdDef);
        }
    }

    public class Person
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }

        public static async System.Threading.Tasks.Task SavePersonAsync(Person p)
        {
            await System.Threading.Tasks.Task.Run(() => Console.WriteLine(p.FirstName + " " + p.LastName + " " + p.Age));
        }

        public static void SavePerson(Person p)
        {
            Console.WriteLine(p.FirstName + " " + p.LastName + " " + p.Age);
        }

        public static async System.Threading.Tasks.Task SavePersonAsync(string firstName, string lastName, int age)
        {
            await System.Threading.Tasks.Task.Run(() => Console.WriteLine(firstName + " " + lastName + " " + age));
        }

        public static void SavePerson(string firstName, string lastName, int age)
        {
            Console.WriteLine(firstName + " " + lastName + " " + age);
        }
    }
}