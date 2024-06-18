using System;

namespace HatTrick.CommandLine
{
    public abstract class FlagToken : Token
    {
        #region constructor
        internal FlagToken(string value) : base(value)
        { }
        #endregion

        #region is valid
        public new static bool IsValid(string value)
        {
            return Token.IsValid(value) && value.Length > 0 && value[0] == '-';
        }
        #endregion
    }
}
