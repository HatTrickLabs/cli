using System;
using System.Collections.Generic;

namespace HatTrick.CommandLine
{
    internal class SetOfCommandDefinition : SetOf<CommandDefinition>
    {
        #region interface
        public CommandDefinition this[string name]
        {
            get
            {
                if (name is null)
                    throw new ArgumentNullException(nameof(name));

                if (base.Length == 0)
                    throw new KeyNotFoundException($"No '{nameof(CommandDefinition)}' found for provided {nameof(name)} '{name}'");

                int i = base.FindIndex((ns) => string.Compare(ns.Name, name, false) == 0);

                if (i == -1)
                    throw new KeyNotFoundException($"No '{nameof(CommandDefinition)}' found for provided {nameof(name)} '{name}'");

                return base[i];
            }
        }
        #endregion

        #region constructors
        public SetOfCommandDefinition() : base()
        { }

        public SetOfCommandDefinition(int minimumCapacity) : base(minimumCapacity)
        { }
        #endregion

        #region contains name
        public bool ContainsName(string name)
        {
            if (name is null)
                throw new ArgumentNullException(nameof(name));

            if (base.Length == 0)
                return false;

            int i = base.FindIndex((ns) => string.Compare(ns.Name, name, false) == 0);

            return i > -1;
        }
        #endregion

        #region add
        public override void Add(CommandDefinition commandDef)
        {
            if (commandDef is null)
                throw new ArgumentNullException(nameof(commandDef));

            commandDef.Validate();

            this.ThrowOnDuplicateName(commandDef.Name);

            base.Add(commandDef);
        }
        #endregion

        #region throw on duplicate name
        private void ThrowOnDuplicateName(string name)
        {
            if (this.ContainsName(name))
                throw new CommandDefinitionException($"Cannot add {nameof(CommandDefinition)}, duplicate key found: '{name}'");
        }
        #endregion

        #region try get
        public bool TryGet(string name, out CommandDefinition commandDef)
        {
            commandDef = base.Find((ns) => string.Compare(ns.Name, name, false) == 0);
            return commandDef is not null; ;
        }
        #endregion

        #region get descendents
        internal CommandDefinition[] GetDescendents(string ofNamespace, bool includeHidden)
        {
            if (ofNamespace is null)
                throw new ArgumentNullException(nameof(ofNamespace));

            if (ofNamespace == string.Empty)
                return base.Empty;

            int depth = 0;
            for (int i = 1; i < ofNamespace.Length - 1; i++)
                if (ofNamespace[i] == '.') { depth += 1; }

            var children = base.FindAll((cmd) =>
                      cmd.Hidden == includeHidden &&
                      cmd.Depth > depth && //must check for > because commands can have segment gaps...
                      cmd.Name.StartsWith(ofNamespace)
            );

            return children;
        }
        #endregion
    }
}
