using System;

namespace HatTrick.CommandLine
{
    public class CommandConstraint
    {
        #region internals
        private Func<IConstrainedCommand, bool> _constraint;
        private string _name;
        private string _description;
        #endregion

        #region interface
        protected Func<IConstrainedCommand, bool> Constraint => _constraint;

        public string Name => _name;

        public string Description => _description;
        #endregion

        #region constructors
        internal CommandConstraint(Func<IConstrainedCommand, bool> constraint, string name, string description)
        {
            _constraint = constraint ?? throw new ArgumentNullException(nameof(constraint));
            _name = name ?? throw new ArgumentNullException(nameof(name));
            _description = description ?? throw new ArgumentNullException(nameof(description));

            if (name == string.Empty)
                throw new ArgumentException("Argument must contain a value.", nameof(name));

            if (description == string.Empty)
                throw new ArgumentException("Argument must contain a value.", nameof(description));
        }

        protected CommandConstraint(string name)
        {
            _name = name ?? throw new ArgumentException(nameof(name));
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
        protected bool[] ResolveAssignmentResults(Func<string, Option> getOptionByKey, string[] opDefKeys)
        {
            bool[] results = new bool[opDefKeys.Length];

            Func<Option, bool> wasAssigned = (op) => !(op is DefaultOption || op is EmptyOption);

            for (int i = 0; i < opDefKeys.Length; i++)
            {
                string key = opDefKeys[i];

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
            if (!_constraint(command))
                throw new CommandInputException($"Constraint Failed...{_name}:  {_description}");
        }
        #endregion
    }
}

