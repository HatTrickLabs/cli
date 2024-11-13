using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Transactions;

namespace HatTrick.CommandLine
{
    public class Command : ICommand, IPreEnsureCommand
    {
        #region internals
        private string _name;
        private SetOf<Option> _ops;
        #endregion

        #region interface
        public string Name => _name;

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

        IOption ICommand.this[string key] => this[key] as IOption;
        #endregion

        #region constructors
        internal Command(string name, params Option[] options)
        {
            if (name is null)
                throw new ArgumentNullException(nameof(name));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException($"{nameof(name)} argument must contain a value.");

            _name = name;
            _ops = new SetOf<Option>(options) ?? new();
        }
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

        IOption ICommand.GetOption(Predicate<IOption> where)
        {
            if (where is null)
                throw new ArgumentNullException(nameof(where));

            var op = _ops.Find(where);
            return (IOption)op;
        }

        IPreEnsureOption IPreEnsureCommand.GetOption(Predicate<IPreEnsureOption> where)
        {
            if (where is null)
                throw new ArgumentNullException(nameof(where));

            var op = _ops.Find(where);
            return (IPreEnsureOption)op;
        }
        #endregion

        #region get option by flag
        public Option GetOptionByFlag(params string[] flags)
        {
            if (flags is null)
                throw new ArgumentNullException(nameof(flags));

            if (flags.Length == 0)
                throw new ArgumentException("Argument cannot be empty.", nameof(flags));

            Option op = _ops.Find(o => Array.Exists(flags, (f) => f == o.Flag));
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