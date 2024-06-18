using System;

namespace HatTrick.CommandLine
{
    public class ExplicitAssignToken : Token
    {
        #region constructor
        internal ExplicitAssignToken(string value) : base(value)
        { }
        #endregion

        #region is valid
        public new static bool IsValid(string value)
        {
            if (!Token.IsValid(value))
                return false;

            if (value.Length == 0 || value.Length > 1)
                return false;

            char c = value[0];

            if (!(c == '=' || c == ':'))
                return false;

            return true;
        }
        #endregion
    }
}
