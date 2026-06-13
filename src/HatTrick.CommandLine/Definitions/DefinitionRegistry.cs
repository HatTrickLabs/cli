using System;

namespace HatTrick.CommandLine
{
    public class DefinitionRegistry
    {
        #region internals
        private static DefinitionRegistry _instance;
        private static object _lock = new();
        private SetOfNamespaceDefinition _namespaces;
        private SetOfCommandDefinition _commands;
        #endregion

        #region interface
        public int CommandDefinitionCount => _commands.Length;

        public int NamespaceDefinitionCount => _namespaces.Length;
        #endregion

        #region constructors
        private DefinitionRegistry()
        {
            _commands = new SetOfCommandDefinition();
            _namespaces = new SetOfNamespaceDefinition();
        }
        #endregion

        #region get instance
        public static DefinitionRegistry GetInstance()
        {
            lock (_lock)
            {
                if (_instance is null)
                {
                    _instance = new DefinitionRegistry();
                    _instance.Add(new DefaultCommandDefinition());
                }
            }
            return _instance;
        }
        #endregion

        #region clear
        internal static void Clear()//for unit testing only!
        {
            lock (_lock)
            {
                _instance = null;
            }
        }
        #endregion

        #region add
        public void Add(NamespaceDefinition namespaceDef)
        {
            if (namespaceDef is null)
                throw new ArgumentNullException(nameof(namespaceDef));

            //ensure no command name collisions — for the namespace itself or any ancestor
            //that will be auto-created as a synthetic placeholder.
            string[] segments = namespaceDef.Name.Split('.');
            string prefix = null;
            for (int i = 0; i < segments.Length; i++)
            {
                prefix = (i > 0) ? string.Concat(prefix, '.', segments[i]) : segments[i];
                if (_commands.ContainsName(prefix))
                    throw new NamespaceDefinitionException($"Naming collision between namespace and command definition: {prefix}");
            }

            _namespaces.Add(namespaceDef);
        }

        public void Add(CommandDefinition commandDef)
        {
            //ensure no namespace name collisions
            if (_namespaces.ContainsName(commandDef.Name))
                throw new CommandDefinitionException($"Naming collision between command definition and namespace: {commandDef.Name}");

            _commands.Add(commandDef);
        }
        #endregion

        #region get command definition
        public CommandDefinition GetCommandDefinition(string name)
        {
            CommandDefinition cmdDef = _commands[name];
            return cmdDef;
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
            return _namespaces.FindAll(where is null ? (ns) => true : where);
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

        #region get child namespace definitions
        internal NamespaceDefinition[] GetChildNamespaceDefinitions(NamespaceDefinition parent, bool includeHidden = false)
        {
            var children = _namespaces.GetChildren(parent, includeHidden);
            return children;
        }
        #endregion

        #region get ancestor namespace definitions
        internal NamespaceDefinition[] GetAncestorNamespaceDefinitions(CommandDefinition descendant, bool includeHidden = false)
        {
            var ancestors = _namespaces.GetAncestors(descendant, includeHidden);
            Array.Sort(ancestors, (a, b) => a.Depth.CompareTo(b.Depth));
            return ancestors;
        }
        #endregion

        #region get child command definitions
        internal CommandDefinition[] GetChildCommandDefinitions(NamespaceDefinition parent, bool includeHidden = false)
        {
            if (parent is null)
                throw new ArgumentNullException(nameof(parent));

            NamespaceDefinition[] descendentNamespaces = _namespaces.GetDescendants(parent, includeHidden);

            CommandDefinition[] descendents = _commands.GetDescendents(parent.Name, includeHidden);

            CommandDefinition[] children = Array.FindAll(descendents, (d) => 
                !Array.Exists(descendentNamespaces, (ns) => d.Name.StartsWith(ns.Name))
            );

            return children;
        }
        #endregion

        #region get descendent command definitions
        internal CommandDefinition[] GetDescendentCommandDefinitions(NamespaceDefinition parent, bool includeHidden = false)
        {
            if (parent is null)
                throw new ArgumentNullException(nameof(parent));

            var descendents = _commands.GetDescendents(parent.Name, includeHidden);

            return descendents;
        }
        #endregion

        #region get command executor
        public CommandExecutor GetCommandExecutor(Command command)
        {
            if (command.Name is null)
                throw new ArgumentNullException($"{nameof(command)}.{nameof(command.Name)}");

            if (command.Name == string.Empty)
                throw new ArgumentException("Property must contain a value.", $"{nameof(command)}.{nameof(command.Name)}");

            CommandDefinition cmdDef = this.GetCommandDefinition(command.Name);
            var executor = new CommandExecutor(cmdDef, command);
            return executor;
        }
        #endregion
    }
}
