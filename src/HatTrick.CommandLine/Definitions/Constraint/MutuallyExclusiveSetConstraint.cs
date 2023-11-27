using System;
using System.Linq;

namespace HatTrick.CommandLine
{
    public class MutuallyExclusiveSetConstraint : CommandConstraint
    {
        #region const 
        public const string ConstraintName = "mutually exclusive";
        #endregion

        #region internals
        private (string key, string flag)[] _opDefKeys;
        #endregion

        #region interface
        internal (string key, string flag)[] OptionDefinitionKeys => _opDefKeys;
        #endregion

        #region constructors
        internal MutuallyExclusiveSetConstraint((string key, string flag)[] opDefKeys) : base(MutuallyExclusiveSetConstraint.ConstraintName)
        {
            _opDefKeys = opDefKeys ?? throw new ArgumentNullException(nameof(opDefKeys));

            base.SetConstraint(this.ZeroOrOneAssigned);

            base.SetDescription(string.Join("|", Array.ConvertAll(opDefKeys, o => o.flag)));
        }
        #endregion

        #region one and only one assigned
        private bool ZeroOrOneAssigned(IConstrainedCommand command)
        {
            Func<string, Option> getOptionByKey = (key) => command[key];

            bool[] results = base.ResolveAssignmentResults(getOptionByKey, Array.ConvertAll(_opDefKeys, o => o.key));

            return results.Where(r => r == true).Take(2).Count() < 2;
        }
        #endregion
    }
}
