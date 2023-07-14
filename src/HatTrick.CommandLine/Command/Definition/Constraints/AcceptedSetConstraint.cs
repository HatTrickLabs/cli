using System;
using System.Collections.Generic;

namespace HatTrick.CommandLine
{
    internal class AcceptedSetConstraint<T> : ArgumentConstraint<T>
    {
        #region const
        public const string ConstraintName = "Accepted values";
        #endregion

        #region internals
        private T[] _accepted;
        #endregion

        #region constructors
        internal AcceptedSetConstraint(T[] values) : base(AcceptedSetConstraint<T>.ConstraintName)
        {
            _accepted = values ?? throw new ArgumentNullException(nameof(values));

            base.SetConstraint(this.IsInAcceptedSet);
            base.SetDescription(string.Join("|", values));
        }
        #endregion

        #region is in accepted set
        private bool IsInAcceptedSet(T val)
        {
            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            T[] set = _accepted;
            return Array.Exists(set, (a) => comparer.Equals(a, val));
        }
        #endregion
    }
}
