using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Threading;

namespace HatTrick.CommandLine
{
    #region option definition
    public abstract class OptionDefinition
    {
        #region internals
        private readonly string _key;
        private readonly string _help;
        private Func<string, object> _converter;
        private bool _hidden;
        private readonly OptionFlags _flags;
        private SetOf<ArgumentConstraint> _constraints;
        #endregion

        #region interface
        public static readonly int MaxKeyLength;

        public static readonly int MaxFlagLength;

        public string Key => _key;

        public abstract Type GenericType { get; }

        public string Help => _help;

        public bool Hidden => _hidden;

        public OptionFlags Flags => _flags;

        internal SetOf<ArgumentConstraint> Constraints => _constraints;

        public bool HasConstraints => _constraints.Length > 0;

        public bool HasDefault => this.HasConstraints && _constraints.Exists(c => c is IDefaultConstraint);

        public bool MustAssign => this.HasConstraints && _constraints.Exists(c => c is IMustAssignConstraint);
        #endregion

        #region constructors
        static OptionDefinition()
        {
            MaxKeyLength = 32;
            MaxFlagLength = 32;
        }

        protected OptionDefinition(string key, string help, Func<string, object> converter, (string terse, string verbose) flags)
        {
            if (key is null)
                throw new ArgumentNullException(nameof(key));

            if (help is null)
                throw new ArgumentNullException(nameof(help));

            if (flags == default)
                 throw new ArgumentNullException(nameof(flags));

            if (converter is null)
                throw new ArgumentNullException(nameof(converter));

            _key = key;
            _help = help;
            _flags = flags;
            _converter = converter;
            _constraints = new SetOf<ArgumentConstraint>();
        }
        #endregion

        #region of T
        private OptionDefinition<T> Of<T>()
        {
            if (this is OptionDefinition<T> cmdOpDefT)
            {
                return cmdOpDefT;
            }
            else
            {
                var name = nameof(OptionDefinition);
                var classGenericName = OptionTypeMap.GetAliasOrName(this.GenericType);
                var localGenericName = OptionTypeMap.GetAliasOrName(typeof(T));
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
                var name = nameof(OptionDefinition);
                var classGenericName = OptionTypeMap.GetAliasOrName(this.GenericType);
                var localGenericName = OptionTypeMap.GetAliasOrName(typeof(T));
                var msg = $"Cannot set accepted values '{vals}' of type {localGenericName} for {name}<{classGenericName}>.";
                throw new CommandDefinitionException(msg, ice);
            }
        }
        #endregion

        #region apply constraint
        public void ApplyConstraint<T>(Func<T, bool> constraint, string name, string description)
        {
            //null checks are done in the concrete type, no sense in doing them twice
            try
            {
                this.Of<T>().ApplyConstraint(constraint, name, description);
            }
            catch (InvalidCastException ice)
            {
                var className = nameof(OptionDefinition);
                var classGenericName = OptionTypeMap.GetAliasOrName(this.GenericType);
                var localGenericName = OptionTypeMap.GetAliasOrName(typeof(T));
                string msg = $"Cannot apply constraint of type {localGenericName} to {className}<{classGenericName}>. ";
                throw new CommandDefinitionException(msg, ice);
            }
        }
        #endregion

        #region get argument converter
        public virtual Func<string, object> GetArgumentConverter()
        {
            return _converter;
        }
        #endregion

        #region empty instance
        internal EmptyOption EmptyInstance()
        {
            var op = new EmptyOption(_key, this.Flags.Verbose);
            return op;
        }
        #endregion

        #region validate
        internal virtual void Validate()
        {
            //we did null checks in constructor, just check for empty
            if (_key == string.Empty)
                throw new CommandDefinitionException("All options must have a valid key...Provided key is empty.");

            if (_key.Length > OptionDefinition.MaxKeyLength)
                throw new CommandDefinitionException($"{nameof(OptionDefinition)}.{nameof(Key)}...max accepted char length is {OptionDefinition.MaxKeyLength}.");

            if (_help == string.Empty)
                throw new CommandDefinitionException($"Options[{_key}] must contain valid help...Provided value is empty.");

            if (_flags.Verbose == string.Empty)
                throw new CommandDefinitionException($"Options[{_key}] must contain at least 1 {nameof(OptionDefinition.Flags)}.");

            this.ValidateVerboseFlag();
            this.ValidateTerseFlag();
        }

