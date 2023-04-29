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
        public CommandDefinition[] GetAllCommands()
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

        public void Add(CommandDefinition command)
        {
            command.Ensure();
            _definitions.Add(command.Key, command);
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
        public bool TryGet(string key, out CommandDefinition cmdDef)
        {
            cmdDef = _definitions.ContainsKey(key) ? _definitions[key] : null;
            return cmdDef is not null;
        }
        #endregion

        #region execute shell command
        public void ExecuteShellCommand(ShellCommand shellCommand)
        {
            if (shellCommand.Command is null || string.IsNullOrEmpty(shellCommand.Command))
                throw new CommandInputException("No command provided.");

            this.EnsureShellCommand(shellCommand, out CommandDefinition cmdDef);

            cmdDef.EntryPoint(shellCommand);
        }
        #endregion

        #region ensure shell command
        private void EnsureShellCommand(ShellCommand shellCommand, out CommandDefinition commandDef)
        {
            if (!this.TryGet(shellCommand.Command, out commandDef))
                throw new CommandInputException($"No command definition registered for provided shell command: {shellCommand.Command}");

            List<string> feedback = new List<string>();
            this.EnsureShellCommandOptions(commandDef, shellCommand, ref feedback);

            if (feedback.Count > 0)
                throw new CommandInputException(feedback.ToArray());
        }
        #endregion

        #region ensure shell command options
        private void EnsureShellCommandOptions(CommandDefinition cmdDef, ShellCommand shellCmd, ref List<string> feedback)
        {
            //ensure all provided options are expected / defined
            this.EnsureNoUndefinedShellOptions(cmdDef, shellCmd, ref feedback);

            //run all validation for specific provided options
            for (int i = 0; i < cmdDef.Options.Count; i++)
            {
                var opDef = cmdDef.Options[i];
                var shellOp = shellCmd.Options?.FirstOrDefault(o => opDef.Flags.Contains(o.Flag));

                if (opDef.MustAssign)
                {
                    //if must assign, ensure provided
                    if (shellOp is null)
                    {
                        feedback.Add($"Expected option [{string.Join("|", opDef.Flags)}] not found...option is marked '{nameof(CommandOptionDefinition.MustAssign)}'");
                        continue;
                    }

                    //if must assign, ensure assigned
                    if (string.IsNullOrEmpty(shellOp.Argument))//ensure option has supplied argument
                    {
                        feedback.Add($"Option '{shellOp.Flag}' requires an argument...no argument provided.");
                        continue;
                    }
                }

                if (shellOp is not null)
                {
                    //TODO: to allow dupes or not to allow dupes ... should dupes be utilized for SIMPLE multi imput scenarios (array of items) without
                    //      the need to define a custom type converter ??? ex: abc.exe get.dir.file.count --path d:\tmp --path d:\img --path d:\my-pics

                    //ensure no dupes
                    for (int j = 0; j < (shellCmd.Options?.Length ?? 0); j++)
                    {
                        ShellCommandOption sco = shellCmd.Options[j];
                        if (sco == shellOp)
                            continue;

                        if (opDef.Flags.Contains(sco.Flag))
                            feedback.Add($"Duplicate options provided at positions: {i + 1} and {j + 1}...'{shellOp.Flag}' and '{shellCmd.Options[j].Flag}'");
                    }

                    //port the formal option key over to the shell option from the option definition
                    shellOp.SetKey(opDef.Key);

                    //utilize the typed option definition to convert the command line arg (string) into T and set the typed shell option value
                    if (!opDef.TrySetOptionValue(shellOp))
                    {
                        string name = opDef.GetType().GetGenericArguments()[0].Name;
                        feedback.Add($"Option '{shellOp.Flag}' requires an argument of type '{name}' ... invalid value provided: '{shellOp.Argument}'");
                    }
                }
            }
        }
        #endregion

        #region ensure no undefined shell options
        private void EnsureNoUndefinedShellOptions(CommandDefinition cmdDef, ShellCommand shellCmd, ref List<string> feedback)
        {
            int countProvided = shellCmd.Options?.Length ?? 0;

            //if no options defined in the definition and options provided in the shell command, all provided options must be invalid
            if (cmdDef.Options is null || cmdDef.Options.Count == 0)
            {
                if (countProvided > 0)
                    feedback.Add($"The '{cmdDef.Key}' command does not accept any options...provided options are invalid.");
            }
            else
            {
                //if options are defined for the command, confirm each shell option provided is valid
                for (int i = 0; i < countProvided; i++)
                {
                    var shellOp = shellCmd.Options[i];
                    if (!cmdDef.Options.Exists(o => o.Flags.Contains(shellOp.Flag)))
                    {
                        feedback.Add($"Undefined option at position: {i + 1} ... option: {shellOp.Flag}");
                    }
                }
            }
        }
        #endregion
    }
}
