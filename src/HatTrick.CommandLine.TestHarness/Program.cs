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
            RegisterCapsCommand();
            RegisterSumCommand();
        }

        static void RegisterNamespaces()
        {
            DefinitionRegistry.GetInstance().Add(new NamespaceDefinition("htl", "HatTrick Labs namespace"));
        }

        static void RegisterGuidCommand()
        {
            var cmdDef = new CommandDefinition(name: "htl.guid");
            cmdDef.Help = "Generates one or many GUID values.";

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
            cmdDef.Help = "Encodes or decodes string input as base 64.";
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
            cmdDef.Help = "Saves a baseline person entity into the SQL database.";
            cmdDef.AddOption<string>(key: "first", help: "Person's first name.", ("-f", "--first"));
            cmdDef.AddOption<string>(key: "last", help: "Person's first name.", ("-l", "--last"));
            cmdDef.AddOption<int>(key: "age", help: "Person's first name.", ("-a", "--age"));

            cmdDef.ApplyConstraint((cmd) => cmd["age"].GetValue<int>() > 21, "age restriction", "must be older than 21.");

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

        static void RegisterCapsCommand()
        {
            var cmdDef = new CommandDefinition("htl.caps");
            cmdDef.AddOption<string>(key: "value", help: "Value to capitalize.", ("-v", "--value"));
            cmdDef.Handler += (cmd) =>
            {
                string value = cmd["value"].GetValue<string>();
                Console.Write(value?.ToUpper() ?? string.Empty);
            };
            DefinitionRegistry.GetInstance().Add(cmdDef);
        }

        static void RegisterSumCommand()
        {
            Func<string, int[]> split = (v) =>
            {
                string[] vals = v.Split(new char[] { ','  }, StringSplitOptions.RemoveEmptyEntries);
                int[] nums = new int[vals.Length];

                for (int i = 0; i < vals.Length; i++)
                {
                    nums[i] = OptionTypeMap.ParseOptionArgument<int>(vals[i]);
                }

                return nums;
            };
            Func<int[], int> sum = (values) =>
            {
                int sum = 0;
                Array.ForEach(values, (v) => sum += v);
                return sum;
            };
            var cmdDef = new CommandDefinition("htl.sum");
            cmdDef.AddOption<int[]>(key: "values", "Comma delimited list of values to sum.", split, ("-v", "--values"));
            cmdDef.Handler += (cmd) => Console.WriteLine(sum(cmd["values"].GetValue<int[]>()));
            DefinitionRegistry.GetInstance().Add(cmdDef);
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