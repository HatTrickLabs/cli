using System;

namespace HatTrick.CommandLine
{
    public class MustAssignOneOfConstraint : CommandConstraint
    {
        #region const
        public const string ConstraintName = "must assign one of";
        #endregion

        #region internals
        private (string key, string flag)[] _opDefKeys;
        #endregion

        #region interface
        internal (string key, string flag)[] OptionDefinitionKeys => _opDefKeys;
        #endregion

        #region constructors
        internal MustAssignOneOfConstraint((string key, string flag)[] opDefKeys) : base(MustAssignOneOfConstraint.ConstraintName)
        {
            _opDefKeys = opDefKeys ?? throw new ArgumentNullException(nameof(opDefKeys));

            base.SetConstraint(this.AtLeastOneAssigned);

            base.SetDescription(string.Join("|", Array.ConvertAll(opDefKeys, o => o.flag)));
        }
        #endregion

        #region at least one assigned
        private bool AtLeastOneAssigned(IConstrainedCommand command)
        {
            Func<string, Option> getOptionByKey = (key) => command[key];

            bool[] results = base.ResolveAssignmentResults(getOptionByKey, Array.ConvertAll(_opDefKeys, o => o.key));

            return Array.Exists(results, (r) => r == true);
        }
        #endregion
    }
}
