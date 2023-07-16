using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace HatTrick.CommandLine
{
    public class MustAssignConstraint : ArgumentConstraint
    {
        #region const
        public const string ConstraintName = "Must assign";
        #endregion

        #region internals
        private string[] _optionFlags;
        #endregion

        #region constructors
        internal MustAssignConstraint(string[] optionFlags) : base(MustAssignConstraint.ConstraintName)
        {
            _optionFlags = optionFlags;
            base.SetDescription("Option arg must be provided.");
        }
        #endregion

        #region ensure
        internal override bool Ensure(CommandOption option, out string feedback)
        {
            feedback = null;

            if (option is EmptyCommandOption)
                feedback = $"Expected option [{string.Join("|", _optionFlags)}] not found...option has a '{MustAssignConstraint.ConstraintName}' constraint.";

            else if (string.IsNullOrEmpty(option.Argument))
                feedback = $"No arg provided for '{option.Flag}'...option has a '{MustAssignConstraint.ConstraintName} constraint...{base.Description}";

            return feedback is null;
        }
        #endregion
    }
}
