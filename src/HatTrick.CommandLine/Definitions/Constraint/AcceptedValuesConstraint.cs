using System;
using System.Collections.Generic;

namespace HatTrick.CommandLine
{
    public class AcceptedValuesConstraint<T> : ArgumentConstraint<T>
    {
        #region const
        public const string ConstraintName = "accepts";
        #endregion

        #region internals
        private T[] _accepted;
        #endregion

        #region constructors
        internal AcceptedValuesConstraint(T[] values) : base(AcceptedValuesConstraint<T>.ConstraintName)
        {
            _accepted = values ?? throw new ArgumentNullException(nameof(values));

            base.SetConstraint(this.IsInAcceptedSet);
            base.SetDescription(string.Join("|", values));
        }
        #endregion

        #region is in accepted set
        internal bool IsInAcceptedSet(T val)
        {
            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            T[] set = _accepted;
            return Array.Exists(set, (a) => comparer.Equals(a, val));
        }
        #endregion
    }
}
