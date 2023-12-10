using System;
using System.Text;
using System.Threading.Tasks;
using HatTrick.CommandLine.Extensions;

namespace HatTrick.CommandLine.TestHarness
{
    internal class Program
    {
        //string input = "htl.guid -u \"100 00\" --format X --silent:true    -abc=\"d:\\tmp\"  -xyz=abc -efg:-b   --quiet=true --force:true -p \"d:\tmp\abcdefg xyz\" ";
        //static async Task Main(string[] args)
        static void Main(string[] args)
        {
            RegisterCommandDefinitions();
            Command cmd = CommandBuilder.Build(args);
            CommandExecutor exe = DefinitionRegistry.GetInstance().GetCommandExecutor(cmd);
            exe.Execute();
            //await exe.ExecuteAsync();

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
                int count = cmd["count"].GetValue<int>();
                string format = cmd["format"].GetValue<string>();

                GenerateGuids(count, format);
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
                string result = cmd["reverse"].GetValue<bool>() == true
                ? Encoding.UTF8.GetString(Convert.FromBase64String(cmd["value"].GetValue<string>()))
                : Convert.ToBase64String(Encoding.UTF8.GetBytes(cmd["value"].GetValue<string>()));

                Console.WriteLine(result);
            };
            cmdDef.AddOption<string>(key: "value", help: "The value to base 64 encode or decode", (terse: "-v", verbose: "--value"));
            cmdDef.AddOption<bool>(key: "reverse", defaultArg: false, help: "Reverse the base 64 encoding (decode).", (terse: "-r", verbose: "--reverse"));
            DefinitionRegistry.GetInstance().Add(cmdDef);
        }

        static void RegisterFakePersonCommand()
        {
            var reg = DefinitionRegistry.GetInstance();
            var cmdDef = new CommandDefinition("htl.add-person");
            cmdDef.AddOption<string>(key: "first", help: "Person's first name.", ("-f", "--first"));
            cmdDef.AddOption<string>(key: "last", help: "Person's first name.", ("-l", "--last"));
            cmdDef.AddOption<int>(key: "age", help: "Person's first name.", ("-a", "--age"));

            cmdDef.MapTo<Person>(("first","FirstName"),("last","LastName"), ("age","Age")).Then(Person.SavePerson);

            cmdDef.MapToSignature<Action<string, string, int>>(
                ("first", "firstName"),
                ("last", "lastName")
                ).Then(Person.SavePerson);


            cmdDef.Handler += (cmd) =>
            {
                string first = cmd["first"].GetValue<string>();
                string last = cmd["last"].GetValue<string>();
                int age = cmd["age"].GetValue<int>();

                Person.SavePerson(new Person() { FirstName = first, LastName = last, Age = age });
            };

            reg.Add(cmdDef);
        }

        static void GenerateGuids(int count, string format)
        {
            for (int i = 0; i < count; i++)
                Console.WriteLine(Guid.NewGuid().ToString(format));
        }
    }

    public class Person
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }

        public static async Task SavePersonAsync(Person p)
        {
            await System.Threading.Tasks.Task.Run(() => Console.WriteLine(p.FirstName + " " + p.LastName + " " + p.Age));
        }

        public static void SavePerson(Person p)
        {
            Console.WriteLine(p.FirstName + " " + p.LastName + " " + p.Age);
        }

        public static async Task SavePersonAsync(string firstName, string lastName, int age)
        {
            await System.Threading.Tasks.Task.Run(() => Console.WriteLine(firstName + " " + lastName + " " + age));
        }

        public static void SavePerson(string firstName, string lastName, int age)
        {
            Console.WriteLine(firstName + " " + lastName + " " + age);
        }
    }
}