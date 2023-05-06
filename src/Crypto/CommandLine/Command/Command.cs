using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;

namespace Crypto.CommandLine
{
    public class Command
    {
        #region internals
        private string _key;
        private IList<CommandOption> _ops;
        #endregion

        #region interface
        public string Key => _key;

        public IList<CommandOption> Options
        {
            get => _ops;
            internal set => _ops = value;
        }

        public CommandOption this[string key]
        {
            get
            {
                var op = _ops.FirstOrDefault(o => o.Key == key);

                if (op == default)
                    throw new KeyNotFoundException($"Provided option key: {key} not found.");

                return op;
            }
        }
        #endregion

        #region constructors
        public Command(string key, IList<CommandOption> options = null)
        {
            if (key is null)
                throw new ArgumentNullException(nameof(key));

            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException($"{nameof(key)} argument must contain a value.");

            _key = key;
            _ops = options ?? new List<CommandOption>(0);
        }
        #endregion

        #region get options
        public IList<CommandOption> GetOptions(Predicate<CommandOption> where = null)
        {
            List<CommandOption> ops = _ops.ToList().FindAll(where == null ? (o) => true : where);
            return ops;
        }
        #endregion

        #region get option
        public CommandOption GetOption(string optionKey)
        {
            CommandOption op = this.Options.ToList().Find(o => o.Key == optionKey);
            return op;
        }
        #endregion
    }
}
