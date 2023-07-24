namespace HatTrick.CommandLine
{
    public interface IDefaultConstraint
    {
    }

    public class DefaultConstraint<T> : ArgumentConstraint<T>, IDefaultConstraint
    {
        #region const
        public const string ConstraintName = "defaults to";
        #endregion

        #region internals
        private string _optionKey;
        private string _mostVerboseFlag;
        private T _default;
        #endregion

        #region interface
        public T DefaultValue => _default;
        #endregion

        #region constructors
        public DefaultConstraint(string optionKey, string mostVerboseFlag, T defaultValue) : base(DefaultConstraint<T>.ConstraintName)
        {
            _optionKey = optionKey;
            _mostVerboseFlag = mostVerboseFlag;
            _default = defaultValue;
            base.SetDescription(defaultValue is null ? "null" : defaultValue.ToString());
        }
        #endregion

        #region ensure
        internal override bool Ensure(ref CommandOption option, out string feedback)
        {
            feedback = null;
            if (option is EmptyCommandOption)
            {
                //changing type...this is WHY ref param is necessary.
                option = new DefaultCommandOption(_optionKey, _mostVerboseFlag);
                option.SetValue(_default);
            }
            return true;
        }
        #endregion
    }
}
