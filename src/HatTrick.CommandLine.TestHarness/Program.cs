using System;
using System.Threading.Tasks;
using HatTrick.CommandLine;
using Microsoft.Win32;

namespace HatTrick.CommandLine.TestHarness
{
    internal class Program
    {
        static void Main(string[] args)
        {
            TestMaskedInputReader();

            Console.WriteLine("Press [Enter] to exit.");
            Console.ReadLine();
        }

        static void TestMaskedInputReader()
        {
            string input = new MaskedInputLineReader().ReadMaskedInput();
            Console.WriteLine(input);
        }

        static void RegisterCommands(out CommandDefinitionRegistry registry)
        {
            registry = CommandDefinitionRegistry.GetInstance();

            DefaultCommandDefinition defCmd = null;
            CommandDefinition cmd = null;
            CommandDefinitionNamespace ns = null;

            /********** Register Default Command **********/
            defCmd = new();
            defCmd.Help = "???";
            defCmd.Handler = (c) => { };
            defCmd.AddOption(key: "help", mustAssign: false, help: "Display help.", type: OpType.Bool, "-?", "-h", "--help");
            defCmd.AddOption(key: "version", mustAssign: false, help: "Display crypto.exe version information.", type: OpType.Bool, "-v", "--version");
            defCmd.AddOption(key: "run", mustAssign: false, help: "Run crypto.exe in a non-exiting command loop.", type: OpType.Bool, "-r", "--run");
            registry.Add(defCmd);


            cmd = new(name: "xxx");
            cmd.Help = "Help xxx.";
            cmd.Handler = (sc) =>{ };

            cmd.AsyncHandler = Mapper.MapCommand(cmd).To<Person>(
                    ("first_name", "FirstName"),
                    ("last_name", "LastName"),
                    ("birth_date", "BirthDate")
                ).ThenAsync(PersonService.AddAsync);

            cmd.Handler = Mapper.MapCommand(cmd).To<Person>(
                ("first_name", "FirstName"),
                ("last_name", "LastName"),
                ("birth_date", "BirthDate")
                ).Then(PersonService.Add);

            cmd.Handler += Mapper.MapCommand(cmd).ToSignature<Action<string, string, DateTime>>(
                ("first_name", "firstName"),
                ("last_name", "lastName"),
                ("birth_date", "birthDate")
                ).Then(PersonService.AddPerson);

            cmd.AsyncHandler += Mapper.MapCommand(cmd).ToSignature<Func<string, string, DateTime, Task>>(
                ("first_name", "firstName"),
                ("last_name", "lastName"),
                ("birth_date", "birthDate")
                ).ThenAsync(PersonService.AddPersonAsync);

            cmd.Handler += Mapper.MapCommand(cmd).ToSignature<Action<string, string, DateTime>>(
                ("first_name", "firstName"),
                ("last_name", "lastName"),
                ("birth_date", "birthDate")
                ).Then((firstName, lastName, birthDate) => Console.WriteLine($"Person: {lastName}, {firstName} born on {birthDate.Date.ToString("yyyy-MM-dd")}"));

            cmd.Handler += Mapper.MapCommand(cmd).ToSignature<Action<string, string, DateTime?>>(
                ("first_name", "firstName"),
                ("last_name", "lastName"),
                ("birth_date", "birthDate")
                ).Then(PersonService.AddPesonTest);

            cmd.AddOption(key: "first_name", mustAssign: true, help: "Person's first name.", type: OpType.String, "-f", "--first");
            cmd.AddOption(key: "birth_date", mustAssign: false, help: "Person's birth date.", type: OpType.DateTime, "--dob");
            cmd.AddOption(key: "last_name", mustAssign: true, help: "Person's last name.", type: OpType.String, "-l", "--last");
            //cmd.AddOption(key: "ProfileId", mustAssign: false, help: "Coinbase profile Id.", type: OpType.Guid, "--profile-id");
            //cmd.AddOption(key: "ProductId", mustAssign: false, help: "Coinbase crypto currencty product Id.", type: OpType.String, "--product-id");
            //cmd.AddOption(key: "Amount", mustAssign: true, help: "Amount help.", type: OpType.Double, "-a", "--amount");
            //cmd.AddOptionOf<LogLevel>(key: "LogLevel", help: "Log level help.", converter: (ll) => Enum.Parse<LogLevel>(ll), "-l", "--log-level");
            //cmd.MustAssignOneOf(mutuallyExclusive: false, "profile_id", "product_id");
            //cmd["ProductId"].SetAccepted("BTC-USD", "ETH-USD");
            //cmd["LogLevel"].SetAccepted(Enum.GetValues<LogLevel>());
            //cmd["LogLevel"].SetDefault(LogLevel.Info);
            //cmd["Amount"].ApplyConstraint((double amt) => amt < 1_000, "Argument '{option-key}' provided for option flag '{option-flag}' cannot exceed 1,000.00");
            //option-key, option-flag (the one provided), option-argument (the one provided) 
            registry.Add(cmd);

        }
    }
}