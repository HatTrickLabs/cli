using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace HatTrick.CommandLine
{
    public class SetOf<T> : IEnumerable<T>
    {
        #region internals
        private T[] _items;
        private int _length;
        #endregion

        #region interface
        public int Length => _length;

        public T this[int i]
        {
            get => _items[i];
            set => _items[i] = value;
        }
        #endregion

        #region constructors
        public SetOf()
        { }
        #endregion

        #region get pointer to
        internal ref T GetPointerTo(int index)
        {
            return ref _items[index];
        }
        #endregion

        #region add
        public void Add(T item)
        {
            if (item is null)
                throw new ArgumentNullException(nameof(item));

            if (_items is null)
            {
                _items = new T[1];
            }
            else
            {
                var newItems = new T[_length + 1];
                Array.Copy(_items, newItems, _length);
                _items = newItems;
            }

            _items[_length++] = item;
        }
        #endregion

        #region exists
        public bool Exists(Predicate<T> where)
        {
            if (_items is null)
                return false;

            return Array.Exists(_items, where);
        }
        #endregion

        #region find index
        public int FindIndex(Predicate<T> where)
        {
            if (_items is null)
                return -1;

            return Array.FindIndex(_items, where);
        }
        #endregion

        #region find
        public T Find(Predicate<T> where)
        {
            if (_items is null)
                return default;

            return Array.Find(_items, where);
        }
        #endregion

        #region find all
        public T[] FindAll(Predicate<T> where)
        {
            if (_items is null)
                return new T[0];

            return Array.FindAll(_items, where);
        }
        #endregion

        #region max
        public int Max(Func<T, int> given)
        {
            if (_items is null || _items.Length == 0)
                return 0;

            int max = _items.Max(given);
            return max;
        }
        #endregion

        #region get enumerator
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new Enumerator(_items);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
        }
        #endregion

        #region enumerator of <T> [class]
        public struct Enumerator : IEnumerator<T>, IEnumerator
        {
            #region internals
            private readonly T[] _items;
            private int _index;
            private T _current;
            #endregion

            #region interface
            public T Current => _current!;

            object IEnumerator.Current
            {
                get
                {
                    if (_index == 0 || _index >= _items.Length)
                        throw new InvalidOperationException("Current object never materialized.");

                    return Current;
                }
            }
            #endregion

            #region constructors
            internal Enumerator(T[] set)
            {
                _items = set;
                _index = 0;
                _current = default;
            }
            #endregion

            #region move next
            public bool MoveNext()
            {
                T[] local = _items;

                if (_index < local.Length)
                {
                    _current = local[_index++];
                    return true;
                }
                return false;
            }
            #endregion

            #region ienumerator reset
            void IEnumerator.Reset()
            {
                _index = 0;
                _current = default;
            }
            #endregion

            #region dispose
            public void Dispose()
            { }
            #endregion
        }
        #endregion
    }
}
