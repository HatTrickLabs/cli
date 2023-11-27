using System;

namespace HatTrick.CommandLine
{
    public class CommandToken : Token
    {
        #region constructor
        internal CommandToken(string value) : base(value)
        { }
        #endregion

        #region is valid
        public new static bool IsValid(string value)
        {
            if (!Token.IsValid(value))
                return false;

            char c = value[0];
            if (c == '.' || c == '-')
                return false;

            for (int i = 1; i < value.Length - 1; i++)
            {
                c = value[i];
                if (!(char.IsLetter(c) || char.IsDigit(c) || c == '.' || c == '-'))
                    return false;
            }

            c = value[^1];
            if (c == '.' || c == '-')
                return false;

            return true;
        }
        #endregion
    }
}
