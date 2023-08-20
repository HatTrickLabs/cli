using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.IO;

namespace HatTrick.CommandLine
{
    public class SetOf<T> : IEnumerable<T>
    {
        #region internals
        private T[] _items;
        private int _length;
        private int _capacity;
        private static readonly T[] _empty;
        private static readonly int _maxCapacity;
        private static readonly int _initialCapacity;
        #endregion

        #region interface
        public static int MaxCapacity => _maxCapacity;

        public int Length => _length;

        public T this[int i]
        {
            get
            {
                if (i >= _length)
                    throw new ArgumentOutOfRangeException("Provided index is outside the upper bounds of the set.");

                return _items[i];
            }
            set
            {
                if (i >= _length)
                    throw new ArgumentOutOfRangeException("Provided index is outside the upper bounds of the set.");

                _items[i] = value;
            }
        }
        #endregion

        #region constructors
        static SetOf()
        {
            _empty = Array.Empty<T>();
            _initialCapacity = 4;//0x4
            _maxCapacity = 1_048_576;//0x100000;
        }

        public SetOf()
        {
            _capacity = 0;
        }
        #endregion

        #region get pointer to
        internal ref T GetPointerTo(int index)
        {
            if (_items is null)
                throw new ArgumentOutOfRangeException("Set is currently empty.");

            if (index >= _length)
                throw new ArgumentOutOfRangeException(nameof(index), "Provided index is outside the upper bounds of the set.");

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
                _capacity = _initialCapacity;
                _items = new T[_capacity];
            }
            else if (_length == _capacity)
            {
                if (_capacity == _maxCapacity)
                    throw new InternalBufferOverflowException($"{nameof(SetOf<T>)} has a maximum internal buffer capacity of {_maxCapacity}.");

                var newItems = new T[_capacity = (_capacity * 2)];
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

            return this.FindIndex(where) > -1;
        }
        #endregion

        #region find index
        public int FindIndex(Predicate<T> where)
        {
            if (_items is null)
                return -1;

            return Array.FindIndex(_items, 0, _length, where);
        }
        #endregion

        #region find
        public T Find(Predicate<T> where)
        {
            if (_items is null)
                return default;

            int idx = this.FindIndex(where);
            return idx > -1 ? _items[idx] : default;
        }
        #endregion

        #region find all
        public T[] FindAll(Predicate<T> where)
        {
            if (where is null)
                throw new ArgumentNullException(nameof(where));

            if (_items is null)
                return _empty;

            int length = _length;

            if (length == 1)
            {
                if (where(_items[0]))
                    return new T[] { _items[0] };
                else
                    return _empty;
            }

            Span<bool> matches = (length > 1024) ? new bool[length] : stackalloc bool[length];
            int count = 0;
            for (int i = 0; i < length; i++)
            {
                if (matches[i] = where(_items[i]))
                    count += 1;
            }

            if (count == 0)
                return _empty;

            var matchSet = new T[count];
            int at = 0;
            for (int i = 0; i < length; i++)
            {
                if (matches[i])
                    matchSet[at++] = _items[i];
            }

            return matchSet;
        }
        #endregion

        #region max
        public Y Max<Y>(Func<T, Y> given) where Y : IComparable
        {
            if (_items is null)
                return default;

            Y max = given(_items[0]);

            if (_length == 1)
                return max;

            for (int i = 1; i < _length; i++)
            {
                Y val = given(_items[i]);
                if (val.CompareTo(max) > 0)
                    max = val;
            }

            return max;
        }
        #endregion

        #region min
        public Y Min<Y>(Func<T, Y> given) where Y : IComparable
        {
            if (_items is null)
                return default;

            Y min = given(_items[0]);

            if (_length == 1)
                return min;

            for (int i = 1; i < _length; i++)
            {
                Y val = given(_items[i]);
                if (val.CompareTo(min) < 0)
                    min = val;
            }

            return min;
        }
        #endregion

        #region get enumerator
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new EnumeratorOf(_items, _length);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(_items, _length);
        }
        #endregion

        #region explict op -> T[]
        public static explicit operator T[](SetOf<T> set)
        {
            var source = set._items;

            if (source is null)
                return _empty;

            int length = set._length;

            var destination = new T[length];
            Array.Copy(source, destination, length);
            return destination;
        }
        #endregion

        #region enumerator [class]
        public class Enumerator : IEnumerator
        {
            #region internals
            private readonly Array _items;
            private int _length;
            private int _index;
            private object _current;
            #endregion

            #region interface
            public object Current => _current!;

            object IEnumerator.Current
            {
                get
                {
                    if (_index == 0 || _index > _length)
                        throw new InvalidOperationException("Current object never materialized.");

                    return Current;
                }
            }
            #endregion

            #region constructors
            internal Enumerator(Array set, int length)
            {
                _items = set;
                _length = length;
                _index = 0;
                _current = default;
            }
            #endregion

            #region move next
            public bool MoveNext()
            {
                if (_index < _length)
                {
                    _current = _items.GetValue(_index++);
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

        #region enumerator of <T> [class]
        public struct EnumeratorOf : IEnumerator<T>, IEnumerator
        {
            #region internals
            private readonly T[] _items;
            private int _length;
            private int _index;
            private T _current;
            #endregion

            #region interface
            public T Current => _current!;

            object IEnumerator.Current
            {
                get
                {
                    if (_index == 0 || _index > _length)
                        throw new InvalidOperationException("Current object never materialized.");

                    return Current;
                }
            }
            #endregion

            #region constructors
            internal EnumeratorOf(T[] set, int length)
            {
                _items = set;
                _length = length;
                _index = 0;
                _current = default;
            }
            #endregion

            #region move next
            public bool MoveNext()
            {
                if (_index < _length)
                {
                    _current = _items[_index++];
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
