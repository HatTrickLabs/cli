using System;

namespace HatTrick.CommandLine
{
    public class CommandConstraint
    {
        #region internals
        private Func<IConstrainedCommand, bool> _constraint;
        private string _error;
        #endregion

        #region interface
        protected Func<IConstrainedCommand, bool> Constraint
        {
            set => _constraint = value;
        }

        internal string Error
        {
            get => _error;
            set => _error = value;
        }
        #endregion

        #region constructors
        internal CommandConstraint(Func<IConstrainedCommand, bool> constraint, string error)
        {
            _constraint = constraint ?? throw new ArgumentNullException(nameof(constraint));
            _error = error ?? throw new ArgumentNullException(nameof(error));

            if (error == string.Empty)
                throw new ArgumentException("Argument must contain a value.", nameof(error));
        }

        protected CommandConstraint()
        {
        }
        #endregion

        #region ensure
        internal void Ensure(Command command)
        {
            bool pass = _constraint(command);
            if (!pass)
                throw new CommandArgumentException(_error);
        }
        #endregion
    }
}
