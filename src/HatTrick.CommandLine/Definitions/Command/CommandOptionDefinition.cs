using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Reflection;

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
        private readonly string _help;
        private bool _hidden;
        private readonly string[] _flags;
        private List<ArgumentConstraint> _constraints;
        #endregion

        #region interface
        public string Key => _key;

        public abstract Type GenericType { get; }

        public string Help => _help;

        public bool Hidden => _hidden;

        public string[] Flags => _flags;

        public string MostVerboseFlag => _flags.MaxBy((f) => f.Length);

        public string LeastVerboseFlag => _flags.MinBy((f) => f.Length);

        public List<ArgumentConstraint> Constraints
        {
            get => _constraints is null ? _constraints = new List<ArgumentConstraint>() : _constraints;
            set => _constraints = value;
        }

        public bool HasConstraints => _constraints is not null;

        public bool HasDefault => this.HasConstraints && _constraints.Exists(c => c is IDefaultConstraint);

        public bool MustAssign => this.HasConstraints && _constraints.Exists(c => c is IMustAssignConstraint);
        #endregion

        #region constructors
        protected CommandOptionDefinition(string key, string help, params string[] flags)
        {
            _key = key ?? throw new ArgumentNullException(nameof(key));
            _help = help;
            _flags = flags ?? throw new ArgumentNullException(nameof(flags));
        }
        #endregion

        #region of T
        private CommandOptionDefinition<T> Of<T>()
        {
            if (this is CommandOptionDefinition<T> cmdOpDefT)
            {
                return cmdOpDefT;
            }
            else
            {
                var name = nameof(CommandOptionDefinition);
                var classGenericName = TypeMap.GetAliasOrName(this.GenericType);
                var localGenericName = TypeMap.GetAliasOrName(typeof(T));
                var msg = $"Cannot cast {name}<{classGenericName}> to {name}<{localGenericName}>";
                throw new InvalidCastException(msg);
            }
        }
        #endregion

        #region hide
        public void Hide()
        {
            _hidden = true;
        }
        #endregion

        #region accepted values
        public void AcceptedValues<T>(params T[] values)
        {
            try
            {
                this.Of<T>().AcceptedValues(values);
            }
            catch (InvalidCastException ice)
            {
                var vals = string.Join("|", values);
                var name = nameof(CommandOptionDefinition);
                var classGenericName = TypeMap.GetAliasOrName(this.GenericType);
                var localGenericName = TypeMap.GetAliasOrName(typeof(T));
                var msg = $"Cannot set accepted values '{vals}' of type {localGenericName} for {name}<{classGenericName}>.";
                throw new CommandDefinitionException(msg, ice);
            }
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

            try
            {
                this.Of<T>().ApplyConstraint(constraint, name, description);
            }
            catch (InvalidCastException ice)
            {
                var className = nameof(CommandOptionDefinition);
                var classGenericName = TypeMap.GetAliasOrName(this.GenericType);
                var localGenericName = TypeMap.GetAliasOrName(typeof(T));
                string msg = $"Cannot apply constraint of type {localGenericName} to {className}<{classGenericName}>. ";
                throw new CommandDefinitionException(msg, ice);
            }
        }
        #endregion

        #region set converted value
        internal abstract void SetConvertedValue(CommandOption option);
        #endregion

        #region ensure custom constraints
        internal void EnsureConstraints(ref CommandOption option, ref List<string> feedback)
        {
            if (!this.HasConstraints)
                return;

            foreach (var c in this.Constraints)
            {
                if (!c.Ensure(ref option, out string fb))
                    feedback.Add(fb);
            }
        }
        #endregion

        #region empty instance
        internal EmptyCommandOption EmptyInstance()
        {
            var op = new EmptyCommandOption(_key, this.MostVerboseFlag);
            return op;
        }
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
                //TODO: may rethink this...quick prototypes may not want to provide more than 1 flag per op.
                if (string.IsNullOrWhiteSpace(flag))
                    throw new CommandDefinitionException($"Options[{_key}] contains a flag that is null or empty.");

                if (flag[0] != '-')
                    throw new CommandDefinitionException($"Option flags must begin with a '-'...'{flag}' is not valid.");

                if (flag[1] == '-') //verbose definition
                {
                    if (flag.Length < 4)
                        throw new CommandDefinitionException($"Verbose option flags begin with '--' and must be longer than 1 additional char...'{flag}' is not valid.");

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
        #endregion

        #region interface
        public override Type GenericType => this.GetGenericType();
        #endregion

        #region constructors
        internal CommandOptionDefinition(string key, string help, Func<string, T> converter, params string[] flags) 
                                  : base(key: key, help: help, flags: flags)
        {
            _converter = converter ?? throw new ArgumentNullException(nameof(converter));
            this.Constraints.Add(new MustAssignConstraint<T>(this.Flags));
        }

        internal CommandOptionDefinition(string key, T defaultArg, string help, Func<string, T> converter, params string[] flags) 
                                  : base(key: key, help: help, flags: flags)
        {
            _converter = converter ?? throw new ArgumentNullException(nameof(converter));
            base.Constraints.Add(new DefaultConstraint<T>(key, base.MostVerboseFlag, defaultArg));
        }
        #endregion

        #region get generic type
        private Type GetGenericType()
        {
            var type = _genericType is null 
                ? _genericType = this.GetType().GetGenericArguments()[0] 
                : _genericType;

            return type;
        }
        #endregion

        #region accepted values
        public void AcceptedValues(T[] values)
        {
            if (values is null)
                throw new ArgumentNullException(nameof(values));

            if (base.HasConstraints)
            {
                List<ArgumentConstraint> constraints = base.Constraints;

                if (constraints.Count > 0)
                {
                    DefaultConstraint<T> dc = constraints.Find(c => c is IDefaultConstraint) as DefaultConstraint<T>;
                    EqualityComparer<T> comparer = EqualityComparer<T>.Default;
                    if (dc is not null && !Array.Exists<T>(values, (v) => comparer.Equals(v, dc.DefaultValue)))
                    {
                        var msg = $"Option '{this.Key}' is defined with a default value of '{dc.DefaultValue}' which is not defined in the accepted values set '{string.Join("|", values)}'. ";
                        throw new ArgumentException(msg);
                    }

                    var avc = constraints.Find(c => c is AcceptedValuesConstraint<T>);
                    if (avc is not null)
                        constraints.Remove(avc);
                }
            }

            if (values.Length == 0)
                return;

            base.Constraints.Add(new AcceptedValuesConstraint<T>(values));
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

            base.Constraints.Add(new ArgumentConstraint<T>(constraint, name, description));
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
                throw new CommandArgumentException($"Option '{flag}' requires argument of type '{name}'...invalid value provided: '{arg}'"); ;
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
