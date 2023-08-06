using System;

namespace HatTrick.CommandLine
{
    public class SetOf<T>
    {
        #region internals
        private T[] _ops;
        private int _cnt;
        #endregion

        #region interface
        public int Count => _cnt;

        public T this[int i]
        {
            get => _ops[i];
            set => _ops[i] = value;
        }
        #endregion

        #region constructors
        public SetOf()
        { }
        #endregion

        #region add
        public void Add(T option)
        {
            if (option is null)
                throw new ArgumentNullException(nameof(option));

            if (_ops is null)
            {
                _ops = new T[1];
            }
            else
            {
                var newOps = new T[_cnt + 1];
                Array.Copy(_ops, newOps, _cnt);
                _ops = newOps;
            }

            _ops[_cnt++] = option;
        }
        #endregion

        #region exists
        public bool Exists(Predicate<T> where)
        {
            if (_ops is null)
                return false;

            return Array.Exists(_ops, where);
        }
        #endregion

        #region find index
        public int FindIndex(Predicate<T> where)
        {
            if (_ops is null)
                return -1;

            return Array.FindIndex(_ops, where);
        }
        #endregion

        #region find
        public T Find(Predicate<T> where)
        {
            if (_ops is null)
                return default;

            return Array.Find(_ops, where);
        }
        #endregion

        #region find all
        public T[] FindAll(Predicate<T> where)
        {
            if (_ops is null)
                return new T[0];

            return Array.FindAll(_ops, where);
        }
        #endregion
    }
}
