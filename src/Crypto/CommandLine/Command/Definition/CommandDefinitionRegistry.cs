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
            commandNamespace.Ensure();
            _namespaces.Add(commandNamespace.Name, commandNamespace);
        }

        public void Add(CommandDefinition commandDef)
        {
            commandDef.Ensure();
            _definitions.Add(commandDef.Key, commandDef);
        }
        #endregion

        #region get
        public CommandDefinition Get(string key)
        {
            return _definitions.ContainsKey(key) 
                ? _definitions[key] 
                : throw new CommandInputException($"No command registered for provided key: {key}");
        }
        #endregion

        #region try get
        public bool TryGet(string key, out CommandDefinition commandDef)
        {
            commandDef = _definitions.ContainsKey(key) ? _definitions[key] : null;
            return commandDef is not null;
        }
        #endregion

        #region execute command
        public void ExecuteCommand(Command command)
        {
            if (command.Key is null || string.IsNullOrEmpty(command.Key))
                throw new CommandInputException("No command provided.");

            this.EnsureCommand(command, out CommandDefinition cmdDef);

            cmdDef.EntryPoint(command);
        }
        #endregion

        #region ensure command
        private void EnsureCommand(Command command, out CommandDefinition cmdDef)
        {
            if (!this.TryGet(command.Key, out cmdDef))
                throw new CommandInputException($"No command definition registered for provided command: {command.Key}");

            List<string> feedback = new List<string>();
            this.EnsureCommandOptions(cmdDef, command, ref feedback);

            if (feedback.Count > 0)
                throw new CommandInputException(feedback.ToArray());
        }
        #endregion

        #region ensure command options
        private void EnsureCommandOptions(CommandDefinition cmdDef, Command cmd, ref List<string> feedback)
        {
            //ensure all provided options are expected / defined
            this.EnsureNoUndefinedOptions(cmdDef, cmd, ref feedback);

            //run all validation for specific provided options
            for (int i = 0; i < cmdDef.Options.Count; i++)
            {
                var opDef = cmdDef.Options[i];
                var op = cmd.Options?.FirstOrDefault(o => opDef.Flags.Contains(o.Flag));

                if (opDef.MustAssign)
                {
                    //if must assign, ensure provided
                    if (op is null)
                    {
                        feedback.Add($"Expected option [{string.Join("|", opDef.Flags)}] not found...option is marked '{nameof(CommandOptionDefinition.MustAssign)}'");
                        continue;
                    }

                    //if must assign, ensure assigned
                    if (string.IsNullOrEmpty(op.Argument))//ensure option has supplied argument
                    {
                        feedback.Add($"Option '{op.Flag}' requires an argument...no argument provided.");
                        continue;
                    }
                }

                if (op is not null)
                {
                    //TODO: to allow dupes or not to allow dupes ... should dupes be utilized for SIMPLE multi imput scenarios (array of items) without
                    //      the need to define a custom type converter ??? ex: abc.exe get.dir.file.count --path d:\tmp --path d:\img --path d:\my-pics

                    //ensure no dupes
                    for (int j = 0; j < (cmd.Options?.Length ?? 0); j++)
                    {
                        CommandOption sco = cmd.Options[j];
                        if (sco == op)
                            continue;

                        if (opDef.Flags.Contains(sco.Flag))
                            feedback.Add($"Duplicate options provided at positions: {i + 1} and {j + 1}...'{op.Flag}' and '{cmd.Options[j].Flag}'");
                    }

                    //port the formal option key over to the option from the option definition
                    op.SetKey(opDef.Key);

                    //utilize the typed option definition to convert the command line arg (string) into T and set the typed option value
                    if (!opDef.TrySetOptionValue(op))
                    {
                        string name = opDef.GetType().GetGenericArguments()[0].Name;
                        feedback.Add($"Option '{op.Flag}' requires an argument of type '{name}' ... invalid value provided: '{op.Argument}'");
                    }
                }
            }
        }
        #endregion

        #region ensure no undefined options
        private void EnsureNoUndefinedOptions(CommandDefinition cmdDef, Command cmd, ref List<string> feedback)
        {
            int countProvided = cmd.Options?.Length ?? 0;

            //if no options defined in the definition and options provided in the command, all provided options must be invalid
            if (cmdDef.Options is null || cmdDef.Options.Count == 0)
            {
                if (countProvided > 0)
                    feedback.Add($"The '{cmdDef.Key}' command does not accept any options...provided options are invalid.");
            }
            else
            {
                //if options are defined for the command, confirm each option provided is valid
                for (int i = 0; i < countProvided; i++)
                {
                    var op = cmd.Options[i];
                    if (!cmdDef.Options.Exists(o => o.Flags.Contains(op.Flag)))
                    {
                        feedback.Add($"Undefined option at position: {i + 1} ... option: {op.Flag}");
                    }
                }
            }
        }
        #endregion
    }
}
