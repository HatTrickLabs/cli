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
            _name = name ?? throw new ArgumentNullException(nameof(name));
            _description = description ?? throw new ArgumentNullException(nameof(description));

            if (name == string.Empty)
                throw new ArgumentException("Argument must contain a value.", nameof(name));

            if (description == string.Empty)
                throw new ArgumentException("Argument must contain a value.", nameof(description));
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
        internal abstract bool Ensure(ref CommandOption option, out string feedback);
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
        internal override bool Ensure(ref CommandOption option, out string feedback)
        {
            feedback = null;
            T val = option.GetValue<T>();
            bool pass = _constraint(val);

            if (!pass)
                feedback = $"Constraint Failed...Flag: '{option.Flag}'  Arg: '{option.Argument}'  {base.Name}: '{base.Description}'";

            return pass;
        }
        #endregion
    }
}
