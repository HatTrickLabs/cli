using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace HatTrick.CommandLine
{
    public interface IDefaultConstraint
    {
    }

    public class DefaultConstraint<T> : ArgumentConstraint<T>, IDefaultConstraint
    {
        #region const
        public const string ConstraintName = "Default value";
        #endregion

        #region internals
        private T _default;
        #endregion

        #region interface
        public T DefaultValue => _default;
        #endregion

        #region constructors
        public DefaultConstraint(T value) : base(DefaultConstraint<T>.ConstraintName)
        {
            _default = value;
            base.SetDescription(value.ToString());
            base.SetConstraint((o) => true);//just pass it...nothing to guard against here...
        }
        #endregion
    }
}
