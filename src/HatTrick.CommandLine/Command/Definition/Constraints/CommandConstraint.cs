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

        #region resolve assignment result
        protected bool[] ResolveAssignmentResults(Func<string, CommandOption> getOptionByKey, string[] optionDefinitionKeys)
        {
            bool[] results = new bool[optionDefinitionKeys.Length];

            Func<CommandOption, bool> wasAssigned = (op) => !(op is EmptyCommandOption || op is DefaultCommandOption);

            for (int i = 0; i < optionDefinitionKeys.Length; i++)
            {
                string key = optionDefinitionKeys[i];

                //we don't need to tip tow around op keys existing or not... its enforced, they will be there...
                var op = getOptionByKey(key);

                results[i] = wasAssigned(op);
            }

            return results;
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
