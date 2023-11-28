using System;

namespace HatTrick.CommandLine
{
    public class TerseFlagToken : FlagToken
    {
        #region constructor
        internal TerseFlagToken(string value) : base(value)
        { }
        #endregion

        #region is valid
        public new static bool IsValid(string value)
        {
            if (!FlagToken.IsValid(value))
                return false;

            if (value.Length != 2)
                return false;

            if (value[1] == '-')
                return false;

            if (!char.IsLetterOrDigit(value[1]))
                return false;

            return true;
        }
        #endregion
    }
}
