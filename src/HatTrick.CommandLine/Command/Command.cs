using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

namespace HatTrick.CommandLine
{
    public class Command : IConstrainedCommand
    {
        #region internals
        private string _key;
        private CommandOptionSet _ops;
        #endregion

        #region interface
        public string Key => _key;

        internal CommandOptionSet Options => _ops;

        public CommandOption this[string key]
        {
            get
            {
                var op = _ops.Find(o => o.Key == key);

                if (op == default)
                    throw new KeyNotFoundException($"Provided option key: {key} not found.");

                return op;
            }
        }
        #endregion

        #region constructors
        internal Command(string key, CommandOptionSet options = null)
        {
            if (key is null)
                throw new ArgumentNullException(nameof(key));

            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException($"{nameof(key)} argument must contain a value.");

            _key = key;
            _ops = options ?? new();
        }
        #endregion

        #region apply default option
        internal void ApplyDefaultOption(DefaultCommandOption option)
        {
            //we swap in default for empty when default constraint exists.
            int idx = _ops.FindIndex((o) => o.Key == option.Key);
            if (idx > -1)
                _ops[idx] = option;
            else
                _ops.Add(option);
        }
        #endregion

        #region apply empty option
        internal void ApplyEmptyOption(EmptyCommandOption option)
        {
            _ops.Add(option);
        }
        #endregion

        #region get options
        public CommandOption[] GetOptions(Predicate<CommandOption> where = null)
        {
            CommandOption[] ops = _ops.FindAll(where == null ? (o) => true : where);
            return ops;
        }
        #endregion

        #region get option
        public CommandOption GetOption(string optionKey)
        {
            CommandOption op = _ops.Find(o => o.Key == optionKey);
            return op;
        }
        #endregion

        #region get option by flag
        public CommandOption GetOptionByFlag(params string[] flags)
        {
            CommandOption op = _ops.Find(o => flags.Contains(o.Flag));
            return op;
        }
        #endregion
    }
}
