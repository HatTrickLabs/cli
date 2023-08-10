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
        private string[] _optionFlags;
        #endregion

        #region constructors
        internal MustAssignConstraint(string[] optionFlags) : base(MustAssignConstraint<T>.ConstraintName)
        {
            _optionFlags = optionFlags;
            base.SetDescription("yes");
        }
        #endregion

        #region ensure
        internal override bool Ensure(ref CommandOption option, out string feedback)
        {
            feedback = null;

            if (option is EmptyCommandOption)
                feedback = $"An expected option [{string.Join("|", _optionFlags)}] not found...option has a '{MustAssignConstraint<T>.ConstraintName}' constraint.";

            return feedback is null;
        }
        #endregion
    }
}
