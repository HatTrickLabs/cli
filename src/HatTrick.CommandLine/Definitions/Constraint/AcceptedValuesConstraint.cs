using System;
using System.Collections.Generic;

namespace HatTrick.CommandLine
{
    internal class AcceptedValuesConstraint<T> : ArgumentConstraint<T>
    {
        #region const
        internal const string ConstraintName = "accepts";
        #endregion

        #region internals
        private T[] _accepted;
        private EqualityComparer<T> _comparer;
        #endregion

        #region constructors
        internal AcceptedValuesConstraint(EqualityComparer<T> comparer, T[] values)
            : base(
                  ConstraintName, 
                  values is null 
                    ? throw new ArgumentNullException(nameof(values))
                    : string.Join("|", values)
        )
        {
            _comparer = comparer;
            _accepted = values ?? throw new ArgumentNullException(nameof(values));
        }
        #endregion

        #region ensure
        internal override void Ensure(ref Option option)
        {
            T val = option.HasValue ? option.GetValue<T>() : default(T);
            if (!this.IsInAcceptedSet(val))
                throw new OptionArgumentException($"Constraint failed...flag: '{option.Flag}'  argument: '{option.Argument}'  {base.Name}: '{base.Description}'");
        }
        #endregion

        #region is in accepted set
        internal bool IsInAcceptedSet(T val)
        {
            T[] set = _accepted;
            return Array.Exists(set, (a) => _comparer.Equals(a, val));
        }
        #endregion
    }
}
