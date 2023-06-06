using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace Crypto.CommandLine
{
    public class CommandDefinition
    {
        #region internals
        private string _name;
        private string _help;
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

        public IList<CommandOptionDefinition> Options
        { get => _options is null ? _options = new List<CommandOptionDefinition>() : _options; }

        internal CommandOptionDefinition this[string key]
        {
            get
            {
                var op = _options.Find(o => o.Key == key);

                if (op == default)
                    throw new KeyNotFoundException($"Provided option key: '{key}' not found.");

                return op;
            }
        }

        private List<CommandConstraint> Constraints
        {
            get => _constraints is null ? _constraints = new List<CommandConstraint>() : _constraints;
            set => _constraints = value;
        }

        public static string DefaultCommandName => "default";
        #endregion

        #region constructors
        public CommandDefinition(string name)
        {
            _name = name;
        }
        #endregion

        #region add option
        public void AddOption(string key, OptionType type, params string[] flags)
        {
            this.AddOption(key: key, null, type: type, flags);
        }

        public void AddOption(string key, string help, OptionType type, params string[] flags)
        {
            this.AddOption(key, false, help, type, flags);
        }

        public void AddOption(string key, bool mustAssign, string help, OptionType type, params string[] flags)
        {
            switch (type)
            {
                case OptionType.String:
                    this.AddOptionOf<string>(key, mustAssign, help, Convert.ToString, flags);
                    break;
                case OptionType.Boolean:
                    this.AddOptionOf<bool>(key, mustAssign, help, BooleanConverter.ConvertToBoolean, flags);
                    break;
                case OptionType.Char:
                    this.AddOptionOf<char>(key, mustAssign, help, Convert.ToChar, flags);
                    break;
                case OptionType.Byte:
                    this.AddOptionOf<byte>(key, mustAssign, help, Convert.ToByte, flags);
                    break;
                case OptionType.SByte:
                    this.AddOptionOf<sbyte>(key, mustAssign, help, Convert.ToSByte, flags);
                    break;
                case OptionType.Short:
                    this.AddOptionOf<short>(key, mustAssign, help, Convert.ToInt16, flags);
                    break;
                case OptionType.UShort:
                    this.AddOptionOf<ushort>(key, mustAssign, help, Convert.ToUInt16, flags);
                    break;
                case OptionType.Int32:
                    this.AddOptionOf<int>(key, mustAssign, help, Convert.ToInt32, flags);
                    break;
                case OptionType.UInt32:
                    this.AddOptionOf<uint>(key, mustAssign, help, Convert.ToUInt32, flags);
                    break;
                case OptionType.Int64:
                    this.AddOptionOf<long>(key, mustAssign, help, Convert.ToInt64, flags);
                    break;
                case OptionType.UInt64:
                    this.AddOptionOf<ulong>(key, mustAssign, help, Convert.ToUInt64, flags);
                    break;
                case OptionType.NInt:
                    this.AddOptionOf<nint>(key, mustAssign, help, nint.Parse, flags);
                    break;
                case OptionType.NUInt:
                    this.AddOptionOf<nuint>(key, mustAssign, help, nuint.Parse, flags);
                    break;
                case OptionType.Float:
                    this.AddOptionOf<float>(key, mustAssign, help, Convert.ToSingle, flags);
                    break;
                case OptionType.Double:
                    this.AddOptionOf<double>(key, mustAssign, help, Convert.ToDouble, flags);
                    break;
                case OptionType.Decimal:
                    this.AddOptionOf<decimal>(key, mustAssign, help, Convert.ToDecimal, flags);
                    break;
                case OptionType.DateTime:
                    this.AddOptionOf<DateTime>(key, mustAssign, help, Convert.ToDateTime, flags);
                    break;
                case OptionType.Guid:
                    this.AddOptionOf<Guid>(key, mustAssign, help, Guid.Parse, flags);
                    break;
                default:
                    throw new InvalidOperationException($"Encountered un-expected {nameof(OptionType)}: {type}");
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

        #region must assign one of
        public void MustAssignOneOf(bool mutuallyExclusive, params string[] optionKeys)
        {
            if (optionKeys is null)
                throw new ArgumentNullException(nameof(optionKeys));

            if (optionKeys.Length < 2)
                throw new ArgumentException("Argument must contain at least 2 values.", nameof(optionKeys));

            for (int i = 0; i < optionKeys.Length; i++)
            {
                var opDef = this[optionKeys[i]];

                if (opDef.MustAssign)
                    throw new CommandDefinitionException($"Option '{optionKeys[i]}' is marked '{nameof(CommandOptionDefinition.MustAssign)}'...'must assign one of' rule cannot be applied.");
            }

            var constraint = new MustAssignOneOfConstraint(mutuallyExclusive, optionKeys);

            this.Constraints.Add(constraint);
        }
        #endregion

        #region apply constraint
        //public void ApplyConstraint(Func<IConstrainedCommand, bool> constraint, string error)
        //{
        //    if (constraint is null)
        //        throw new ArgumentNullException(nameof(constraint));

        //    if (error is null)
        //        throw new ArgumentNullException(nameof(error));

        //    if (error == string.Empty)
        //        throw new ArgumentException("Argument must contain a value.", nameof(error));

        //    var customConstraint = new CommandConstraint(constraint, error);

        //    this.Constraints.Add(customConstraint);
        //}
        #endregion

        #region ensure constraints
        internal void EnsureConstraints(Command command)
        {
            if (_constraints is null || _constraints.Count == 0)
                return;

            foreach (var c in _constraints)
            {
                c.Ensure(command);
            }
        }
        #endregion

        #region validate
        internal void Validate()
        {
            if (_name is null || _name == string.Empty)
                throw new CommandDefinitionException($"{nameof(CommandDefinition)} must be provided a value for {nameof(Name)}.");

            if (_name[0] == '-')
                throw new CommandDefinitionException($"{nameof(CommandDefinition)} key cannot start with a '-'.");

            if (_handler is null)
                throw new CommandDefinitionException($"{nameof(CommandDefinition)} must be provided a value for {nameof(Handler)}.");

            if (_options is not null && _options.Count > 0)
            {
                foreach (var op in _options)
                {
                    op.Validate();
                }
            }
        }
        #endregion
    }
}
