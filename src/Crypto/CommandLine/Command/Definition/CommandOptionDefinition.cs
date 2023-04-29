using System;
using System.Collections.Generic;

namespace Crypto.CommandLine
{
    #region command option definition
    public abstract class CommandOptionDefinition
    {
        #region internals
        private readonly string _key;
        private readonly bool _mustAssign;
        private readonly string _help;
        private readonly string[] _flags;
        #endregion

        #region interface
        public string Key => _key;

        public bool MustAssign => _mustAssign;

        public string Help => _help;

        public string[] Flags => _flags;
        #endregion

        #region constructors
        protected CommandOptionDefinition(string key, bool mustAssign, string help, params string[] flags)
        {
            _key = key ?? throw new ArgumentNullException(nameof(key));
            _mustAssign = mustAssign;
            _help = help;
            _flags = flags ?? throw new ArgumentNullException(nameof(flags));
        }
        #endregion

        #region try set option value
        internal abstract bool TrySetOptionValue(CommandOption option);
        #endregion

        #region ensure
        internal virtual void Ensure()
        {
            if (_key == string.Empty)
                throw new CommandDefinitionException("All options must have a valid key...Provided key is empty.");

            if (_flags is null || _flags.Length == 0)
                throw new CommandDefinitionException($"Options[{_key}] must contain at least 1 {nameof(CommandOptionDefinition.Flags)}.");

            foreach (string flag in _flags)
            {
                if (string.IsNullOrWhiteSpace(flag))
                    throw new CommandDefinitionException($"Options[{_key}] contains a flag that is null or empty.");

                if (flag[0] != '-')
                    throw new CommandDefinitionException($"Option flags must begin with a '-'...'{flag}' is not valid.");

                if (flag[1] == '-') //verbose definition
                {
                    if (flag.Length < 4)
                        throw new CommandDefinitionException($"Verbose option flags begin with '--' and must be longer than 1 char...'{flag}' is not valid.");
                }
                else //terse definition
                {
                    if (flag.Length > 2)
                        throw new CommandDefinitionException($"Terse option flags begin with '-' and must be exactly 1 other char...'{flag}' is not valid.");
                }
            }
        }
        #endregion
    }
    #endregion

    #region command option definition of T
    public class CommandOptionDefinition<T> : CommandOptionDefinition
    {
        #region internals
        private readonly Func<string, T> _converter;
        #endregion

        #region interface
      
        #endregion

        #region constructors
        internal CommandOptionDefinition(string key, Func<string, T> converter, params string[] flags) : this(key, converter, null, flags)
        {
        }

        internal CommandOptionDefinition(string key, Func<string, T> converter, string help, params string[] flags) : this(key, converter, false, help, flags)
        {
        }

        internal CommandOptionDefinition(string key, Func<string, T> converter, bool mustAssign, string help, params string[] flags) : base(key, mustAssign, help, flags)
        {
            _converter = converter ?? throw new ArgumentNullException(nameof(key));
        }
        #endregion

        #region try set option value
        internal override bool TrySetOptionValue(CommandOption option)
        {
            try
            {
                T val = _converter.Invoke(option.Argument);
                option.SetValue(val);
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region ensure
        internal override void Ensure()
        {
            base.Ensure();
        }
        #endregion
    }
    #endregion
}
