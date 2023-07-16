using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace HatTrick.CommandLine
{
    public class CommandDefinition
    {
        #region const
        public const int MaxNameLength = 40;
        #endregion

        #region internals
        private string _name;
        private string _help;
        private int _depth;
        private bool _hidden;
        private Action _mappedHanderValidators;
        private Action<Command> _handler;
        private Func<Command, Task> _asyncHandler;
        private List<CommandOptionDefinition> _options;
        private List<CommandConstraint> _constraints;
        #endregion

        #region interface
        public string Name => _name;

        public string Help
        {
            get => _help;
            set => _help = value;
        }

        public bool Hidden => _hidden;

        internal int Depth => _depth;

        public Action<Command> Handler
        {
            get => _handler;
            set => _handler = value;
        }

        public Func<Command, Task> AsyncHandler
        {
            get => _asyncHandler;
            set => _asyncHandler = value;
        }

        public List<CommandOptionDefinition> Options
        { get => _options is null ? _options = new List<CommandOptionDefinition>() : _options; }

        public CommandOptionDefinition this[string key]
        {
            get
            {
                var op = _options.Find(o => o.Key == key);

                if (op == default)
                    throw new KeyNotFoundException($"Provided option key: '{key}' not found.");

                return op;
            }
        }

        public List<CommandConstraint> Constraints
        {
            get => _constraints is null ? _constraints = new List<CommandConstraint>() : _constraints;
            set => _constraints = value;
        }

        public bool HasConstraints => _constraints is not null;

        public static string DefaultCommandName => "usage-help";
        #endregion

        #region constructors
        public CommandDefinition(string name)
        {
            _name = name;
        }
        #endregion

        #region hide
        public void Hide()
        {
            _hidden = true;
        }
        #endregion

        #region add option
        public void AddOption(string key, OpType type, params string[] flags)
        {
            this.AddOption(key: key, null, type: type, flags);
        }

        public void AddOption(string key, string help, OpType type, params string[] flags)
        {
            this.AddOption(key, false, help, type, flags);
        }

        public void AddOption(string key, bool mustAssign, string help, OpType type, params string[] flags)
        {
            switch (type)
            {
                case OpType.String:
                    this.AddOptionOf<string>(key, mustAssign, help, Convert.ToString, flags);
                    break;
                case OpType.Bool:
                    this.AddOptionOf<bool>(key, mustAssign, help, BooleanConverter.ConvertToBoolean, flags);
                    break;
                case OpType.Char:
                    this.AddOptionOf<char>(key, mustAssign, help, Convert.ToChar, flags);
                    break;
                case OpType.Byte:
                    this.AddOptionOf<byte>(key, mustAssign, help, Convert.ToByte, flags);
                    break;
                case OpType.SByte:
                    this.AddOptionOf<sbyte>(key, mustAssign, help, Convert.ToSByte, flags);
                    break;
                case OpType.Short:
                    this.AddOptionOf<short>(key, mustAssign, help, Convert.ToInt16, flags);
                    break;
                case OpType.UShort:
                    this.AddOptionOf<ushort>(key, mustAssign, help, Convert.ToUInt16, flags);
                    break;
                case OpType.Int32:
                    this.AddOptionOf<int>(key, mustAssign, help, Convert.ToInt32, flags);
                    break;
                case OpType.UInt32:
                    this.AddOptionOf<uint>(key, mustAssign, help, Convert.ToUInt32, flags);
                    break;
                case OpType.Int64:
                    this.AddOptionOf<long>(key, mustAssign, help, Convert.ToInt64, flags);
                    break;
                case OpType.UInt64:
                    this.AddOptionOf<ulong>(key, mustAssign, help, Convert.ToUInt64, flags);
                    break;
                case OpType.NInt:
                    this.AddOptionOf<nint>(key, mustAssign, help, nint.Parse, flags);
                    break;
                case OpType.NUInt:
                    this.AddOptionOf<nuint>(key, mustAssign, help, nuint.Parse, flags);
                    break;
                case OpType.Float:
                    this.AddOptionOf<float>(key, mustAssign, help, Convert.ToSingle, flags);
                    break;
                case OpType.Double:
                    this.AddOptionOf<double>(key, mustAssign, help, Convert.ToDouble, flags);
                    break;
                case OpType.Decimal:
                    this.AddOptionOf<decimal>(key, mustAssign, help, Convert.ToDecimal, flags);
                    break;
                case OpType.DateTime:
                    this.AddOptionOf<DateTime>(key, mustAssign, help, Convert.ToDateTime, flags);
                    break;
                case OpType.Guid:
                    this.AddOptionOf<Guid>(key, mustAssign, help, Guid.Parse, flags);
                    break;
                default:
                    throw new InvalidOperationException($"Encountered un-expected {nameof(OpType)}: {type}");
            }
        }
        #endregion

        #region add option of T
        public void AddOptionOf<T>(string key, Func<string, T> converter, params string[] flags)
        {
            this.AddOptionOf<T>(key: key, help: null, converter: converter, flags: flags);
        }

        public void AddOptionOf<T>(string key, string help, Func<string, T> converter, params string[] flags)
        {
            this.AddOptionOf<T>(key: key, mustAssign: false, help: help, converter: converter, flags: flags);
        }

        public void AddOptionOf<T>(string key, bool mustAssign, string help, Func<string, T> converter, params string[] flags)
        {
            var op = new CommandOptionDefinition<T>(key: key, converter: converter, mustAssign: mustAssign, help: help, flags: flags);
            this.Options.Add(op);
        }
        #endregion

        #region get option
        internal CommandOptionDefinition GetOption(string key)
        {
            var op = _options.Find(o => o.Key == key);
            return op;
        }
        #endregion

        #region must assign one of
        public void MustAssignOneOf(params string[] optionKeys)
        {
            if (optionKeys is null)
                throw new ArgumentNullException(nameof(optionKeys));

            if (optionKeys.Length < 2)
                throw new ArgumentException("Argument must contain at least 2 values.", nameof(optionKeys));

            var opDefKeys = new (string key, string flag)[optionKeys.Length];
            for (int i = 0; i < optionKeys.Length; i++)
            {
                var opDef = this[optionKeys[i]];

                bool mustAssign = opDef.HasConstraints && opDef.Constraints.Exists(c => c is MustAssignConstraint);

                if (mustAssign)
                    throw new CommandDefinitionException($"Option '{opDef.Key}' has a '{MustAssignConstraint.ConstraintName}' constraint...'{MustAssignOneOfConstraint.ConstraintName}' constraint cannot be applied.");

                opDefKeys[i] = (optionKeys[i], opDef.MostVerboseFlag);
            }

            var constraint = new MustAssignOneOfConstraint(opDefKeys);

            this.Constraints.Add(constraint);
        }
        #endregion

        #region mutually exclusive set
        public void MutaullyExclusiveSet(params string[] optionKeys)
        {
            if (optionKeys is null)
                throw new ArgumentNullException(nameof(optionKeys));

            if (optionKeys.Length < 2)
                throw new ArgumentException("Argument must contain at least 2 values.", nameof(optionKeys));

            var opDefKeys = new (string key, string flag)[optionKeys.Length];
            for (int i = 0; i < optionKeys.Length; i++)
            {
                var opDef = this[optionKeys[i]];

                bool mustAssign = opDef.HasConstraints && opDef.Constraints.Exists(c => c is MustAssignConstraint);

                if (mustAssign)
                    throw new CommandDefinitionException($"Option '{opDef.Key}' has a '{MustAssignConstraint.ConstraintName}' constraint...'{MutuallyExclusiveSetConstraint.ConstraintName}' constraint cannot be applied.");

                opDefKeys[i] = (optionKeys[i], opDef.MostVerboseFlag);
            }

            var constraint = new MutuallyExclusiveSetConstraint(opDefKeys);

            this.Constraints.Add(constraint);
        }
        #endregion

        #region apply constraint
        public void ApplyConstraint(Func<IConstrainedCommand, bool> constraint, string name, string description)
        {
            if (constraint is null)
                throw new ArgumentNullException(nameof(constraint));

            if (name is null)
                throw new ArgumentNullException(nameof(name));

            if (description is null)
                throw new ArgumentNullException(nameof(description));

            if (name == string.Empty)
                throw new ArgumentException("Argument must contain a value.", nameof(name));

            if (description == string.Empty)
                throw new ArgumentException("Argument must contain a value.", nameof(description));

            var customConstraint = new CommandConstraint(constraint, name, description);

            this.Constraints.Add(customConstraint);
        }
        #endregion

        #region ensure constraints
        internal void EnsureConstraints(Command command, ref List<string> feedback)
        {
            if (_constraints is null || _constraints.Count == 0)
                return;

            foreach (var c in _constraints)
            {
                if (!c.Ensure(command, out string fb))
                {
                    feedback.Add(fb);
                }
            }
        }
        #endregion

        #region register mapped handler validator
        internal void RegisterMappedHandlerValidator(Action validator)
        {
            _mappedHanderValidators += validator;
        }
        #endregion

        #region validate
        internal void Validate()
        {
            if (_name is null || _name == string.Empty)
                throw new CommandDefinitionException($"{nameof(CommandDefinition)} must be provided a value for {nameof(Name)}.");

            if (_name[0] == '-')
                throw new CommandDefinitionException($"{nameof(CommandDefinition)} {nameof(Name)} cannot start with a '-'.");

            if (_name.Length > CommandDefinition.MaxNameLength)
                throw new CommandDefinitionException($"{nameof(CommandDefinition)}.{nameof(Name)}...max accepted char length is {CommandDefinition.MaxNameLength}.");

            if (_handler is null && _asyncHandler is null)
                throw new CommandDefinitionException($"{nameof(CommandDefinition)} must be provided a value for one of: {nameof(Handler)}, {nameof(AsyncHandler)}.");

            int depth = 0;
            for (int i = 1; i < _name.Length; i++)
            {
                char c = _name[i];
                if (!(char.IsLetter(c) || char.IsDigit(c) || c == '.' || c == '-'))
                    throw new NamespaceDefinitionException($"{nameof(CommandDefinition)}.{nameof(Name)} can only contain letters, digits, '-' and '.'");

                if (c == '.')
                    depth += 1;
            }

            _depth = depth;

            if (_options is not null && _options.Count > 0)
            {
                foreach (var op in _options)
                {
                    op.Validate();
                }
            }

            _mappedHanderValidators?.Invoke();
        }
        #endregion
    }
}
