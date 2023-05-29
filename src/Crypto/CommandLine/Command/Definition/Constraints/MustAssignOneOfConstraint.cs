using System;
using System.Collections.Generic;
using System.Linq;

namespace Crypto.CommandLine
{
    public class MustAssignOneOfConstraint : CommandConstraint
    {
        #region internals
        private bool _mutuallyExclusive;
        private string[] _opDefKeys;
        #endregion

        #region interface
        internal string[] OptionDefinitionKeys => _opDefKeys;

        internal bool IsMutuallyExclusive => _mutuallyExclusive;
        #endregion

        #region constructors
        internal MustAssignOneOfConstraint(string[] optionDefinitionKeys) : this(false, optionDefinitionKeys)
        {
            _opDefKeys = optionDefinitionKeys ?? throw new ArgumentNullException(nameof(optionDefinitionKeys));
        }

        internal MustAssignOneOfConstraint(bool mutuallyExclusive, string[] optionDefinitionKeys) 
        {
            _mutuallyExclusive = mutuallyExclusive;
            _opDefKeys = optionDefinitionKeys ?? throw new ArgumentNullException(nameof(optionDefinitionKeys));

            base.Constraint = (mutuallyExclusive) ? this.OneAndOnlyOneAssigned : this.AtLeastOneAssigned;

            string keys = string.Join("|", optionDefinitionKeys);
            string mEx = _mutuallyExclusive ? " [Mutually Exclusive]" : string.Empty;
            string error = $"Failed 'Must Assign One of' constraint...One of: {keys}{mEx}";
            base.Error = error;
        }
        #endregion

        #region resolve assignment result
        private bool[] ResolveAssignmentResults(Func<string, CommandOption> getOptionByKey, string[] optionDefinitionKeys)
        {
            bool[] results = new bool[optionDefinitionKeys.Length];

            Func<CommandOption, bool> wasAssigned = (op) => !(op is EmptyCommandOption || op is DefaultCommandOption);

            for (int i = 0; i < optionDefinitionKeys.Length; i++)
            {
                string key = _opDefKeys[i];

                //we don't need to tip tow around op keys existing or not... its enforced, they will be there...
                var op = getOptionByKey(key);

                results[i] = wasAssigned(op);
            }

            return results;
        }
        #endregion

        #region at least one assigned
        private bool AtLeastOneAssigned(Command command)
        {
            Func<string, CommandOption> getOptionByKey = (key) => command[key];

            bool[] results = this.ResolveAssignmentResults(getOptionByKey, _opDefKeys);

            return Array.Exists(results, (r) => r == true);
        }
        #endregion

        #region one and only one assigned
        private bool OneAndOnlyOneAssigned(Command command)
        {
            Func<string, CommandOption> getOptionByKey = (key) => command[key];

            bool[] results = this.ResolveAssignmentResults(getOptionByKey, _opDefKeys);

            return results.Where(r => r == true).Take(2).Count() == 1;
        }
        #endregion
    }
}
