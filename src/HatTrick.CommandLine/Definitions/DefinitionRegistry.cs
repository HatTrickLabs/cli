using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reflection.Metadata.Ecma335;

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

        #region build command
        private Command BuildCommand(string input)
        {
            string[] args = Scanner.Scan(input);
            return this.BuildCommand(args);
        }

        private Command BuildCommand(string[] args)
        {
            Token[] tokens = Tokenizer.Tokenize(args);
            Command cmd = CommandParser.Parse(tokens);
            return cmd;
        }
        #endregion

        #region get executor
        public CommandExecutor GetCommandExecutor(string input)
        {
            Command cmd = this.BuildCommand(input);
            CommandDefinition cmdDef = this.GetCommandDefinition(cmd.Name);
            var executor = new CommandExecutor(cmdDef, cmd);
            return executor;
        }

        public CommandExecutor GetCommandExecutor(string[] args)
        {
            Command cmd = this.BuildCommand(args);
            CommandDefinition cmdDef = this.GetCommandDefinition(cmd.Name);
            var executor = new CommandExecutor(cmdDef, cmd);
            return executor;
        }
        #endregion
    }
}
