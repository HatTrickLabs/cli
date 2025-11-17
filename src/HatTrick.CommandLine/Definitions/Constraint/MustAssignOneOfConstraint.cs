using System;

namespace HatTrick.CommandLine
{
    internal class MustAssignOneOfConstraint : CommandConstraint
    {
        #region const
        internal const string ConstraintName = "Must assign one of";
        #endregion

        #region internals
        private (string key, string flag)[] _opDefKeys;
        #endregion

        #region interface
        internal (string key, string flag)[] OptionDefinitionKeys => _opDefKeys;
        #endregion

        #region constructors
        internal MustAssignOneOfConstraint(params (string key, string flag)[] opDefKeys) 
            : base(MustAssignOneOfConstraint.ConstraintName, 
                  opDefKeys is null 
                    ? throw new ArgumentNullException(nameof(opDefKeys))
                    : string.Join("|", Array.ConvertAll(opDefKeys, o => o.flag))
            )
        {
            _opDefKeys = opDefKeys;
        }
        #endregion

        #region ensure
        internal override void Ensure(Command command)
        {
            if (!this.AtLeastOneAssigned(command))
                throw new CommandInputException($"Constraint Failed...{base.Name}:  {base.Description}");
        }
        #endregion

        #region at least one assigned
        private bool AtLeastOneAssigned(ICommand command)
        {
            Func<string, IOption> getOptionByKey = (key) => command[key];

            bool[] results = base.ResolveAssignmentResults(getOptionByKey, Array.ConvertAll(_opDefKeys, o => o.key));

            return Array.Exists(results, (r) => r == true);
        }
        #endregion
    }
}
