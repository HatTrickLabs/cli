using System;

namespace HatTrick.CommandLine
{
    public abstract class Token
    {
        #region internals
        private string _value;
        #endregion

        #region interface
        public string Value
        {
            get => _value;
            protected set => _value = value;
        }
        #endregion

        #region constructor
        protected Token(string value)
        {
            _value = value;
        }
        #endregion

        #region is valid
        public static bool IsValid(string value)
        {
            if (value is null)
                return false;

            if (value == string.Empty)
                return false;

            //no need to check entire string, any preceding whitespace is invalid.
            if (char.IsWhiteSpace(value[0]))
                return false;

            return true;
        }
        #endregion

        #region to string
        public override string ToString()
        {
            return this.GetType().Name + " " + _value;
        }
        #endregion
    }
}
