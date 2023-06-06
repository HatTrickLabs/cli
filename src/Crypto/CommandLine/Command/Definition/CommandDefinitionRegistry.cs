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
            get { lock (_lock) { return _definitions[key]; } }
            set { lock (_lock) { _definitions[key] = value; } }
        }

        public int DefinitionCount => _definitions.Count;

        public int NamespaceCount => _namespaces.Count;
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

        #region get all command definitions
        public CommandDefinition[] GetAllDefinitions()
        {
            return _definitions.Values.ToArray();
        }
        #endregion

        #region get all namespaces
        public CommandDefinitionNamespace[] GetAllNamespaces()
        {
            return _namespaces.Values.ToArray();
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

        #region get namespace
        public CommandDefinitionNamespace GetNamespace(string name)
        {
            return _namespaces[name];
        }
        #endregion

        #region get definition
        public CommandDefinition GetDefinition(string key)
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

            CommandDefinition cmdDef = this.GetDefinition(command.Key);

            this.EnsureCommand(command, cmdDef);

            cmdDef.Handler(command);
        }
        #endregion

        #region ensure command
        private void EnsureCommand(Command command, CommandDefinition cmdDef)
        {
            List<string> feedback = new List<string>();

            this.EnsureCommandOptions(cmdDef, command, ref feedback);

            if (feedback.Count == 0)
                cmdDef.EnsureConstraints(command);

            if (feedback.Count > 0)
                throw new CommandInputException(feedback.ToArray());
        }
        #endregion

        #region ensure command options
        private void EnsureCommandOptions(CommandDefinition cmdDef, Command cmd, ref List<string> feedback)
        {
            //ensure options fully hydrated
            this.EnsureCommandOptionsFullyHydrated(cmdDef, cmd);

            this.EnsureAllInputCommandOptionsDefined(cmdDef, cmd, ref feedback);
            if (feedback.Count > 0)
                return;

            this.EnsureNoDuplicateOptions(cmdDef, cmd, ref feedback);
            if (feedback.Count > 0)
                return;

            foreach(var opDef in cmdDef.Options)
            {
                var op = cmd.GetOption(opDef.Key);

                if (!this.EnsureMustAssignOptionProvidedAndAssigned(opDef, op, ref feedback))
                    continue;

                if (op is EmptyCommandOption || op is DefaultCommandOption)
                    continue;

                this.EnsureOptionConstraints(opDef, op, ref feedback);
            }
        }
        #endregion

        #region ensure options fully hydrated
        private void EnsureCommandOptionsFullyHydrated(CommandDefinition cmdDef, Command cmd)
        {
            foreach (var opDef in cmdDef.Options)
            {
                var op = cmd.GetOptionByFlag(opDef.Flags);

                if (op is not null)//apply the definition key to the option
                    op.SetKey(opDef.Key);

                else if (opDef.HasDefault)//apply the default option value
                    cmd.ApplyDefaultOption(opDef.DefaultInstance());

                else//empty, just need an empty shell with correct key
                    cmd.ApplyEmptyOption(opDef.EmptyInstance());
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

                //empty ops are added to simply fully hydrate all op keys, they will always be 'valid'
                if (op is EmptyCommandOption)
                    continue;

                //default ops can always be assumed to be valid...
                if (op is DefaultCommandOption)
                    continue;

                if (!cmdDef.Options.Any(o => o.Flags.Contains(op.Flag)))
                    feedback.Add($"Undefined option at position: {i + 1} ... option: {op.Flag}");
            }
        }
        #endregion

        #region ensure no duplicate options
        private void EnsureNoDuplicateOptions(CommandDefinition cmdDef, Command cmd, ref List<string> feedback)
        {
            for (int i = 0; i < cmdDef.Options.Count; i++)
            {
                CommandOptionDefinition opDef = cmdDef.Options[i];
                var opUno = cmd.GetOptionByFlag(opDef.Flags);
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
        private bool EnsureMustAssignOptionProvidedAndAssigned(CommandOptionDefinition opDef, CommandOption op, ref List<string> feedback)
        {
            if (opDef.MustAssign)
            {
                if (op is EmptyCommandOption)
                {
                    feedback.Add($"Expected option [{string.Join("|", opDef.Flags)}] not found...option is marked '{nameof(CommandOptionDefinition.MustAssign)}'");
                    return false;
                }
                else if (string.IsNullOrEmpty(op.Argument))
                {
                    feedback.Add($"Option '{op.Flag}' requires an argument...no argument provided.");
                    return false;
                }
            }
            return true;
        }
        #endregion

        #region ensure option constraints
        private void EnsureOptionConstraints(CommandOptionDefinition optionDef, CommandOption option, ref List<string> feedback)
        {
            try
            {
                //pass op to typed definition to convert the command line arg (string) into T and set the typed option value
                optionDef.SetConvertedValue(option);

                //ensure passes custom contraints if defined...
                optionDef.EnsureConstraints(option);
            }
            catch (CommandArgumentException ex)
            {
                feedback.Add(ex.Message);
            }
        }
        #endregion

        #region ensure command constraints
        private void EnsureCommandConstraints(CommandDefinition cmdDef, Command cmd, ref List<string> feedback)
        {
            try
            {
                cmdDef.EnsureConstraints(cmd);
            }
            catch (CommandArgumentException ex)
            {
                feedback.Add(ex.Message);
            }
        }
        #endregion
    }
}
