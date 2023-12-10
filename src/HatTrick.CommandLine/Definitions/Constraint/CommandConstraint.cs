using System;

namespace HatTrick.CommandLine
{
    internal abstract class CommandConstraint
    {
        #region internals
        private string _name;
        private string _description;
        #endregion

        #region interface
        internal string Name => _name;

        internal string Description => _description;
        #endregion

        #region constructors
        internal CommandConstraint(string name, string description)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
            _description = description ?? throw new ArgumentNullException(nameof(description));

            if (name == string.Empty)
                throw new ArgumentException("Argument must contain a value.", nameof(name));

            if (description == string.Empty)
                throw new ArgumentException("Argument must contain a value.", nameof(description));
        }
        #endregion

        #region resolve assignment result
        //TODO: This def should not live here ...
        protected bool[] ResolveAssignmentResults(Func<string, IOption> getOptionByKey, string[] opDefKeys)
        {
            bool[] results = new bool[opDefKeys.Length];

            Func<IOption, bool> wasAssigned = (op) => !(op is DefaultOption || op is EmptyOption);

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
        internal abstract void Ensure(Command command);
        #endregion
    }
}

