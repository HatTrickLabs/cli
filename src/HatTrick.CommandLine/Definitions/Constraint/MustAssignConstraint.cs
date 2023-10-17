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
        internal MustAssignConstraint(OptionFlags flags) : base(MustAssignConstraint<T>.ConstraintName)
        {
            _flags = flags;
            base.SetDescription("yes");
        }
        #endregion

        #region ensure
        internal override bool Ensure(ref Option option, out string feedback)
        {
            feedback = null;

            if (option is EmptyOption)
                feedback = $"An expected option [{_flags.Verbose}|{_flags.Terse}] not found...option has a '{MustAssignConstraint<T>.ConstraintName}' constraint.";

            return feedback is null;
        }
        #endregion
    }
}
