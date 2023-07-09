using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace HatTrick.CommandLine
{
    #region command option definition
    public abstract class CommandOptionDefinition
    {
        #region const
        public const int MaxKeyLength = 32;
        public const int MaxFlagLength = 32;
        #endregion

        #region internals
        private readonly string _key;
        private readonly bool _mustAssign;
        private readonly string _help;
        private bool _hidden;
        private readonly string[] _flags;
        private bool _hasDefault;
        #endregion

        #region interface
        public string Key => _key;

        public abstract Type GenericType { get; }

        public bool MustAssign => _mustAssign;

        public string Help => _help;

        public bool Hidden => _hidden;

        public string[] Flags => _flags;

        public string MostVerboseFlag => _flags.MaxBy((f) => f.Length);

        public string LeastVerboseFlag => _flags.MinBy((f) => f.Length);

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

        #region of T
        public CommandOptionDefinition<T> Of<T>()
        {
            if (this is CommandOptionDefinition<T> cmdOpDefT)
                return cmdOpDefT;
            else
                throw new InvalidCastException($"Cannot cast {nameof(CommandOptionDefinition)} to {nameof(CommandOptionDefinition)}<{nameof(T)}>");
        }
        #endregion

        #region hide
        public void Hide()
        {
            _hidden = true;
        }
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
        public void ApplyConstraint<T>(Func<T, bool> constraint, string name, string description)
        {
            if (constraint is null)
                throw new ArgumentNullException(nameof(constraint));

            if (description is null)
                throw new ArgumentNullException(nameof(description));

            if (description == string.Empty)
                throw new ArgumentException("Argument must contain a value.", nameof(description));

            this.Of<T>().ApplyConstraint(constraint, name, description);
        }
        #endregion

        #region set converted value
        internal abstract void SetConvertedValue(CommandOption option);
        #endregion

        #region ensure custom constraints
        internal abstract void EnsureConstraints(CommandOption option);
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

            if (_key.Length > CommandOptionDefinition.MaxKeyLength)
                throw new CommandDefinitionException($"{nameof(CommandOptionDefinition)}.{nameof(Key)}...max accepted char length is {CommandOptionDefinition.MaxKeyLength}.");

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

                    if (flag.Length > CommandOptionDefinition.MaxFlagLength)
                        throw new CommandDefinitionException($"Verbose option flags cannot be > {CommandOptionDefinition.MaxFlagLength} chars in length.");
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
        private Type _genericType;
        private readonly Func<string, T> _converter;
        private T _default;
        private List<ArgumentConstraint<T>> _constraints;
        #endregion

        #region interface
        internal T Default => _default;

        public override Type GenericType => this.GetGenericType();

        private List<ArgumentConstraint<T>> Constraints
        {
            get => _constraints is null ? _constraints = new List<ArgumentConstraint<T>>() : _constraints;
            set => _constraints = value;
        }
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
        private Type GetGenericType()
        {
            var name = _genericType is null 
                ? _genericType = this.GetType().GetGenericArguments()[0] 
                : _genericType;

            return name;
        }
        #endregion

        #region empty instance
        internal override EmptyCommandOption EmptyInstance()
        {
            var op = new EmptyCommandOption(base.Key, base.MostVerboseFlag);
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
            if (values is null)
                throw new ArgumentNullException(nameof(values));

            List<ArgumentConstraint<T>> constraints = this.Constraints;

            if (constraints.Count > 0)
            {
                var c = constraints.Find(c => c is AcceptedSetConstraint<T>);
                if (c is not null)
                    constraints.Remove(c);
            }

            if (values.Length == 0)
                return;

            var constraint = new AcceptedSetConstraint<T>(values);
            Constraints.Add(constraint);
        }
        #endregion

        #region apply constraint
        public void ApplyConstraint(Func<T, bool> constraint, string name, string description)
        {
            if (constraint is null)
                throw new ArgumentNullException(nameof(constraint));

            if (description is null)
                throw new ArgumentNullException(nameof(description));

            if (description == string.Empty)
                throw new ArgumentException("Argument must contain a value.", nameof(description));

            this.Constraints.Add(new ArgumentConstraint<T>(constraint, name, description));
        }
        #endregion

        #region set converted value
        internal override void SetConvertedValue(CommandOption option)
        {
            try
            {
                T val = _converter.Invoke(option.Argument);
                option.SetValue(val);
            }
            catch
            {
                var flag = option.Flag;
                var name = this.GetGenericType().Name;
                var arg = option.Argument;
                throw new CommandArgumentException($"Option '{flag}' requires argument of type '{name}'...invalid value provided: {arg}"); ;
            }
        }
        #endregion

        #region ensure constraints
        internal override void EnsureConstraints(CommandOption option)
        {
            if (_constraints is null || _constraints.Count == 0)
                return;

            foreach (var c in _constraints)
            {
                c.Ensure(option);
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
