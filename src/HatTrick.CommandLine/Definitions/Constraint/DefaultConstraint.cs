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
        private string _verboseFlag;
        private T _default;
        #endregion

        #region interface
        public T DefaultValue => _default;
        #endregion

        #region constructors
        public DefaultConstraint(string optionKey, string verboseFlag, T defaultValue) : base(DefaultConstraint<T>.ConstraintName)
        {
            _optionKey = optionKey;
            _verboseFlag = verboseFlag;
            _default = defaultValue;
            base.SetDescription(defaultValue is null ? "null" : defaultValue.ToString());
        }
        #endregion

        #region ensure
        internal override void Ensure(ref Option option)
        {   //NOTE: This only works because default constraint is the very FIRST constraint added (constructor of opdef)
            if (option is EmptyOption)
            {
                //type swap...this is WHY ref param is necessary.
                option = new DefaultOption(_optionKey, _verboseFlag);
                option.SetValue(_default);
            }
        }
        #endregion
    }
}
