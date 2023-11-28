using System;

namespace HatTrick.CommandLine
{
    public class VerboseFlagToken : FlagToken
    {
        #region constructor
        internal VerboseFlagToken(string value) : base(value)
        { }
        #endregion

        #region is valid
        public new static bool IsValid(string value)
        {
            if (!FlagToken.IsValid(value))
                return false;

            if (value.Length < 4)
                return false;

            if (!(value[0] == '-' && value[1] == '-'))
                return false;

            for (int i = 2; i < value.Length; i++)
            {
                char c = value[i];
                if (i == 2 && c == '-')//no third dash '---'
                    return false;

                if (!(c == '-' || char.IsLetterOrDigit(c)))
                    return false;
            }

            return true;
        }
        #endregion
    }
}
