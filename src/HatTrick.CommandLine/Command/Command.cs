using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        //internal void ApplyDefaultOption(DefaultOption option)
        //{
        //    //we swap in default for empty when default constraint exists.
        //    int idx = _ops.FindIndex((o) => o.Key == option.Key);

        //    if (idx > -1)
        //        _ops[idx] = option;
        //    else
        //        _ops.Add(option);
        //}
        #endregion

        #region add empty option
        internal void AddEmptyOption(EmptyOption option)
        {
            int idx = _ops.FindIndex((o) => o.Key == option.Key);
            if (idx > -1)
                throw new ArgumentException($"Duplication option key exists...Cannot add option with key: {option.Key}");

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

        #region get option by flag
        public Option GetOptionByFlag(params string[] flags)
        {
            if (flags is null)
                throw new ArgumentNullException(nameof(flags));

            if (flags.Length == 0)
                throw new ArgumentException("Argument cannot be empty.", nameof(flags));

            Option op = _ops.Find(o => flags.Contains(o.Flag));
            return op;
        }
        #endregion

        #region get option by ref
        internal ref Option GetOptionByRef(string key)
        {
            if (key is null)
                throw new ArgumentNullException(nameof(key));

            if (key == string.Empty)
                throw new ArgumentException("Argument cannot be empty.", nameof(key));

            int i = _ops.FindIndex(o => o.Key == key);
            
            if (i < 0)
                throw new KeyNotFoundException($"Provided option key: {key} not found.");

            return ref _ops.GetPointerTo(i);
        }
        #endregion
    }
}
