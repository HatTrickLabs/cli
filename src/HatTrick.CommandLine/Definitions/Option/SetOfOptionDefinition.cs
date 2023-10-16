using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace HatTrick.CommandLine
{
    internal class SetOfOptionDefinition : SetOf<OptionDefinition>
    {
        #region interface
        public OptionDefinition this[string key]
        {
            get
            {
                if (key is null)
                    throw new ArgumentNullException(nameof(key));

                if (base.Length == 0)
                    throw new KeyNotFoundException($"No '{nameof(OptionDefinition)}' found for provided {nameof(key)} '{key}'");

                int i = base.FindIndex((o) => string.Compare(o.Key, key, false) == 0);

                if (i == -1)
                    throw new KeyNotFoundException($"No '{nameof(OptionDefinition)}' found for provided {nameof(key)} '{key}'");

                return base[i];
            }
        }
        #endregion

        #region constructors
        public SetOfOptionDefinition() : base()
        { }

        public SetOfOptionDefinition(int minimumCapacity) : base(minimumCapacity)
        { }
        #endregion

        #region contains key
        public bool ContainsKey(string key)
        {
            if (key is null)
                throw new ArgumentNullException(nameof(key));

            if (base.Length == 0)
                return false;

            int i = base.FindIndex((o) => string.Compare(o.Key, key, false) == 0);

            return i > -1;
        }
        #endregion

        #region add
        public override void Add(OptionDefinition option)
        {
            if (option is null)
                throw new ArgumentNullException(nameof(option));

            option.Validate();

            this.ThrowOnDuplicateKey(option.Key);
            this.ThrowOnDuplicateFlag(option.Flags);

            base.Add(option);
        }
        #endregion

        #region throw on duplicate key
        private void ThrowOnDuplicateKey(string key)
        {
            if (this.ContainsKey(key))
                throw new ArgumentException($"Cannot insert {nameof(OptionDefinition)}, duplicate key found: '{key}'");
        }
        #endregion

        #region throw on duplicate flag
        private void ThrowOnDuplicateFlag((string terse, string verbose) flags)
        {
            for (int i = 0; i < base.Length; i++)
            {
                if (flags.terse == base[i].Flags.terse)
                    throw new CommandDefinitionException($"Cannot add {nameof(OptionDefinition)}, duplicate flag found: {flags.terse}");

                if (flags.verbose == base[i].Flags.verbose)
                    throw new CommandDefinitionException($"Cannot add {nameof(OptionDefinition)}, duplicate flag found: {flags.verbose}");
            }
        }
        #endregion
    }
}
