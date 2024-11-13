using System;

namespace HatTrick.CommandLine
{
    public class Option : IOption, IPreEnsureOption
    {
        #region internals
        private string _key;
        private string _flag;
        private string _arg;
        private object _value;
        #endregion

        #region interface
        public string Key => _key;

        public string Flag => _flag;

        public string Argument => _arg;

        public bool HasArgument => _arg is not null;

        public bool HasValue => _value is not null;

        public dynamic Value => _value;
        #endregion

        #region constructors
        internal Option(string flag)
        {
            if (flag is null)
                throw new ArgumentNullException(nameof(flag));

            if (flag == string.Empty)
                throw new ArgumentException("Argument cannot be empty.", nameof(flag));

            _flag = flag;
        }

        internal Option(string key, string flag)
        {
            if (key is null)
                throw new ArgumentNullException(nameof(key));

            if (key == string.Empty)
                throw new ArgumentException("Argument cannot be empty.", nameof(key));

            if (flag is null)
                throw new ArgumentNullException(nameof(flag));

            if (flag == string.Empty)
                throw new ArgumentException("Argument cannot be empty.", nameof(flag));

            _key = key;
            _flag = flag;
        }

        internal Option(string key, string flag, string argument)
        {
            if (key is null)
                throw new ArgumentNullException(nameof(key));

            if (key == string.Empty)
                throw new ArgumentException("Argument cannot be empty.", nameof(key));

            if (flag is null)
                throw new ArgumentNullException(nameof(flag));

            if (flag == string.Empty)
                throw new ArgumentException("Argument cannot be empty.", nameof(flag));

            _key = key;
            _flag = flag;
            _arg = argument;
        }
        #endregion

        #region set key
        internal void SetKey(string key)
        {
            if (key is null)
                throw new ArgumentNullException(nameof(key));

            if (key == string.Empty)
                throw new ArgumentException("Argument cannot be empty.", nameof(key));

            _key = key;
        }
        #endregion

        #region set arg
        internal void SetArgument(string argument)
        {
            if (argument is null)
                throw new ArgumentNullException(nameof(argument));

            _arg = argument;
        }
        #endregion

        #region set value
        internal void SetValue(object value)
        {
            _value = value;
        }
        #endregion

        #region get value
        internal object GetValue()
        {
            return _value;
        }

        public T GetValue<T>()
        {
            return (T)_value;
        }
        #endregion

        #region override argument
        void IPreEnsureOption.OverrideArgument(string argument)
        {
            _arg = argument;
        }
        #endregion
    }
}
