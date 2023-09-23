using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reflection.Metadata.Ecma335;

namespace HatTrick.CommandLine
{
    public class Registry
    {
        #region internals
        private static Registry _instance;
        private static object _lock = new();
        private SetOfNamespaceDefinition _namespaces;
        private SetOfCommandDefinition _commands;
        #endregion

        #region interface
        public int CommandDefinitionCount => _commands.Length;

        public int NamespaceDefinitionCount => _namespaces.Length;
        #endregion

        #region constructors
        private Registry()
        {
            _commands = new SetOfCommandDefinition();
            _namespaces = new SetOfNamespaceDefinition();
        }
        #endregion

        #region get instance
        public static Registry GetInstance()
        {
            lock (_lock)
            {
                if (_instance is null)
                {
                    _instance = new Registry();
                    _instance.Add(new DefaultCommandDefinition());
                }
            }
            return _instance;
        }
        #endregion

        #region get command definitions
        public CommandDefinition[] GetCommandDefinitions(Predicate<CommandDefinition> where = null)
        {
            return _commands.FindAll(where is null ? (cmd) => true : where);
        }
        #endregion

        #region get namespace definitions
        public NamespaceDefinition[] GetNamespaceDefinitions(Predicate<NamespaceDefinition> where = null)
        {
            return _namespaces.FindAll(where);
        }
        #endregion

        #region add
        public void Add(NamespaceDefinition namespaceDef)
        {
            //ensure no command name collisions
            if (_commands.ContainsName(namespaceDef.Name))
                throw new NamespaceDefinitionException($"Naming collision between namespace and command definition: {namespaceDef.Name}");

            _namespaces.Add(namespaceDef);
        }

        public void Add(CommandDefinition commandDef)
        {
            //ensure no namespace name collisions
            if (_namespaces.ContainsName(commandDef.Name))
                throw new NamespaceDefinitionException($"Naming collision between command definition and namespace: {commandDef.Name}");

            _commands.Add(commandDef);
        }
        #endregion

        #region try get namespace definition
        public bool TryGetNamespaceDefinition(string name, out NamespaceDefinition namespaceDef)
        {
            return _namespaces.TryGet(name, out namespaceDef);
        }
        #endregion

        #region try get command definition
        public bool TryGetCommandDefinition(string name, out CommandDefinition cmdDef)
        {
            return _commands.TryGet(name, out cmdDef);
        }
        #endregion

        #region get namespace definition
        public NamespaceDefinition GetNamespaceDefinition(string name)
        {
            NamespaceDefinition namespaceDef = _namespaces[name];
            return namespaceDef;
        }
        #endregion

        #region get child namespace definitions
        internal NamespaceDefinition[] GetChildNamespaceDefinitions(NamespaceDefinition parent, bool includeHidden)
        {
            var children = _namespaces.GetChildren(parent, includeHidden);
            return children;
        }
        #endregion

        #region get descendent namespaces definitions
        internal NamespaceDefinition[] GetDescendentNamespaceDefinitions(NamespaceDefinition parent, bool includeHidden)
        {
            var descendents = _namespaces.GetDescendents(parent, includeHidden);
            return descendents;
        }
        #endregion

        #region get command definition
        public CommandDefinition GetCommandDefinition(string name)
        {
            CommandDefinition cmdDef =  _commands[name];
            return cmdDef;
        }
        #endregion

        #region get child command definitions
        internal CommandDefinition[] GetChildCommandDefinitions(NamespaceDefinition parent, bool includeHidden)
        {
            if (parent is null)
                throw new ArgumentNullException(nameof(parent));

            NamespaceDefinition[] descendentNamespaces = this.GetDescendentNamespaceDefinitions(parent, includeHidden);

            CommandDefinition[] descendents = _commands.GetDescendents(parent.Name, includeHidden);

            CommandDefinition[] children = Array.FindAll(descendents, (d) => 
                !Array.Exists(descendentNamespaces, (ns) => d.Name.StartsWith(ns.Name))
            );

            return children;
        }
        #endregion

        #region get descendent command definitions
        internal CommandDefinition[] GetDescendentCommandDefinitions(NamespaceDefinition parent, bool includeHidden)
        {
            if (parent is null)
                throw new ArgumentNullException(nameof(parent));

            var descendents = _commands.GetDescendents(parent.Name, includeHidden);

            return descendents;
        }
        #endregion

        #region execute command
        public void ExecuteCommand(Command command)
        {
            if (command.Key is null || string.IsNullOrEmpty(command.Key))
                throw new CommandInputException("No command provided.");

            CommandDefinition cmdDef = this.GetCommandDefinition(command.Key);

            this.EnsureCommand(command, cmdDef);

            cmdDef.Handler(command);
        }
        #endregion

