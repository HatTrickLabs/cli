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
        private bool _hasDefault;
        #endregion

        #region interface
        public string Key => _key;

        public bool MustAssign => _mustAssign;

        public string Help => _help;

        public string[] Flags => _flags;

        public bool HasDefault => _hasDefault;
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

        #region get generic type name
        internal abstract string GetGenericTypeName();
        #endregion

        #region set default
        public void SetDefault<T>(T value)
        {
            this.Of<T>().SetDefault(value);
            _hasDefault = true;
        }
        #endregion

        #region set accepted
        public void SetAccepted<T>(params T[] values)
        {
            this.Of<T>().SetAccepted(values);
        }
        #endregion

        #region apply constraint
        public void ApplyConstraint<T>(Predicate<T> constraint)
        {
            if (constraint is null)
                throw new ArgumentNullException(nameof(constraint));

            this.Of<T>().ApplyConstraint(constraint);
        }
        #endregion

        #region of T
        public CommandOptionDefinition<T> Of<T>()
        {
            if (this is CommandOptionDefinition<T> cmdOpDefT)
                return cmdOpDefT;
            else
                throw new InvalidCastException($"Cannot cast {nameof(CommandOptionDefinition)} to {nameof(CommandOptionDefinition)}<{nameof(T)}>");
        }
        #endregion

        #region try set value
        internal abstract bool TrySetValue(CommandOption option);
        #endregion

        #region value is in accepted set
        internal abstract bool ValueIsInAcceptedSet(CommandOption option);
        #endregion

        #region value conforms to constraints
        internal abstract bool ValueConformsToConstraints(CommandOption option);
        #endregion

        #region empty instance
        internal abstract EmptyCommandOption EmptyInstance();
        #endregion

        #region default instance
        internal abstract DefaultCommandOption DefaultInstance();
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
        private Predicate<T> _constraints;
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

        #region get generic type name
        internal override string GetGenericTypeName()
        {
            //we can do this because the base is abstract with a protected constructor...
            string name = this.GetType().GetGenericArguments()[0].Name;
            return name;
        }
        #endregion

        #region empty instance
        internal override EmptyCommandOption EmptyInstance()
        {
            //TODO: prob impl way to find the most verbose flag
            var op = new EmptyCommandOption(base.Key, base.Flags[0]);
            return op;
        }
        #endregion

        #region default instance
        internal override DefaultCommandOption DefaultInstance()
        {
            //TODO: prob impl way to find the most verbose flag
            var op = new DefaultCommandOption(base.Key, base.Flags[0]);
            op.SetValue(_default);
            return op;
        }
        #endregion

        #region set default
        public void SetDefault(T value)
        {
            if (base.MustAssign)
                throw new CommandDefinitionException($"Option '{base.Key}' is marked '{nameof(CommandOptionDefinition.MustAssign)}'...'default' cannot be applied.");

            _default = value;
        }
        #endregion

        #region set accepted
        public void SetAccepted(T[] values)
        {
            _accepted = values;
        }
        #endregion

        #region value is in accepted set
        internal override bool ValueIsInAcceptedSet(CommandOption option)
        {
            if (_accepted is null || _accepted.Length == 0)
                return true;

            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            T a = option.GetValue<T>();
            return Array.FindIndex(_accepted, (b) => comparer.Equals(a, b)) > -1;
        }
        #endregion

        #region value conforms to constraints
        internal override bool ValueConformsToConstraints(CommandOption option)
        {
            if (_constraints is null)
                return true;

            T val = option.GetValue<T>();

            return _constraints(val);
        }
        #endregion

        #region apply constraint
        public void ApplyConstraint(Predicate<T> constraint)
        {
            if (constraint is null)
                throw new ArgumentNullException(nameof(constraint));

            _constraints += constraint;
        }
        #endregion

        #region try set value
        internal override bool TrySetValue(CommandOption option)
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

        #region validate
        internal override void Validate()
        {
            base.Validate();
        }
        #endregion
    }
    #endregion
}
