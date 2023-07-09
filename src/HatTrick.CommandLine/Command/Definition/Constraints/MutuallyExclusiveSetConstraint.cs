using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HatTrick.CommandLine
{
    public class MutuallyExclusiveSetConstraint : CommandConstraint
    {
        #region const 
        public const string ConstraintName = "Mutually Exclusive Set";
        #endregion

        #region internals
        private (string key, string flag)[] _opDefKeys;
        #endregion

        #region interface
        internal (string key, string flag)[] OptionDefinitionKeys => _opDefKeys;
        #endregion

        #region constructors
        internal MutuallyExclusiveSetConstraint((string key, string flag)[] optionDefinitionKeys) : base(MutuallyExclusiveSetConstraint.ConstraintName)
        {
            _opDefKeys = optionDefinitionKeys ?? throw new ArgumentNullException(nameof(optionDefinitionKeys));

            base.SetConstraint(this.ZeroOrOneAssigned);

            base.SetDescription(string.Join("|", Array.ConvertAll(optionDefinitionKeys, o => o.flag)));
        }
        #endregion

        #region one and only one assigned
        private bool ZeroOrOneAssigned(IConstrainedCommand command)
        {
            Func<string, CommandOption> getOptionByKey = (key) => command[key];

            bool[] results = base.ResolveAssignmentResults(getOptionByKey, Array.ConvertAll(_opDefKeys, o => o.key));

            return results.Where(r => r == true).Take(2).Count() < 2;
        }
        #endregion
    }
}
