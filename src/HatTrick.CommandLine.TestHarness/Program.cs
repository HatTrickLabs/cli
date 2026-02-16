using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using HatTrick.CommandLine.Extensions;

namespace HatTrick.CommandLine.TestHarness
{
    internal class Program
    {
        //string input = "htl.guid -u \"100 00\" --format X --silent:true    -abc=\"d:\\tmp\"  -xyz=abc -efg=-b   --quiet=true --force:true -p \"d:\tmp\abcdefg xyz\" ";

        #region main
        //static async Task Main(string[] args)
        static void Main(string[] args)
        {
            RegisterCommandDefinitions();
            Command cmd = CommandBuilder.Build(args);
            CommandExecutor exe = DefinitionRegistry.GetInstance().GetCommandExecutor(cmd);
            try
            {
                exe.Execute();
                //await exe.ExecuteAsync();
            }
            finally
            {
                //TODO: clean up any state that is IDisposable
                //a?.Dispose();
                //b?.Dispose();
                //c?.Dispose();
            }

#if DEBUG
            Console.ReadLine();
#endif
        }
        #endregion

        #region register command defintions
        static void RegisterCommandDefinitions()
        {
            RegisterNamespaces();
            RegisterGuidCommand();
            RegisterBase64Command();
            RegisterFakePersonCommand();
            RegisterCapsCommand();
            RegisterSumCommand();
            RegisterAddCommand();
            RegisterSnapshotListCommand();
        }
        #endregion

        #region register namespaces
        static void RegisterNamespaces()
        {
            DefinitionRegistry.GetInstance().Add(new NamespaceDefinition("htl", "HatTrick Labs namespace"));
        }
        #endregion

        #region register guid command
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
            cmdDef["count"].ApplyConstraint<int>((cnt) => cnt > 0 && cnt <= 100, "Allowed Range", "1..100.");

            DefinitionRegistry.GetInstance().Add(cmdDef);
        }
        #endregion

        #region register base64 command
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
        #endregion

        #region register fake person command
        static void RegisterFakePersonCommand()
        {
            var reg = DefinitionRegistry.GetInstance();
            var cmdDef = new CommandDefinition("htl.add-person");
            cmdDef.Help = "Saves a baseline person entity into the SQL database.";
            cmdDef.AddOption<string>(key: "first", help: "Person's first name.", ("-f", "--first"));
            cmdDef.AddOption<string>(key: "last", help: "Person's first name.", ("-l", "--last"));
            cmdDef.AddOption<int>(key: "age", help: "Person's first name.", ("-a", "--age"));

            cmdDef.ApplyConstraint((cmd) => cmd["age"].GetValue<int>() > 21, "Age Restriction", "must be older than 21.");

            cmdDef.MapTo<Person>(("first","FirstName"),("last","LastName"), ("age","Age")).Then(Person.SavePerson);

            cmdDef.MapTo<Person>(("first", "FirstName"), ("last", "LastName"), ("age", "Age")).Then((p) => Person.SavePerson(p));

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
        #endregion

        #region register caps command
        static void RegisterCapsCommand()
        {
            var cmdDef = new CommandDefinition("htl.caps");
            cmdDef.Help = "Capitalize input.";
            cmdDef.AddOption<string>(key: "value", help: "Value to capitalize.", ("-v", "--value"));
            cmdDef.Handler += (cmd) =>
            {
                string value = cmd["value"].GetValue<string>();
                Console.Write(value?.ToUpper() ?? string.Empty);
            };
            DefinitionRegistry.GetInstance().Add(cmdDef);
        }
        #endregion

        #region register sum command
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
            cmdDef.Help = "Sum input.";
            cmdDef.AddOption<int[]>(key: "values", "Comma delimited list of values to sum.", split, ("-v", "--values"));
            cmdDef.Handler += (cmd) => Console.WriteLine(sum(cmd["values"].GetValue<int[]>()));
            DefinitionRegistry.GetInstance().Add(cmdDef);
        }
        #endregion

        #region register guids command
        static void GenerateGuids(int count, string format)
        {
            for (int i = 0; i < count; i++)
                Console.WriteLine(Guid.NewGuid().ToString(format));
        }
        #endregion

