using System;

namespace HatTrick.CommandLine
{
    public class CustomCommandConstraint : CommandConstraint
    {
        #region internals
        private Func<ICommand, bool> _constraint;
        #endregion

        #region constructors
        public CustomCommandConstraint(Func<ICommand, bool> constraint, string name, string description) 
            : base(name, description)
        {
            _constraint = constraint ?? throw new ArgumentNullException(nameof(constraint));
        }
        #endregion

        #region ensure
        public override void Ensure(Command command)
        {
            if (!_constraint(command))
                throw new CommandInputException($"Constraint Failed...{base.Name}:  {base.Description}");
        }
        #endregion
    }
}