        #region execute command async
        public async Task ExecuteCommandAsync(Command command)
        {
            if (command.Key is null || string.IsNullOrEmpty(command.Key))
                throw new CommandInputException("No command provided.");

            CommandDefinition cmdDef = this.GetCommandDefinition(command.Key);

            this.EnsureCommand(command, cmdDef);

            await cmdDef.AsyncHandler(command);
        }
        #endregion

        #region ensure command
        private void EnsureCommand(Command command, CommandDefinition cmdDef)
        {
            var feedback = new SetOf<string>();

            this.EnsureCommandOptions(cmdDef, command, ref feedback);

            if (feedback.Length == 0)
                cmdDef.EnsureConstraints(command, ref feedback);

            if (feedback.Length > 0)
                throw new CommandInputException(feedback.ToArray());
        }
        #endregion

        #region ensure command options
        private void EnsureCommandOptions(CommandDefinition cmdDef, Command cmd, ref SetOf<string> feedback)
        {
            this.EnsureCommandOptionsFullyHydrated(cmdDef, cmd, ref feedback);
            if (feedback.Length > 0)
                return;

            this.EnsureAllProvidedOptionsDefined(cmdDef, cmd, ref feedback);
            if (feedback.Length > 0)
                return;

            this.EnsureNoDuplicateOptions(cmdDef, cmd, ref feedback);
            if (feedback.Length > 0)
                return;

            this.EnsureOptionConstraints(cmdDef, cmd, ref feedback);
        }
        #endregion

        #region ensure options fully hydrated
        private void EnsureCommandOptionsFullyHydrated(CommandDefinition cmdDef, Command cmd, ref SetOf<string> feedback)
        {
            for(int i = 0; i < cmdDef.Options.Length; i++)
            {
                var opDef = cmdDef.Options[i];
                var op = cmd.GetOptionByFlag(opDef.Flags);

                if (op is null)
                {
                    //empty, just need an empty shell with correct key
                    op = opDef.EmptyInstance();
                    cmd.AddEmptyOption(op as EmptyOption);
                }
                else
                {
                    //apply the definition key to the option
                    op.ApplyKey(opDef.Key);

                    //pass op to opDef to set the value (passing in because only the def knows what T is).
                    opDef.ApplyConvertedValueTo(op, out string error);
                    if (error is not null)
                        feedback.Add(error);
                }
            }
        }
        #endregion

        #region ensure all provided options defined
        private void EnsureAllProvidedOptionsDefined(CommandDefinition cmdDef, Command cmd, ref SetOf<string> feedback)
        {
            if (cmd.Options.Length > 0 && !cmdDef.HasOptions)
            {
                feedback.Add($"The '{cmdDef.Name}' command does not accept any options...provided options are invalid.");
                return;
            }

            //if any options are defined for the command, confirm each option provided is valid
            for (int i = 0; i < cmd.Options.Length; i++)
            {
                var op = cmd.Options[i];

                //empty ops can always be assumed to be valid...because they were injected not provided
                if (op is EmptyOption)
                    continue;

                if (!cmdDef.Options.Exists(o => o.Flags.Contains(op.Flag)))
                    feedback.Add($"Undefined option at position: {i + 1} ... option: {op.Flag}");
            }
        }
        #endregion

        #region ensure no duplicate options
        private void EnsureNoDuplicateOptions(CommandDefinition cmdDef, Command cmd, ref SetOf<string> feedback)
        {
            //TODO: refactor, this looks fundamentally wrong
            for (int i = 0; i < cmdDef.Options.Length; i++)
            {
                var opDef = cmdDef.Options[i];
                var opUno = cmd.GetOptionByFlag(opDef.Flags);
                for (int j = 0; j < cmd.Options.Length; j++)
                {
                    var opDos = cmd.Options[j];

                    if (opDos == opUno)
                        continue;

                    if (opDef.Flags.Contains(opDos.Flag))
                        feedback.Add($"Duplicate options provided at positions: {i + 1} and {j + 1}...'{opUno.Flag}' and '{opDos.Flag}'");
                }
            }
        }
        #endregion

        #region ensure option constraints
        private void EnsureOptionConstraints(CommandDefinition cmdDef, Command cmd, ref SetOf<string> feedback)
        {
            for (int i = 0; i < cmdDef.Options.Length; i++)
            {
                OptionDefinition opDef = cmdDef.Options[i];
                ref Option op = ref cmd.GetOptionByRef(opDef.Key);

                //If empty op and a default constraint exists, empty op will be swapped for a default...hence the ref param
                opDef.EnsureConstraints(ref op, ref feedback);
            }
        }
        #endregion
    }
}