        #region register add command
        private static void RegisterAddCommand()
        {
            var cmdDef = new CommandDefinition(name: "add");
            cmdDef.Help = "Adds a new time entry record.";
            cmdDef.Handler = (cmd) =>
            {
                var billTO = cmd["bill-to"].GetValue<string>();
                var day = cmd["day"].GetValue<DateOnly>();
                var hrs = cmd["hours"].GetValue<double>();
                var com = cmd["comment"].GetValue<string>();
            };
            cmdDef.AddOption<string>(key: "bill-to", help: "Bill to client key (case insensitive).", (terse: "-b", verbose: "--bill-to"));
            cmdDef["bill-to"].AcceptedValues("apa", "ms");
            cmdDef.AddOption<double>(
                key: "hours", 
                help: "Billable hours for time entry.", 
                flags: (terse: "-h", verbose: "--hours")
            );
            cmdDef["hours"].ApplyConstraint<double>(
                constraint: (hours) => hours > 0,
                name: "Min Value",
                description: "Miminum hours value is 0.5."
            );
            cmdDef["hours"].ApplyConstraint<double>(
                constraint: (hours) => 2 * hours == (int)(2 * hours) >> 0,
                name: "Valid Increment",
                description: "Argument must be a floating point value of 0.5 (half hour) increments."
            );

            cmdDef.AddOption<DateOnly>(
                key: "day", 
                defaultArg: (() => DateOnly.FromDateTime(DateTime.Now), "Current date."), 
                help: "Date of billable hours for time entry.", 
                flags: (terse: "-d", "--day")
            );
            cmdDef.AddOption<string>(
                key: "comment", 
                help: "Comment releated to billable hours for time entry.", 
                flags: (terse: "-c", verbose: "--comment")
            );
            cmdDef["comment"].ApplyConstraint<string>(
                constraint: (comment) => !string.IsNullOrWhiteSpace(comment), 
                name: "Not Whitespace", 
                description: "Comment must contain a non-whitespace value."
            );

            cmdDef.OnPreEnsure = (preCmd) =>
            {
                var dayOp = preCmd.GetOption(o => o.Flag == "-d" || o.Flag == "--day");
                if (dayOp is not null)
                {
                    if (int.TryParse(dayOp.Argument, out int shift))
                    {
                        var today = DateOnly.FromDateTime(DateTime.Now);
                        var shifted = today.AddDays(shift);
                        dayOp.OverrideArgument(shifted.ToString("yyyy-MM-dd"));
                    }
                }
            };

            DefinitionRegistry.GetInstance().Add(cmdDef);
        }
        #endregion

        #region register snapshot list command
        private static void RegisterSnapshotListCommand()
        {
            var cmdDef = new CommandDefinition(name: "snapshot-list");
            cmdDef.Help = "Retrieve and render locally persisted directory tree snapshots.";
            cmdDef.Handler = (cmd) => {
                var path = cmd["path"].GetValue<string>();
                var per = cmd["period"].GetValue<string>();
            };
            cmdDef.AddOption<string>(key: "path", help: "Absolute root path of a local directory (directory tree root).", (terse: "-p", verbose: "--path"));
            cmdDef.AddOption<string>(key: "period", defaultArg: "all", help: "Filter for snapshot build/refresh timestamp period (all, day, week to date, month to date, year to date).", (terse: null, verbose: "--period"));
            cmdDef["path"].ApplyConstraint<string>(constraint: (path) => System.IO.Path.IsPathFullyQualified(path), name: "Full Qualified Path", description: "Argument must be a valid fully qualified directory path on the local machine.");
            cmdDef["period"].AcceptedValues<string>("all", "day", "wtd", "mtd", "ytd");

            DefinitionRegistry.GetInstance().Add(cmdDef);
        }
        #endregion

        #region register configured pass through commands
        static void RegisterConfiguredPassThroughCommands()
        {
            var cmdDef = new CommandDefinition(name: "chrome.exe");
            cmdDef.Help = "";
            cmdDef.Handler = (cmd) => System.Diagnostics.Process.Start("chrome.exe");
            cmdDef.AddOption<string>(key: "url", defaultArg: null, help: "", (terse: "-u", verbose: "--url"));
            cmdDef.AddOption<bool>(key: "incognito", defaultArg: true, help: "", (terse: null, verbose: "--incognito"));
            cmdDef.AddOption<bool>(key: "new-window", defaultArg: false, help: "", (terse: null, verbose: "--new-window"));
        }
        #endregion
    }

    #region person [class]
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
            Console.WriteLine(firstName + " " + "'" + lastName + "'" + " " + age);
        }
    }
    #endregion
}
