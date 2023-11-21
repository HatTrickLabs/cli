using System;

namespace HatTrick.CommandLine
{
    #region token [abstract]
    public abstract class Token
    {
        #region internals
        private string _value;
        #endregion

        #region interface
        public string Value => _value;
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
    #endregion

    #region command token
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
    #endregion

    #region explicit assign token
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

            if (value.Length > 1)
                return false;

            char c = value[0];

            if (!(c == '=' || c == ':'))
                return false;

            return true;
        }
        #endregion
    }
    #endregion

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

            if (!char.IsLetter(value[1]))
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

                if (!(c == '-' || char.IsLetter(c)))
                    return false;
            }

            return true;
        }
        #endregion
    }
    #endregion

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
