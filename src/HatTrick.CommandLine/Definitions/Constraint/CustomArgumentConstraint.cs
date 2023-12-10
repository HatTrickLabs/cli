using System;

namespace HatTrick.CommandLine
{
    internal class CustomArgumentConstraint<T> : ArgumentConstraint<T>
    {
        #region internals
        private Func<T, bool> _constraint;
        #endregion

        #region constructors
        internal CustomArgumentConstraint(Func<T, bool> constraint, string name, string description) : base(name, description)
        {
            _constraint = constraint;
        }
        #endregion

        #region ensure
        internal override void Ensure(ref Option option)
        {
            T val = option.HasValue ? option.GetValue<T>() : default(T);
            if (!_constraint(val))
                throw new OptionArgumentException($"Constraint failed...flag: '{option.Flag}'  argument: '{option.Argument}'  {base.Name}: '{base.Description}'");
        }
        #endregion
    }
}
