using System;

namespace HatTrick.CommandLine
{
    public abstract class ArgumentConstraint
    {
        #region internals
        private string _name;
        private string _description;
        #endregion

        #region interface
        public string Name => _name;

        public string Description => _description;
        #endregion

        #region constructors
        public ArgumentConstraint(string name, string description)
        {
            if (name is null)
                throw new ArgumentNullException(nameof(name));

            if (description is null)
                throw new ArgumentNullException(nameof(description));

            if (name == string.Empty)
                throw new ArgumentException("Argument must contain a value.", nameof(name));

            if (description == string.Empty)
                throw new ArgumentException("Argument must contain a value.", nameof(description));

            _name = name;
            _description = description;
        }

        public ArgumentConstraint(string name)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
        }
        #endregion

        #region set description
        protected void SetDescription(string description)
        {
            _description = description;
        }
        #endregion

        #region ensure
        internal abstract void Ensure(ref Option option);
        #endregion
    }

    public class ArgumentConstraint<T> : ArgumentConstraint
    {
        #region internals
        private Func<T, bool> _constraint;
        #endregion

        #region interface
        protected Func<T, bool> Constraint => _constraint;
        #endregion

        #region constructors
        internal ArgumentConstraint(Func<T, bool> constraint, string name, string description) : base(name, description)
        {
            _constraint = constraint ?? throw new ArgumentNullException(nameof(constraint));
        }

        protected ArgumentConstraint(string name) : base(name)
        {
        }
        #endregion

        #region set constraint
        protected void SetConstraint(Func<T, bool> constraint)
        {
            _constraint = constraint;
        }
        #endregion

        #region ensure
        internal override void Ensure(ref Option option)
        {
            T val = option.HasValue ? option.GetValue<T>() : default(T);
            bool pass = _constraint(val);

            if (!pass)
                throw new ArgumentException($"Constraint failed...flag: '{option.Flag}'  argument: '{option.Argument}'  {base.Name}: '{base.Description}'");
        }
        #endregion
    }
}
