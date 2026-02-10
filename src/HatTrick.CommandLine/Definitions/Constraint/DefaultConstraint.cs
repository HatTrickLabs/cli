using System;

namespace HatTrick.CommandLine
{
    #region i default constraint [interface]
    public interface IDefaultConstraint
    {
    }
    #endregion

    #region defaulg constraint [class]
    internal class DefaultConstraint<T> : ArgumentConstraint<T>, IDefaultConstraint
    {
        #region const
        internal const string ConstraintName = "Defaults To";
        #endregion

        #region internals
        private string _optionKey;
        private string _verboseFlag;
        private Func<T> _default;
        #endregion

        #region interface
        internal T DefaultValue => _default();
        #endregion

        #region constructors
        internal DefaultConstraint(string optionKey, string verboseFlag, T defaultValue) 
            : base(ConstraintName, defaultValue is null ? "null" : defaultValue.ToString())
        {
            _optionKey = optionKey;
            _verboseFlag = verboseFlag;
            _default = () => defaultValue;
        }

        internal DefaultConstraint(string optionKey, string verboseFlag, Func<T> getValue, string description)
            : base(ConstraintName, description)
        {
            _optionKey = optionKey;
            _verboseFlag = verboseFlag;
            _default = getValue;
        }
        #endregion

        #region ensure
        internal override void Ensure(ref Option option)
        {   //NOTE: This only works because default constraint is the very FIRST constraint added (constructor of opdef)
            if (option is EmptyOption)
            {
                //type swap...this is WHY ref param is necessary.
                option = new DefaultOption(_optionKey, _verboseFlag);
                option.SetValue(this.DefaultValue);
            }
        }
        #endregion
    }
    #endregion
}
