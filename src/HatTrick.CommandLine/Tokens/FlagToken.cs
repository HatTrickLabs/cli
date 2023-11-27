using System;

namespace HatTrick.CommandLine
{
    #region flag token [abstract]
    public abstract class FlagToken : Token
    {
        #region constructor
        internal FlagToken(string value) : base(value)
        { }
        #endregion

        #region is valid
        public new static bool IsValid(string value)
        {
            return Token.IsValid(value) && value[0] == '-';
        }
        #endregion
    }
    #endregion

    #region terse flag token
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
    #endregion

    #region compound terse flag token
    public class CompoundTerseFlagToken : FlagToken
    {
        #region constructors
        internal CompoundTerseFlagToken(string value) : base(value)
        { }
        #endregion

        #region is valid
        public new static bool IsValid(string value)
        {
            if (!FlagToken.IsValid(value))
                return false;

            if (value.Length < 3)
                return false;

            if (value[1] == '-')
                return false;

            for (int i = 1; i < value.Length; i++)
            {
                char c = value[i];

                if (!char.IsLetter(c))
                    return false;
            }

            return true;
        }
        #endregion

        #region unroll
        internal TerseFlagToken[] Unroll()
        {
            SetOf<TerseFlagToken> flags = new SetOf<TerseFlagToken>();
            string compound = base.Value;
            for (int i = 1; i < compound.Length; i++)
            {
                flags.Add(new TerseFlagToken("-" + compound[i]));
            }
            return flags.ToArray();
        }
        #endregion
    }
    #endregion

    #region verbose flag token
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
    #endregion
}
