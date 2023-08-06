using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace HatTrick.CommandLine
{
    public class SetOf<T> : IEnumerable<T>
    {
        #region internals
        private T[] _of;
        private int _length;
        #endregion

        #region interface
        public int Length => _length;

        public T this[int i]
        {
            get => _of[i];
            set => _of[i] = value;
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

            if (_of is null)
            {
                _of = new T[1];
            }
            else
            {
                var newOps = new T[_length + 1];
                Array.Copy(_of, newOps, _length);
                _of = newOps;
            }

            _of[_length++] = option;
        }
        #endregion

        #region exists
        public bool Exists(Predicate<T> where)
        {
            if (_of is null)
                return false;

            return Array.Exists(_of, where);
        }
        #endregion

        #region find index
        public int FindIndex(Predicate<T> where)
        {
            if (_of is null)
                return -1;

            return Array.FindIndex(_of, where);
        }
        #endregion

        #region find
        public T Find(Predicate<T> where)
        {
            if (_of is null)
                return default;

            return Array.Find(_of, where);
        }
        #endregion

        #region find all
        public T[] FindAll(Predicate<T> where)
        {
            if (_of is null)
                return new T[0];

            return Array.FindAll(_of, where);
        }
        #endregion

        #region max
        public int Max(Func<T, int> given)
        {
            if (_of is null || _of.Length == 0)
                return 0;

            int max = _of.Max(given);
            return max;
        }
        #endregion

        #region get enumerator
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return ((IEnumerable<T>)_of).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _of.GetEnumerator();
        }
        #endregion
    }
}
