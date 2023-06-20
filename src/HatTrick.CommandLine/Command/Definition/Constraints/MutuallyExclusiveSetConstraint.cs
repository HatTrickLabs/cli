using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HatTrick.CommandLine
{
    public class MutuallyExclusiveSetConstraint : CommandConstraint
    {
        #region internals
        private string[] _opDefKeys;
        #endregion

        #region interface
        internal string[] OptionDefinitionKeys => _opDefKeys;
        #endregion

        #region constructors
        internal MutuallyExclusiveSetConstraint(string[] optionDefinitionKeys)
        {
            _opDefKeys = optionDefinitionKeys ?? throw new ArgumentNullException(nameof(optionDefinitionKeys));

            base.Constraint = this.ZeroOrOneAssigned;

            string keys = string.Join("|", optionDefinitionKeys);
            string error = $"Failed 'Mutually Exclusive Set' constraint...Set: {keys}";
            base.Error = error;
        }
        #endregion

        #region one and only one assigned
        private bool ZeroOrOneAssigned(IConstrainedCommand command)
        {
            Func<string, CommandOption> getOptionByKey = (key) => command[key];

            bool[] results = base.ResolveAssignmentResults(getOptionByKey, _opDefKeys);

            return results.Where(r => r == true).Take(2).Count() < 2;
        }
        #endregion
    }
}
