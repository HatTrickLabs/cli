using System;

namespace HatTrick.CommandLine
{
    internal class ArgumentConstraint<T>
    {
        #region internals
        private Func<T, bool> _constraint;
        private string _description;
        #endregion

        #region interface
        protected Func<T, bool> Constraint => _constraint;

        protected string Description => _description;
        #endregion

        #region constructors
        internal ArgumentConstraint(Func<T, bool> constraint, string description)
        {
            _constraint = constraint ?? throw new ArgumentNullException(nameof(constraint));
            _description = description ?? throw new ArgumentNullException(nameof(description));

            if (description == string.Empty)
                throw new ArgumentException("Argument must contain a value.", nameof(description));
        }

        protected ArgumentConstraint()
        {
        }
        #endregion

        #region set constraint
        protected void SetConstraint(Func<T, bool> constraint)
        {
            _constraint = constraint;
        }
        #endregion

        #region set description
        protected void SetDescription(string description)
        {
            _description = description;
        }
        #endregion

        #region ensure
        internal void Ensure(CommandOption option)
        {
            T val = option.GetValue<T>();
            if (_constraint(val))
                return;

            string error = "Failed " + _description;

            throw new CommandArgumentException(error);
        }
        #endregion
    }
}
