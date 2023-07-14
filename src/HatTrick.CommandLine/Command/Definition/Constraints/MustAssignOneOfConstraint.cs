using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace HatTrick.CommandLine
{
    public class MustAssignOneOfConstraint : CommandConstraint
    {
        #region const
        public const string ConstraintName = "Must assign one of";
        #endregion

        #region internals
        private (string key, string flag)[] _opDefKeys;
        #endregion

        #region interface
        internal (string key, string flag)[] OptionDefinitionKeys => _opDefKeys;
        #endregion

        #region constructors
        internal MustAssignOneOfConstraint((string key, string flag)[] optionDefinitionKeys) : base(MustAssignOneOfConstraint.ConstraintName)
        {
            _opDefKeys = optionDefinitionKeys ?? throw new ArgumentNullException(nameof(optionDefinitionKeys));

            base.SetConstraint(this.AtLeastOneAssigned);

            base.SetDescription(string.Join("|", Array.ConvertAll(optionDefinitionKeys, o => o.flag)));
        }
        #endregion

        #region at least one assigned
        private bool AtLeastOneAssigned(IConstrainedCommand command)
        {
            Func<string, CommandOption> getOptionByKey = (key) => command[key];

            bool[] results = base.ResolveAssignmentResults(getOptionByKey, Array.ConvertAll(_opDefKeys, o => o.key));

            return Array.Exists(results, (r) => r == true);
        }
        #endregion
    }
}
