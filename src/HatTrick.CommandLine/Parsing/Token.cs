using System;

namespace HatTrick.CommandLine
{
    #region token [abstract]
    public abstract class Token
    {
        #region internals
        private string _value;
        #endregion

        #region constructor
        public Token(string value)
        {
            _value = value;
        }
        #endregion
    }
    #endregion

    #region command token
    public class CommandToken : Token
    {
        #region constructor
        public CommandToken(string value) : base(value)
        { }
        #endregion
    }
    #endregion

    #region explicit assign token
    public class ExplicitAssignToken : Token
    {
        #region constructor
        public ExplicitAssignToken(string value) : base(value)
        { }
        #endregion
    }
    #endregion

    #region flag token [abstract]
    public abstract class FlagToken : Token
    {
        #region constructor
        public FlagToken(string value) : base(value)
        { }
        #endregion
    }
    #endregion

    #region terse flag token
    public class TerseFlagToken : FlagToken
    {
        #region constructor
        public TerseFlagToken(string value) : base(value)
        { }
        #endregion
    }
    #endregion

    #region verbose flag token
    public class VerboseFlagToken : FlagToken
    {
        #region constructor
        public VerboseFlagToken(string value) : base(value)
        { }
        #endregion
    }
    #endregion

    #region argument token
    public class ArgumentToken : Token
    {
        #region constructor
        public ArgumentToken(string value) : base(value)
        { }
        #endregion
    }
    #endregion
}
