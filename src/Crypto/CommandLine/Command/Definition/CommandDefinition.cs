using System;
using System.Collections.Generic;
using System.Linq;

namespace Crypto.CommandLine
{
    public class CommandDefinition
    {
        #region internals
        private string _commandKey;
        private string _help;
        private Action<Command> _entryPoint;
        private List<CommandOptionDefinition> _options;
        #endregion

        #region interface
        public string Key
        {
            get { return _commandKey; }
            set { _commandKey = value; }
        }

        public string Help
        {
            get { return _help; }
            set { _help = value; }
        }

        public Action<Command> EntryPoint
        {
            get { return _entryPoint; }
            set { _entryPoint = value; }
        }

        public Func<Command, System.Threading.Tasks.Task> AsyncEntryPoint
        { get; set; }

        public List<CommandOptionDefinition> Options
        {
            get { return _options is null ? _options = new List<CommandOptionDefinition>() : _options; }
            protected set { _options = value; }
        }
        #endregion

        #region constructors
        public CommandDefinition()
        {
        }

        public CommandDefinition(string commandKey, string help, Action<Command> entryPoint, params CommandOptionDefinition[] options)
        {
            _commandKey = commandKey;
            _help = help;
            _entryPoint = entryPoint;
            _options = options.ToList();
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

        #region ensure
        internal void Ensure()
        {
            if (_commandKey is null || _commandKey == string.Empty)
                throw new CommandDefinitionException($"{nameof(CommandDefinition)} must be provided a value for {nameof(Key)}.");

            if (_commandKey[0] == '-')
                throw new CommandDefinitionException($"{nameof(CommandDefinition)} key cannot start with a '-'.");

            if (_entryPoint is null)
                throw new CommandDefinitionException($"{nameof(CommandDefinition)} must be provided a value for {nameof(EntryPoint)}.");

            if (_options is not null && _options.Count > 0)
            {
                foreach (var op in _options)
                {
                    op.Ensure();
                }
            }
        }
        #endregion
    }
}
