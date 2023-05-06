using System;
using System.Linq;
using System.Collections.Generic;

namespace Crypto.CommandLine
{
    public class CommandDefinitionRegistry
    {
        #region internals
        private static CommandDefinitionRegistry _instance;
        private static object _lock = new();
        private Dictionary<string, CommandDefinitionNamespace> _namespaces;
        private Dictionary<string, CommandDefinition> _definitions;
        #endregion

        #region interface
        public CommandDefinition this[string key]
        {
            get { lock(_lock) { return _definitions[key]; } }
            set { lock (_lock) { _definitions[key] = value; } }
        }

        public int Count => _definitions.Count;
        #endregion

        #region constructors
        private CommandDefinitionRegistry()
        {
            _definitions = new Dictionary<string, CommandDefinition>();
            _namespaces = new Dictionary<string, CommandDefinitionNamespace>();
        }
        #endregion

        #region get instance
        public static CommandDefinitionRegistry GetInstance()
        {
            lock (_lock)
            {
                return _instance is null ? _instance = new CommandDefinitionRegistry() : _instance;
            }
        }
        #endregion

        #region get all
        public CommandDefinition[] GetAllCommandDefinitions()
        {
            return _definitions.Values.ToArray();
        }
        #endregion

        #region add
        public void Add(CommandDefinitionNamespace commandNamespace)
        {
            commandNamespace.Validate();
            _namespaces.Add(commandNamespace.Name, commandNamespace);
        }

        public void Add(CommandDefinition commandDef)
        {
            commandDef.Validate();
            _definitions.Add(commandDef.Name, commandDef);
        }
        #endregion

        #region get command definition namespace
        public CommandDefinitionNamespace GetNamespace(string name)
        {
            return _namespaces[name];
        }
        #endregion

        #region get command definition
        public CommandDefinition GetCommmandDefinition(string key)
        {
            return _definitions.ContainsKey(key) 
                ? _definitions[key] 
                : throw new CommandInputException($"No command registered for provided key: {key}");
        }
        #endregion

        #region execute command
        public void ExecuteCommand(Command command)
        {
            if (command.Key is null || string.IsNullOrEmpty(command.Key))
                throw new CommandInputException("No command provided.");

            CommandDefinition cmdDef = this.GetCommmandDefinition(command.Key);

            this.EnsureCommand(command, cmdDef);

            cmdDef.EntryPoint(command);
        }
        #endregion

        #region ensure command
        private void EnsureCommand(Command command, CommandDefinition cmdDef)
        {
            List<string> feedback = new List<string>();

            //ensure fully hydrated
            foreach (var opDef in cmdDef.Options)
            {
                var op = command.Options.FirstOrDefault(o => opDef.Flags.Contains(o.Flag));

                if (op is not null)
                    op.SetKey(opDef.Key);
                else
                    command.Options.Add(CommandOption.GetEmptyInstance(opDef.Key));
            }

            this.EnsureCommandOptions(cmdDef, command, ref feedback);

            if (feedback.Count > 0)
                throw new CommandInputException(feedback.ToArray());
        }
        #endregion

        #region ensure command options
        private void EnsureCommandOptions(CommandDefinition cmdDef, Command cmd, ref List<string> feedback)
        {
            this.EnsureAllInputCommandOptionsDefined(cmdDef, cmd, ref feedback);

            this.EnsureNoDuplicateOptions(cmdDef, cmd, ref feedback);

            for (int i = 0; i < cmdDef.Options.Count; i++)
            {
                var opDef = cmdDef.Options[i];
                var op = cmd.Options?.FirstOrDefault(o => opDef.Key == o.Key);

                this.EnsureMustAssignOptionsProvidedAndAssigned(opDef, op, ref feedback);

                if (op is not EmptyCommandOption)
                {
                    //utilize the typed option definition to convert the command line arg (string) into T and set the typed option value
                    if (!opDef.TrySetOptionValue(op))
                    {
                        string name = opDef.GetType().GetGenericArguments()[0].Name;
                        feedback.Add($"Option '{op.Flag}' requires an argument of type '{name}'...invalid value provided: '{op.Argument}'");
                    }
                }
            }   
        }
        #endregion

        #region ensure no duplicate options
        private void EnsureNoDuplicateOptions(CommandDefinition cmdDef, Command cmd, ref List<string> feedback)
        {
            //TODO: to allow dupes or not to allow dupes ... should dupes be utilized for SIMPLE multi imput scenarios (array of items) without
            //the need to define a custom type converter ??? ex: abc.exe get.dir.file.count --path d:\tmp --path d:\img --path d:\my-pics
            //ensure no dupes
            for (int i = 0; i < cmdDef.Options.Count; i++)
            {
                CommandOptionDefinition opDef = cmdDef.Options[i];
                var opUno = cmd.Options?.FirstOrDefault(o => opDef.Flags.Contains(o.Flag));
                for (int j = 0; j < cmd.Options.Count; j++)
                {
                    CommandOption opDos = cmd.Options[j];

                    if (opDos == opUno)
                        continue;

                    if (opDef.Flags.Contains(opDos.Flag))
                        feedback.Add($"Duplicate options provided at positions: {i + 1} and {j + 1}...'{opUno.Flag}' and '{opDos.Flag}'");
                }
            }
        }
        #endregion

        #region ensure must assign options provided and assigned
        private void EnsureMustAssignOptionsProvidedAndAssigned(CommandOptionDefinition opDef, CommandOption op, ref List<string> feedback)
        {
            if (opDef.MustAssign)
            {
                if (op is EmptyCommandOption)
                    feedback.Add($"Expected option [{string.Join("|", opDef.Flags)}] not found...option is marked '{nameof(CommandOptionDefinition.MustAssign)}'");

                else if (string.IsNullOrEmpty(op.Argument))
                    feedback.Add($"Option '{op.Flag}' requires an argument...no argument provided.");
            }
        }
        #endregion

        #region ensure all input command options defined
        private void EnsureAllInputCommandOptionsDefined(CommandDefinition cmdDef, Command cmd, ref List<string> feedback)
        {
            if ((cmdDef.Options is null || cmdDef.Options.Count == 0) && cmd.Options.Count > 0)
                feedback.Add($"The '{cmdDef.Name}' command does not accept any options...provided options are invalid.");

            //if any options are defined for the command, confirm each option provided is valid
            for (int i = 0; i < cmd.Options.Count; i++)
            {
                var op = cmd.Options[i];

                //empty ops are added to simply fully hydrate all op keys, flags will be null...they are definitely defined.
                if (op is EmptyCommandOption)
                    continue;

                if (!cmdDef.Options.Exists(o => o.Flags.Contains(op.Flag)))
                    feedback.Add($"Undefined option at position: {i + 1} ... option: {op.Flag}");
            }
        }
        #endregion
    }
}
