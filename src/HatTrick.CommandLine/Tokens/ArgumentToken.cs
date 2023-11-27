using System;

namespace HatTrick.CommandLine
{
    #region argument token
    public class ArgumentToken : Token
    {
        #region constructor
        internal ArgumentToken(string value) : base(value)
        { }
        #endregion

        #region is valid
        public new static bool IsValid(string value)
        {
            return Token.IsValid(value);
        }
        #endregion
    }
    #endregion
}
