namespace HatTrick.CommandLine
{
    public interface IMustAssignConstraint
    {
    }

    public class MustAssignConstraint<T> : ArgumentConstraint<T>, IMustAssignConstraint
    {
        #region const
        public const string ConstraintName = "must assign";
        #endregion

        #region internals
        private OptionFlags _flags;
        #endregion

        #region constructors
        internal MustAssignConstraint(OptionFlags flags) : base(ConstraintName, "yes")
        {
            _flags = flags;
        }
        #endregion

        #region ensure
        internal override void Ensure(ref Option option)
        {
            if (option is EmptyOption)
                throw new OptionArgumentException($"An expected option [{_flags.Verbose}|{_flags.Terse}] not found...option has a '{MustAssignConstraint<T>.ConstraintName}' constraint.");
        }
        #endregion
    }
}
