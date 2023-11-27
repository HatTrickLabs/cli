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
        #endregion

        #region ensure
        internal abstract void Ensure(ref Option option);
        #endregion
    }

    public abstract class ArgumentConstraint<T> : ArgumentConstraint
    {
        #region constructors
        internal ArgumentConstraint(string name, string description) : base(name, description)
        {
        }
        #endregion
    }
}
