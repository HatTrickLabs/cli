using System;
using System.Collections.Generic;

namespace Crypto.CommandLine
{
    internal class AcceptedSetConstraint<T> : ArgumentConstraint<T>
    {
        #region internals
        private T[] _accepted;
        #endregion

        #region constructors
        internal AcceptedSetConstraint(T[] values)
        {
            _accepted = values ?? throw new ArgumentNullException(nameof(values));

            base.Constraint = this.IsInAcceptedSet;
            var accepted = string.Join("|", values);
            base.ErrorTemplate = $"Argument provided for '{{option-flag}}' is not within accepted set: {accepted}"; ;
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
