using System;
using System.Data.Common;
using HatTrick.CommandLine.Parsing;

namespace HatTrick.CommandLine.TestHarness
{
    internal class Program
    {
        static void Main(string[] args)
        {
            RegisterCommands(out Registry registry);
            registry.ExecuteCommand(Parser.Parse(args));

            //TestMaskedInputReader();

            Console.WriteLine("Press [Enter] to exit.");
            Console.ReadLine();
        }

        static void TestMaskedInputReader()
        {
            string input = new MaskedInputLineReader().ReadMaskedInput();
            Console.WriteLine(input);
        }

        static void RegisterCommands(out Registry registry)
        {
            registry = Registry.GetInstance();

            CommandDefinition cmd = null;

            /********** Register Command Namespaces **********/
            registry.Add(new NamespaceDefinition(name: "vault", help: "Namespace for vault secret related commands."));
            registry.Add(new NamespaceDefinition(name: "cb", help: "Namespace for all Coinbase related commands."));
            registry.Add(new NamespaceDefinition(name: "cb.profiles", help: "Namespace for all Coinbase profile related commands."));
            registry.Add(new NamespaceDefinition(name: "cb.products", help: "Namespace for all Coinbase product related commands."));
            registry.Add(new NamespaceDefinition(name: "cb.withdrawals", help: "Namespace for all Coinbase withdrawals related commands."));

            cmd = new(name: "test");
            cmd.Help = "Test command help.";
            cmd.Handler = Mapper.MapCommand(cmd).To<Person>(
                ("first_name", "FirstName"),
                ("last_name", "LastName"),
                ("birth_date", "BirthDate"),
                ("uuid", "Code"),
                ("score", "~")
                ).Then(PersonService.Add);

            cmd.Handler += Mapper.MapCommand(cmd).To<Person>(
                ("first_name", "FirstName"),
                ("last_name", "LastName"),
                ("birth_date", "BirthDate"), 
                ("uuid", "Code"),
                ("score", "~")
                ).Then((p) => Console.WriteLine(p.FirstName + " " + p.LastName + " " + p.Code + " " + p.Score));

            cmd.Handler += Mapper.MapCommand(cmd).ToSignature<Action<string, string, DateOnly?, Guid?>>(
                ("first_name", "firstName"),
                ("last_name", "lastName"),
                ("birth_date", "birthDate"),
                ("uuid", "code"),
                ("score", "~")
                ).Then(PersonService.AddPerson);

            cmd.AddOption<string>(key: "first_name", help: "First name.", flags: ("-f", "--first-name"));
            cmd.AddOption<string>(key: "last_name", help: "Last name.", flags: ("-l", "--last-name"));
            cmd.AddOption<DateOnly?>(key: "birth_date", defaultArg: null, help: "Birth date.", flags: ("-b", "--birth-date"));
            cmd.AddOption<Guid?>(key: "uuid", defaultArg: null, help: "Some long cryptic code.", flags: ("-u", "--uuid"));
            cmd.AddOption<int>(key: "score", defaultArg: 9, help: "score", flags: ("-s", "--score"));
            cmd["score"].AcceptedValues(1, 2, 3, 4, 5, 6, 7, 8);
            registry.Add(cmd);

            /********** Register Vault Commands **********/
            cmd = new(name: "vault.set");
            cmd.Help = "Encrypts data into a vault json file.";
            cmd.Handler = (command) => { };
            cmd.AddOption<string>(key: "key", help: "Key pointer within vault json file.", flags: ("-k", "--key"));
            cmd.AddOption<string>(key: "value", help: "Value to encrypt.", flags: ("-v", "--value"));
            registry.Add(cmd);

            cmd = new(name: "vault.unset");
            cmd.Help = "Remove encrypted data from a vault json file.";
            cmd.Handler = (command) => { };
            cmd.AddOption<string>(key: "key", help: "Key pointer within vault json file.", flags: ("-k", "--key"));
            registry.Add(cmd);

            /********** Register REST commands **********/
            cmd = new(name: "cb.profiles.get");
            cmd.Help = "Get Coinbase profiles.";
            cmd.Handler = (command) => { };
            cmd.AddOption<string>(key: "active", help: "Active profiles only.", flags: ("-a", "--active"));
            registry.Add(cmd);

            cmd = new(name: "cb.profiles.rename.put");
            cmd.Help = "Rename a Coinbase profile.";
            cmd.Handler = (command) => { };
            cmd.AddOption<string>(key: "profile_id", help: "The profile id.", flags: ("-p", "--profile-id"));
            cmd.AddOption<string>(key: "name", help: "The new profile name.", flags: ("-n", "--name"));
            registry.Add(cmd);

            cmd = new(name: "cb.profiles.deactivate.put");
            cmd.Help = "Deactivate a Coinbase profile.";
            cmd.Handler = (command) => { };
            cmd.AddOption<Guid>(key: "profile_id", help: "The profile id.", flags: ("-p", "--profile-id"));
            cmd.AddOption<Guid>(key: "to", help: "Profile id all existing funds will be moved to.", flags: ("-t", "--to"));
            registry.Add(cmd);

            cmd = new(name: "cb.products.get");
            cmd.Help = "Get Coinbase trading products or 'currency pairs'.";
            cmd.Handler = (command) => { };
            cmd.AddOption<string>(key: "id", help: "Filter to specific product id.", flags: ("-i", "--id"));
            registry.Add(cmd);

            cmd = new(name: "cb.products.book.get");
            cmd.Help = "Get a list of open orders for a product order book.";
            cmd.Handler = (command) => { };
            cmd.AddOption<string>(key: "product_id", help: "The product id.", flags: ("-p", "--product-id"));
            cmd.AddOption<int>(
                key: "level",
                help: "The aggregation level (1|2|3). 1: Best bid ask and auction info. 2: Full order book (aggregated) and auction info. 3: Full order book (not aggregated) and auction info.",
                flags: ("-l", "--level"));
            cmd["level"].AcceptedValues(1, 2, 3);
            registry.Add(cmd);

            cmd = new(name: "cb.products.candles.get");
            cmd.Help = "Get historical product rates in grouped buckets or 'Candles'.";
            cmd.Handler = (command) => { };
            cmd.AddOption<string>(key: "product_id", help: "The product id.", flags: ("-p", "--product-id"));
            cmd.AddOption<int>(key: "granularity", help: "Timeslice granularity (60|300|900|3600|21600|86400) in seconds.", flags: ("-g", "--granularity"));
            cmd.AddOption<DateTime>(key: "start", help: "Start timestamp for aggregation range.  Ignored if no end timestamp provided.", flags: ("-s", "--start"));
            cmd.AddOption<DateTime>(key: "end", help: "End timestamp for aggregation range.  Ignored if no start timestamp provided.", flags: ("-e", "--end"));
            cmd["granularity"].AcceptedValues(60, 300, 900, 3600, 21600, 86400);
            cmd["start"].ApplyConstraint<DateTime>(
                constraint: (s) => s > DateTime.Now.AddDays(-180), 
                name: "constraint",
                description: "Arg must be within the past 180 days.");
            cmd["end"].ApplyConstraint<DateTime>(
                constraint: (e) => e <= DateTime.Now.Date,
                name: "constraint",
                description: "less than or equal today's date");
            registry.Add(cmd);

            cmd = new(name: "cb.products.stats.get");
            cmd.Help = "Gets 30 day and 24 hour stats for a product.";
            cmd.Handler = (command) => { };
            cmd.AddOption<string>(key: "product_id", help: "The product id.", flags: ("-p", "--product-id"));
            registry.Add(cmd);

            cmd = new(name: "cb.products.ticker.get");
            cmd.Help = "Gets snapshot information about the last trade 'tick', best bid/ask and 24 hour volume for a Coinbase product.";
            cmd.Handler = (command) => { };
            cmd.AddOption<string>(key: "product_id", help: "The product id.", flags: ("-p", "--product-id"));
            registry.Add(cmd);

            cmd = new(name: "cb.products.trades.get");
            cmd.Help = "Gets a list the latest trades for a product.";
            cmd.Handler = (command) => { };
            cmd.AddOption<string>(key: "product_id", help: "The product id.", flags: ("-p", "--product-id"));
            cmd.AddOption<int>(key: "limit", defaultArg: 100, help: "Limit on number of results to return.", flags: ("-l", "--limit"));
            //TODO: add before and after filter options...
            cmd.AddOption<int>(key: "max", defaultArg: 200, help: "Max number of trades to request (limit * iteration count).", flags: ("-m", "--max"));
            cmd["max"].ApplyConstraint<int>((m) => m <= 1000, "max value", "arg must be <= 1000");

            cmd.ApplyConstraint(
                constraint: (command) => command["limit"].Value <= command["max"].Value,
                name: "constraint",
                description: "limit must be <= max"
                );

            registry.Add(cmd);

            cmd = new(name: "cb.products.fills.get");
            cmd.Help = "Get Coinbase product fills.";
            cmd.Handler = (command) => { };
            cmd.AddOption<string>(key: "product_id", defaultArg: null, help: "The product id.", flags: ("--prod-id", "--product-id"));
            cmd.AddOption<Guid?>(key: "profile_id", defaultArg: null, help: "The profile id.", flags: ("--prof-id", "--profile-id"));
            cmd.AddOption<int>(key: "limit", defaultArg: 100, help: "Limit on number of results to return.", flags: ("-l", "--limit"));
            cmd.AddOption<int>(key: "max", defaultArg: 500, help: "Max number of fills to request (limit * iteration count).", flags: ("-m", "--max"));
            cmd["max"].ApplyConstraint<int>((m) => m <= 1000, "max value", "arg must be <= 1000");
            cmd.MustAssignOneOf("product_id", "profile_id");
            registry.Add(cmd);

            cmd = new(name: "cb.products.orders.delete");
            cmd.Help = "Cancel all open Coinbase orders.";
            cmd.Handler = (command) => { };
            cmd.AddOption<string>(key: "product_id", help: "Cancel all open orders for a specific product id.", flags:  ("-p", "--product-id"));
            registry.Add(cmd);
        }
    }
}