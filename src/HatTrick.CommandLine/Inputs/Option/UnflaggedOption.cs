using System;

namespace HatTrick.CommandLine
{
    internal class UnflaggedOption : Option
    {
        #region internals
        private const string Unflagged = "UNFLAGGED";
        #endregion

        #region constructors
        internal UnflaggedOption(string argument) : base(Unflagged)
        {
            base.SetArgument(argument);
        }
        #endregion
    }
}
