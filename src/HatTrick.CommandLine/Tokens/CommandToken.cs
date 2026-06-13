using System;

namespace HatTrick.CommandLine
{
    public class CommandToken : Token
    {
        #region constructor
        internal CommandToken(string value) : base(value)
        { }
        #endregion

        #region is valid command char
        public static bool IsValidCommandChar(char c)
        {
            return (char.IsLetter(c) || char.IsDigit(c) || c == '.' || c == '-' || c == '+');
        }
        #endregion

        #region is valid
        public new static bool IsValid(string value)
        {
            if (!Token.IsValid(value))
                return false;

            if (value.Length == 0)
                return false;

            char c = value[0];
            if (c == '.' || c == '-')
                return false;

            for (int i = 0; i < value.Length; i++)
            {
                c = value[i];
                if (!CommandToken.IsValidCommandChar(c))
                    return false;

                if (c == '.' && i > 0 && value[i - 1] == '.')
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
