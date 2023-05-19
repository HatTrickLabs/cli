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

        #region set default value
        //public virtual void SetDefaultValuex<T>(T value)
        //{
        //}
        public abstract void SetDefaultValuex<T>(T value);
        #endregion

        #region of T
        internal CommandOptionDefinition<T> Of<T>()
        {
            if (this is CommandOptionDefinition<T> cmdOpDefT)
                return cmdOpDefT;
            else
                throw new InvalidCastException($"Cannot cast {nameof(CommandOptionDefinition)} to {nameof(CommandOptionDefinition)}<{nameof(T)}>");
        }
        #endregion

        #region try set option value
        internal abstract bool TrySetOptionValue(CommandOption option);
        #endregion

        #region validate
        internal virtual void Validate()
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
        private T _default;
        private T[] _accepted;
        private Action<T> _constraints;
        #endregion

        #region interface
        internal T Default => _default;
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

        #region set default value
        public override void SetDefaultValuex<Y>(Y value)
        {
            if (typeof(Y) != typeof(T))
                throw new ArgumentException("");
        }

        public void SetDefaultValue(T value)
        {
            if (base.MustAssign)
                throw new CommandDefinitionException($"Option '{base.Key}' is marked '{nameof(CommandOptionDefinition.MustAssign)}'...'default' cannot be applied.");

            _default = value;
        }
        #endregion

        #region set accepted values
        internal void SetAcceptedValues(T[] values)
        {
            _accepted = values;
        }
        #endregion

        #region apply constraint
        public void ApplyConstraint(Action<T> constraint)
        {
            if (constraint is null)
                throw new ArgumentNullException(nameof(constraint));

            _constraints += constraint;
        }
        #endregion

        #region try set option value
        internal override bool TrySetOptionValue(CommandOption option)
        {
            EqualityComparer<T> eq = EqualityComparer<T>.Default;
            Func<T, bool> isAcceptedValue = (a) => Array.FindIndex(_accepted, (b) => eq.Equals(a, b)) > -1;
            try
            {
                T val = _converter.Invoke(option.Argument);

                if (_accepted is not null && !isAcceptedValue(val))
                        throw new CommandInputException($"Provided argument: {option.Argument} does not exist in accepted value set: {string.Join(", ", _accepted)}");

                _constraints?.Invoke(val);

                option.SetValue(val);
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region validate
        internal override void Validate()
        {
            base.Validate();
        }
        #endregion
    }
    #endregion
}
