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
        private SetOf<Option> _ops;
        #endregion

        #region interface
        public string Key => _key;

        internal SetOf<Option> Options => _ops;

        public Option this[string key]
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
        internal Command(string key, SetOf<Option> options = null)
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
        internal void ApplyDefaultOption(DefaultOption option)
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
        internal void ApplyEmptyOption(EmptyOption option)
        {
            _ops.Add(option);
        }
        #endregion

        #region get options
        public Option[] GetOptions(Predicate<Option> where = null)
        {
            Option[] ops = _ops.FindAll(where == null ? (o) => true : where);
            return ops;
        }
        #endregion

        #region get option
        //public CommandOption GetOption(string optionKey)
        //{
        //    CommandOption op = _ops.Find(o => o.Key == optionKey);
        //    return op;
        //}
        #endregion

        #region get option by ref
        internal ref Option GetOptionByRef(string optionKey)
        {
            int i = _ops.FindIndex(o => o.Key == optionKey);
            
            if (i < 0)
                throw new KeyNotFoundException($"Provided option key: {optionKey} not found.");

            return ref _ops.GetPointerTo(i);
        }
        #endregion

        #region get option by flag
        public Option GetOptionByFlag(params string[] flags)
        {
            Option op = _ops.Find(o => flags.Contains(o.Flag));
            return op;
        }
        #endregion
    }
}
