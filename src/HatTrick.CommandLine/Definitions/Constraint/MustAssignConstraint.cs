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
        private (string terse, string verbose) _optionFlags;
        #endregion

        #region constructors
        internal MustAssignConstraint((string terse, string verbose) optionFlags) : base(MustAssignConstraint<T>.ConstraintName)
        {
            _optionFlags = optionFlags;
            base.SetDescription("yes");
        }
        #endregion

        #region ensure
        internal override bool Ensure(ref Option option, out string feedback)
        {
            feedback = null;

            if (option is EmptyOption)
                feedback = $"An expected option [{_optionFlags.verbose}|{_optionFlags.terse}] not found...option has a '{MustAssignConstraint<T>.ConstraintName}' constraint.";

            return feedback is null;
        }
        #endregion
    }
}
