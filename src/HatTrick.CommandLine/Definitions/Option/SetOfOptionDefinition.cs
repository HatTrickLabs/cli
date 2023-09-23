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
        private void ThrowOnDuplicateFlag(string[] flags)
        {
            for (int i = 0; i < flags.Length; i++)
            {
                for (int j = 0; j < base.Length; j++)
                {
                    for (int k = 0; k < base[j].Flags.Length; k++)
                    {
                        if (string.Compare(base[j].Flags[k], flags[i], false) == 0)
                            throw new CommandDefinitionException($"Cannot add {nameof(OptionDefinition)}, duplicate flag found: {flags[i]}");
                    }
                }
            }
        }
        #endregion
    }
}
