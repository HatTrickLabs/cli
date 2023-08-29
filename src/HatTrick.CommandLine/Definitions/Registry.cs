using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HatTrick.CommandLine
{
    public class Registry
    {
        #region internals
        private static Registry _instance;
        private static object _lock = new();
        private Dictionary<string, NamespaceDefinition> _namespaceDefs;
        private Dictionary<string, CommandDefinition> _cmdDefs;
        #endregion

        #region interface
        public int CommandDefinitionCount => _cmdDefs.Count;

        public int NamespaceDefinitionCount => _namespaceDefs.Count;
        #endregion

        #region constructors
        private Registry()
        {
            _cmdDefs = new Dictionary<string, CommandDefinition>();
            _namespaceDefs = new Dictionary<string, NamespaceDefinition>();
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
            return Array.FindAll(_cmdDefs.Values.ToArray(), (where is null) ? (cmd) => true : where);
        }
        #endregion

        #region get namespace definitions
        public NamespaceDefinition[] GetNamespaceDefinitions(Predicate<NamespaceDefinition> where = null)
        {
            return Array.FindAll(_namespaceDefs.Values.ToArray(), (where is null) ? (ns) => true : where);
        }
        #endregion

        #region add
        public void Add(NamespaceDefinition ns)
        {
            ns.Validate();

            //ensure no command name collisions
            if (_cmdDefs.ContainsKey(ns.Name))
                throw new NamespaceDefinitionException($"Naming collision between namespace and command definition: {ns.Name}");

            if (ns.Name.Contains('.'))
            {
                //ensure no segment gaps
                string[] segments = ns.Name.Split('.');
                string segment = null;
                for (int i = 0; i < (segments.Length - 1); i++)
                {
                    segment = (i > 0) 
                        ? string.Concat(segment, '.', segments[i])
                        : segments[i];

                    if (!_namespaceDefs.ContainsKey(segment))
                    {
                        string msg = $"Cannot register namespace {ns.Name}...no parent namespace for '{segment}' exists.";
                        throw new NamespaceDefinitionException(msg);
                    }
                }
            }
            _namespaceDefs.Add(ns.Name, ns);
        }

        public void Add(CommandDefinition commandDef)
        {
            if (commandDef is not DefaultCommandDefinition)
            {
                commandDef.Validate();

                //ensure no namespace name collisions
                if (_namespaceDefs.ContainsKey(commandDef.Name))
                    throw new NamespaceDefinitionException($"Naming collision between command definition and namespace: {commandDef.Name}");
            }

            _cmdDefs.Add(commandDef.Name, commandDef);
        }
        #endregion

        #region try get namespace
        public bool TryGetNamespaceDefinition(string name, out NamespaceDefinition namespaceDef)
        {
            return _namespaceDefs.TryGetValue(name, out namespaceDef);
        }
        #endregion

        #region try get command definition
        public bool TryGetCommandDefinition(string name, out CommandDefinition cmdDef)
        {
            return _cmdDefs.TryGetValue(name, out cmdDef);
        }
        #endregion

        #region get namespace definition
        public NamespaceDefinition GetNamespaceDefinition(string name)
        {
            return _namespaceDefs.ContainsKey(name)
                ? _namespaceDefs[name]
                : throw new CommandInputException($"No namespace registered for provided name: {name}");
        }
        #endregion

        #region get child namespace definitions
        internal NamespaceDefinition[] GetChildNamespaceDefinitions(NamespaceDefinition parent, bool includeHidden)
        {
            if (parent is null)
                throw new ArgumentNullException(nameof(parent));

            int atDepth = parent.Depth + 1;
            var children = this.GetNamespaceDefinitions((ns) => 
                ns.Hidden == includeHidden &&
                ns.Depth == atDepth &&
                ns.Name.StartsWith(parent.Name)
            );

            return children;
        }
        #endregion

        #region get descendent namespaces
        internal NamespaceDefinition[] GetDescendentNamespaces(NamespaceDefinition parent, bool includeHidden)
        {
            if (parent is null)
                throw new ArgumentNullException(nameof(parent));

            var descendents = this.GetNamespaceDefinitions((ns) =>
                ns.Hidden == includeHidden && 
                ns.Depth > parent.Depth &&
                ns.Name.StartsWith(parent.Name)
            );

            return descendents;
        }
        #endregion

        #region get command definition
        public CommandDefinition GetCommandDefinition(string name)
        {
            return _cmdDefs.ContainsKey(name) 
                ? _cmdDefs[name] 
                : throw new CommandInputException($"No command registered for provided name: {name}");
        }
        #endregion

        #region get child command definitions
        internal CommandDefinition[] GetChildCommandDefinitions(NamespaceDefinition parent, bool includeHidden)
        {
            if (parent is null)
                throw new ArgumentNullException(nameof(parent));

            NamespaceDefinition[] descNamespaces = this.GetDescendentNamespaces(parent, includeHidden);

            var children = this.GetCommandDefinitions((cmd) => 
                      cmd.Hidden == includeHidden &&
                      cmd.Depth > parent.Depth && //must check for > because commands can have segment gaps...
                      cmd.Name.StartsWith(parent.Name) &&
                      !Array.Exists(descNamespaces, (ns) => cmd.Name.StartsWith(ns.Name))
            );

            return children;
        }
        #endregion

        #region get descendent command definitions
        internal CommandDefinition[] GetDescendentCommandDefinitions(NamespaceDefinition parent, bool includeHidden)
        {
            if (parent is null)
                throw new ArgumentNullException(nameof(parent));

            var descendents = this.GetCommandDefinitions((cmd) => 
                    cmd.Hidden == includeHidden && 
                    cmd.Depth > parent.Depth &&
                    cmd.Name.StartsWith(parent.Name)
            );

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
                throw new CommandInputException((string[])feedback);
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
                    cmd.ApplyEmptyOption(op as EmptyCommandOption);
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
                if (op is EmptyCommandOption)
                    continue;

                if (!cmdDef.Options.Exists(o => o.Flags.Contains(op.Flag)))
                    feedback.Add($"Undefined option at position: {i + 1} ... option: {op.Flag}");
            }
        }
        #endregion

        #region ensure no duplicate options
        private void EnsureNoDuplicateOptions(CommandDefinition cmdDef, Command cmd, ref SetOf<string> feedback)
        {
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
                CommandOptionDefinition opDef = cmdDef.Options[i];
                ref CommandOption op = ref cmd.GetOptionByRef(opDef.Key);

                //If empty op and a default constraint exists, empty op will be swapped for a default...hence the ref param
                opDef.EnsureConstraints(ref op, ref feedback);
            }
        }
        #endregion
    }
}
