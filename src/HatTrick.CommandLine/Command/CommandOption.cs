using System;

namespace HatTrick.CommandLine
{
    public class CommandOption
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
        public dynamic Value => _value;
        #endregion

        #region constructors
        protected CommandOption()
        { }

        internal CommandOption(string flag)
        {
            _flag = flag ?? throw new ArgumentNullException(nameof(flag));
        }

        internal CommandOption(string key, string flag)
        {
            _key = key ?? throw new ArgumentNullException(nameof(key));
            _flag = flag ?? throw new ArgumentNullException(nameof(flag));
        }
        #endregion

        #region apply key
        internal void ApplyKey(string key)
        {
            _key = key;
        }
        #endregion

        #region apply arg
        internal void ApplyArgument(string argument)
        {
            _arg = argument;
        }
        #endregion

        #region set value
        internal void SetValue<T>(T value)
        {
            _value = value;
        }
        #endregion

        #region get value
        internal T GetValue<T>()
        {
            return (T)_value;
        }
        #endregion
    }
}
