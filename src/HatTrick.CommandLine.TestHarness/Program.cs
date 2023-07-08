using System;
using System.IO;
using System.Threading.Tasks;
using HatTrick.CommandLine.Namespace;
using Microsoft.Win32;

namespace HatTrick.CommandLine.TestHarness
{
    internal class Program
    {
        static void Main(string[] args)
        {
            RegisterCommands(out CommandDefinitionRegistry registry);

            registry.ExecuteCommand(CommandParser.Parse(args));

            //TestMaskedInputReader();

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

            CommandDefinition cmd = null;
            NamespaceDefinition ns = null;

            /********** Register Command Namespaces **********/
            ns = new(name: "vault", help: "Namespace for vault secret related commands.");
            registry.Add(ns);

            ns = new(name: "cb", help: "Namespace for all Coinbase related commands.");
            registry.Add(ns);

            ns = new(name: "cb.profiles", help: "Namespace for all Coinbase profile related commands.");
            registry.Add(ns);

            ns = new(name: "cb.products", help: "Namespace for all Coinbase product related commands.");
            registry.Add(ns);

            ns = new(name: "cb.withdrawals", help: "Namespace for all Coinbase withdrawals related commands.");
            registry.Add(ns);

            /********** Register Vault Commands **********/
            cmd = new(name: "vault.set");
            cmd.Help = "Encrypts data into a vault json file.";
            cmd.Handler = (command) => { };
            cmd.AddOption(key: "key", mustAssign: true, help: "Key pointer within vault json file.", type: OpType.String, "-k", "--key");
            cmd.AddOption(key: "value", mustAssign: true, help: "Value to encrypt.", type: OpType.String, "-v", "--value");
            registry.Add(cmd);

            cmd = new(name: "vault.unset");
            cmd.Help = "Remove encrypted data from a vault json file.";
            cmd.Handler = (command) => { };
            cmd.AddOption(key: "key", mustAssign: true, help: "Key pointer within vault json file.", type: OpType.String, "-k", "--key");
            registry.Add(cmd);

            /********** Register REST commands **********/
            cmd = new(name: "cb.profiles.get");
            cmd.Help = "Get Coinbase profiles.";
            cmd.Handler = (command) => { };
            cmd.AddOption(key: "active", mustAssign: false, help: "Active profiles only.", type: OpType.Bool, "-a", "--active");
            registry.Add(cmd);

            cmd = new(name: "cb.profiles.rename.put");
            cmd.Help = "Rename a Coinbase profile.";
            cmd.Handler = (command) => { };
            cmd.AddOption(key: "profile_id", mustAssign: true, help: "The profile id.", type: OpType.Guid, "-p", "--profile-id");
            cmd.AddOption(key: "name", mustAssign: true, help: "The new profile name.", type: OpType.String, "-n", "--name");
            registry.Add(cmd);

            cmd = new(name: "cb.profiles.deactivate.put");
            cmd.Help = "Deactivate a Coinbase profile.";
            cmd.Handler = (command) => { };
            cmd.AddOption(key: "profile_id", mustAssign: true, help: "The profile id.", type: OpType.Guid, "-p", "--profile-id");
            cmd.AddOption(key: "to", mustAssign: true, help: "Profile id all existing funds will be moved to.", type: OpType.Guid, "-t", "--to");
            registry.Add(cmd);

            cmd = new(name: "cb.products.get");
            cmd.Help = "Get Coinbase trading products or 'currency pairs'.";
            cmd.Handler = (command) => { };
            cmd.AddOption(key: "id", mustAssign: false, help: "Filter to specific product id.", type: OpType.String, "-i", "--id");
            registry.Add(cmd);

            cmd = new(name: "cb.products.book.get");
            cmd.Help = "Get a list of open orders for a product order book.";
            cmd.Handler = (command) => { };
            cmd.AddOption(key: "product_id", mustAssign: true, help: "The product id.", type: OpType.String, "-p", "--product-id");
            cmd.AddOption(
                key: "level",
                mustAssign: true,
                help: "The aggregation level (1|2|3). 1: Best bid ask and auction info. 2: Full order book (aggregated) and auction info. 3: Full order book (not aggregated) and auction info.",
                type: OpType.Int32,
                "-l", "--level");
            cmd["level"].SetAccepted(1, 2, 3);
            registry.Add(cmd);

            cmd = new(name: "cb.products.candles.get");
            cmd.Help = "Get historical product rates in grouped buckets or 'Candles'.";
            cmd.Handler = (command) => { };
            cmd.AddOption(key: "product_id", mustAssign: true, help: "The product id.", type: OpType.String, "-p", "--product-id");
            cmd.AddOption(key: "granularity", mustAssign: true, help: "Timeslice granularity (60|300|900|3600|21600|86400) in seconds.", type: OpType.Int32, "-g", "--granularity");
            cmd.AddOption(key: "start", mustAssign: false, help: "Start timestamp for aggregation range.  Ignored if no end timestamp provided.", type: OpType.DateTime, "-s", "--start");
            cmd.AddOption(key: "end", mustAssign: false, help: "End timestamp for aggregation range.  Ignored if no start timestamp provided.", type: OpType.DateTime, "-e", "--end");
            cmd["granularity"].SetAccepted(60, 300, 900, 3600, 21600, 86400);
            cmd["start"].ApplyConstraint<DateTime>((s) => s > DateTime.Now.AddDays(-180), "Arg must be within the past 180 days.");
            cmd["end"].ApplyConstraint<DateTime>((e) => e <= DateTime.Now.Date, "Arg must be less than or equal today's date.");
            registry.Add(cmd);

            cmd = new(name: "cb.products.stats.get");
            cmd.Help = "Gets 30 day and 24 hour stats for a product.";
            cmd.Handler = (command) => { };
            cmd.AddOption(key: "product_id", mustAssign: true, help: "The product id.", type: OpType.String, "-p", "--product-id");
            registry.Add(cmd);

            cmd = new(name: "cb.products.ticker.get");
            cmd.Help = "Gets snapshot information about the last trade 'tick', best bid/ask and 24 hour volume for a Coinbase product.";
            cmd.Handler = (command) => { };
            cmd.AddOption(key: "product_id", mustAssign: true, help: "The product id.", type: OpType.String, "-p", "--product-id");
            registry.Add(cmd);

            cmd = new(name: "cb.products.trades.get");
            cmd.Help = "Gets a list the latest trades for a product.";
            cmd.Handler = (command) => { };
            cmd.AddOption(key: "product_id", mustAssign: true, help: "The product id.", type: OpType.String, "-p", "--product-id");
            cmd.AddOption(key: "limit", mustAssign: false, help: "Limit on number of results to return.", type: OpType.Int32, "-l", "--limit");
            //TODO: add before and after filter options...
            cmd.AddOption(key: "max", mustAssign: false, help: "Max number of trades to request (limit * iteration count).", type: OpType.Int32, "-m", "--max");
            cmd["limit"].SetDefault(200);
            cmd["max"].SetDefault(200);
            registry.Add(cmd);

            cmd = new(name: "cb.products.fills.get");
            cmd.Help = "Get Coinbase product fills.";
            cmd.Handler = (command) => { };
            cmd.AddOption(key: "product_id", mustAssign: true, help: "The product id.", type: OpType.String, "--prod-id", "--product-id");
            cmd.AddOption(key: "profile_id", mustAssign: false, help: "The profile id.", type: OpType.Guid, "--prof-id", "--profile-id");
            cmd.AddOption(key: "limit", mustAssign: false, help: "Limit on number of results to return.", type: OpType.Int32, "-l", "--limit");
            cmd.AddOption(key: "max", mustAssign: false, help: "Max number of fills to request (limit * iteration count).", type: OpType.Int32, "-m", "--max");
            registry.Add(cmd);

            cmd = new(name: "cb.products.orders.delete");
            cmd.Help = "Cancel all open Coinbase orders.";
            cmd.Handler = (command) => { };
            cmd.AddOption(key: "product_id", mustAssign: false, help: "Cancel all open orders for a specific product id.", type: OpType.String, "-p", "--product-id");
            registry.Add(cmd);
        }
    }
}