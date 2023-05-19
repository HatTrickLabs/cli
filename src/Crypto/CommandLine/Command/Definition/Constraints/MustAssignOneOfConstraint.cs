using System;

namespace Crypto.CommandLine
{
    public class MustAssignOneOfConstraint : Constraint
    {
        #region internals
        private bool _mutuallyExclusive;
        private CommandOptionDefinition[] _opDefs;
        #endregion

        internal CommandOptionDefinition[] OptionDefinitions => _opDefs;

        internal bool IsMutuallyExclusive => _mutuallyExclusive;

        #region constructors
        internal MustAssignOneOfConstraint(CommandOptionDefinition[] optionDefinitions)
        {
            _opDefs = optionDefinitions ?? throw new ArgumentNullException(nameof(optionDefinitions));
        }
        #endregion

        #region mutually exclusive
        public void MutuallyExclusive()
        {
            _mutuallyExclusive = true;
        }
        #endregion

        #region references option definition
        internal override bool ReferencesOption(string optionKey)
        {
            return Array.FindIndex(_opDefs, (od) => od.Key == optionKey) > -1;
        }
        #endregion

        #region meets mandate
        internal override bool TryApply(CommandOption option)
        {
            return true;
        }
        #endregion
    }
}
