using System;

namespace HatTrick.CommandLine
{
    public class CommandConstraint
    {
        #region internals
        private Func<IConstrainedCommand, bool> _constraint;
        private string _description;
        #endregion

        #region interface
        protected Func<IConstrainedCommand, bool> Constraint => _constraint;

        internal string Description => _description;
        #endregion

        #region constructors
        internal CommandConstraint(Func<IConstrainedCommand, bool> constraint, string description)
        {
            _constraint = constraint ?? throw new ArgumentNullException(nameof(constraint));
            _description = description ?? throw new ArgumentNullException(nameof(description));

            if (description == string.Empty)
                throw new ArgumentException("Argument must contain a value.", nameof(description));
        }

        protected CommandConstraint()
        {
        }
        #endregion

        #region set constraint
        protected void SetConstraint(Func<IConstrainedCommand, bool> constraint)
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

            string error = "Failed " + _description;

            if (!pass)
                throw new CommandArgumentException(error);
        }
        #endregion
    }
}
