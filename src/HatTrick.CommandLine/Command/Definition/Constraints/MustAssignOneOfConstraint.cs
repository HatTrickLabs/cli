using System;
using System.Collections.Generic;
using System.Linq;

namespace HatTrick.CommandLine
{
    public class MustAssignOneOfConstraint : CommandConstraint
    {
        #region internals
        private string[] _opDefKeys;
        #endregion

        #region interface
        internal string[] OptionDefinitionKeys => _opDefKeys;
        #endregion

        #region constructors
        internal MustAssignOneOfConstraint(string[] optionDefinitionKeys) 
        {
            _opDefKeys = optionDefinitionKeys ?? throw new ArgumentNullException(nameof(optionDefinitionKeys));

            base.SetConstraint(this.AtLeastOneAssigned);

            string keys = string.Join("|", optionDefinitionKeys);
            string description = $"'Must Assign One of' constraint...One of: {keys}";
            base.SetDescription(description);
        }
        #endregion

        #region at least one assigned
        private bool AtLeastOneAssigned(IConstrainedCommand command)
        {
            Func<string, CommandOption> getOptionByKey = (key) => command[key];

            bool[] results = base.ResolveAssignmentResults(getOptionByKey, _opDefKeys);

            return Array.Exists(results, (r) => r == true);
        }
        #endregion
    }
}