        private void ValidateVerboseFlag()
        {
            //TODO: this should share logic with VerboseFlagToken.IsValid

            string verbose = _flags.Verbose;

            if (verbose.Length < 4)//must be two dashes and at least 2 additional chars
                throw new CommandDefinitionException($"Option '{_key}' contains an invalid flag...verbose flags must be at least 4 chars in length.");

            if (!(verbose[0] == '-' && verbose[1] == '-'))
                throw new CommandDefinitionException($"Option '{_key}' contains an invalid flag...verbose flags must start with 2 dashes.");

            if (verbose[2] == '-')
                throw new CommandDefinitionException($"Option '{_key} contains an invalid flag...verbose flags cannot start with 3 dashes.");

            if (verbose.Length > OptionDefinition.MaxFlagLength)
                throw new CommandDefinitionException($"Option '{_key}' contains an invalid flag...flag length '{verbose.Length}' is greater than max allowed '{OptionDefinition.MaxFlagLength}'. ");

            for (int i = 2; i < verbose.Length; i++)
            {
                char c = verbose[i];
                if (!(char.IsLetterOrDigit(c) || c == '-'))
                    throw new CommandDefinitionException($"Option '{_key}' contains an invalid flag...char '{c}' at index '{i}'...verbose flags can contain letters, digits and dashes.");
            }
        }

        private void ValidateTerseFlag()
        {
            //TODO: this should share logic with TerseFlagToken.IsValid

            string terse = _flags.Terse;
            if (terse is null || terse == string.Empty)
                return;//terse flag NOT required.

            if (terse.Length != 2)
                throw new CommandDefinitionException($"Option '{_key}' contains an invalid flag...terse flags start with a single dash followed by a single char.");

            if (terse[0] != '-')
                throw new CommandDefinitionException($"Option '{_key}' contains an invalid flag...terse flags must start with a single dash.");

            if (!(char.IsLetterOrDigit(terse[1])))
                throw new CommandDefinitionException($"Option '{_key}' contains an invalid flag...terse flags start with a dash followed by a single letter or digit.");
        }
        #endregion
    }
    #endregion

    #region option definition of T
    public class OptionDefinition<T> : OptionDefinition
    {
        #region internals
        private Type _genericType;
        private readonly Func<string, T> _converter;
        #endregion

        #region interface
        public override Type GenericType => this.GetGenericType();
        #endregion

        #region constructors
        internal OptionDefinition(string key, string help, Func<string, T> converter, (string terse, string verbose) flags)
                           : base(key: key, help: help, converter: (string arg) => converter(arg), flags: flags)
        {
            if (converter is null)
                throw new ArgumentNullException(nameof(converter));

            _converter = converter;

            if (typeof(T) == typeof(bool))//bool should never require assignment...should simply default to false
                base.Constraints.Add(new DefaultConstraint<bool>(key, base.Flags.Verbose, false));
            else
                this.Constraints.Add(new MustAssignConstraint<T>(this.Flags));
        }

        internal OptionDefinition(string key, T defaultArg, string help, Func<string, T> converter, (string terse, string verbose) flags)
                           : base(key: key, help: help, converter: (string arg) => converter(arg), flags: flags)
        {
            if (converter is null)
                throw new ArgumentNullException(nameof(converter));

            _converter = converter;

            base.Constraints.Add(new DefaultConstraint<T>(key, base.Flags.Verbose, defaultArg));
        }
        #endregion

        #region get generic type
        protected Type GetGenericType()
        {
            var type = _genericType is null
                ? _genericType = this.GetType().GetGenericArguments()[0]
                : _genericType;

            return type;
        }
        #endregion

        #region accepted values
        internal void AcceptedValues(T[] values)
        {
            if (values is null)
                throw new ArgumentNullException(nameof(values));

            if (values.Length == 0)
                throw new ArgumentException(nameof(values), "Argument must contain at least one value.");

            if (base.HasConstraints)
            {
                SetOf<ArgumentConstraint> constraints = base.Constraints;

                if (constraints.Length > 0)
                {
                    //if op def already contains an accepted values constraint, throw ex
                    var avc = constraints.Find(c => c is AcceptedValuesConstraint<T>);
                    if (avc is not null)
                        throw new CommandDefinitionException($"Option '{this.Key}' already contains an {nameof(this.AcceptedValues)} constraint.");

                    //if op def contains a default constraint, ensure the default is included in the accepted values set
                    DefaultConstraint<T> dc = constraints.Find(c => c is IDefaultConstraint) as DefaultConstraint<T>;
                    EqualityComparer<T> comparer = EqualityComparer<T>.Default;
                    if (dc is not null && !Array.Exists<T>(values, (v) => comparer.Equals(v, dc.DefaultValue)))
                    {
                        var msg = $"Option '{this.Key}' is defined with a default value of '{dc.DefaultValue}' which is not defined in the accepted values set '{string.Join("|", values)}'. ";
                        throw new CommandDefinitionException(msg);
                    }
                }
            }

            base.Constraints.Add(new AcceptedValuesConstraint<T>(values));
        }
        #endregion

        #region apply constraint
        internal void ApplyConstraint(Func<T, bool> constraint, string name, string description)
        {
            if (constraint is null)
                throw new ArgumentNullException(nameof(constraint));

            if (name is null)
                throw new ArgumentNullException(nameof(name));

            if (description is null)
                throw new ArgumentNullException(nameof(description));

            base.Constraints.Add(new CustomArgumentConstraint<T>(constraint, name, description));
        }
        #endregion

        #region get argument converter
        new public Func<string, T> GetArgumentConverter()
        {
            return _converter;
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
