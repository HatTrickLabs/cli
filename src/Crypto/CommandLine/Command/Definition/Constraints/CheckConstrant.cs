using System;

namespace Crypto.CommandLine
{
    public class CheckConstrant<T> : Constraint
    {
        #region internals
        private CommandOptionDefinition<T> _opDef;
        private T[] _acceptedValues;
        #endregion

        #region interface
        //internal CommandOptionDefinition OptionDefinitions => _opDef;
        public string OptionKey => _opDef.Key;

        internal T[] AcceptedValues => _acceptedValues;
        #endregion

        #region constructors
        internal CheckConstrant(CommandOptionDefinition<T> optionDefinition, params T[] acceptedValues)
        {
            _opDef = optionDefinition ?? throw new ArgumentNullException(nameof(optionDefinition));
            _acceptedValues = acceptedValues ?? throw new ArgumentNullException(nameof(acceptedValues));
            if (acceptedValues.Length == 0)
                throw new ArgumentException("Argument must contain at least 1 item.", nameof(acceptedValues));
        }
        #endregion

        #region references option definition
        internal override bool ReferencesOption(string optionKey)
        {
            return _opDef.Key == optionKey;
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
