using System;

namespace HatTrick.CommandLine
{
    internal class ArgumentConstraint<T>
    {
        #region internals
        private Func<T, bool> _constraint;
        private string _name;
        private string _description;
        #endregion

        #region interface
        protected Func<T, bool> Constraint => _constraint;

        public string Name => _name;

        public string Description => _description;
        #endregion

        #region constructors
        internal ArgumentConstraint(Func<T, bool> constraint, string name, string description)
        {
            _constraint = constraint ?? throw new ArgumentNullException(nameof(constraint));
            _name = name ?? throw new ArgumentNullException(nameof(name));
            _description = description ?? throw new ArgumentNullException(nameof(description));

            if (name == string.Empty)
                throw new ArgumentException("Argument must contain a value.", nameof(name));

            if (description == string.Empty)
                throw new ArgumentException("Argument must contain a value.", nameof(description));
        }

        protected ArgumentConstraint(string name)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
        }
        #endregion

        #region set constraint
        protected void SetConstraint(Func<T, bool> constraint)
        {
            _constraint = constraint;
        }
        #endregion

        #region set description
        protected void SetDescription(string description)
        {
            _description = description;
        }
        #endregion

        #region ensure
        internal void Ensure(CommandOption option)
        {
            T val = option.GetValue<T>();
            bool pass = _constraint(val);

            if (!pass)
                throw new CommandArgumentException($"Constraint Failed...Flag: {option.Flag}...Arg: {option.Argument}...{_name}: {_description}");
        }
        #endregion
    }
}
