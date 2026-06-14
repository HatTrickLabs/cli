// SPDX-License-Identifier: Apache-2.0
// Copyright (c) HatTrick Labs, LLC

using System;

namespace HatTrick.CommandLine
{
    internal class MutuallyExclusiveSetConstraint : CommandConstraint
    {
        #region const 
        internal const string ConstraintName = "Mutually Exclusive";
        #endregion

        #region internals
        private (string key, string flag)[] _opDefKeys;
        #endregion

        #region interface
        internal (string key, string flag)[] OptionDefinitionKeys => _opDefKeys;
        #endregion

        #region constructors
        internal MutuallyExclusiveSetConstraint(params (string key, string flag)[] opDefKeys)
            : base(MutuallyExclusiveSetConstraint.ConstraintName,
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
            if (!this.ZeroOrOneAssigned(command))
                throw new CommandInputException($"Constraint Failed...{base.Name}:  {base.Description}");
        }
        #endregion

        #region one and only one assigned
        private bool ZeroOrOneAssigned(ICommand command)
        {
            Func<string, IOption> getOptionByKey = (key) => command[key];

            bool[] results = base.ResolveAssignmentResults(getOptionByKey, Array.ConvertAll(_opDefKeys, o => o.key));

            int count = 0;
            Array.ForEach(results, (r) => { if (r == true) count += 1; });
            return count < 2;
        }
        #endregion
    }
}
