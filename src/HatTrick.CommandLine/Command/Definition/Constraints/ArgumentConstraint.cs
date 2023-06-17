using System;

namespace HatTrick.CommandLine
{
    internal class ArgumentConstraint<T>
    {
        #region internals
        private Func<T, bool> _constraint;
        private string _errorTemplate;
        #endregion

        #region interface
        protected Func<T, bool> Constraint
        {
            set => _constraint = value;
        }

        protected string ErrorTemplate
        {
            set => _errorTemplate = value;
        }
        #endregion

        #region constructors
        internal ArgumentConstraint(Func<T, bool> constraint, string errorTemplate)
        {
            _constraint = constraint ?? throw new ArgumentNullException(nameof(constraint));
            _errorTemplate = errorTemplate ?? throw new ArgumentNullException(nameof(errorTemplate));

            if (errorTemplate == string.Empty)
                throw new ArgumentException("Argument must contain a value.", nameof(errorTemplate));
        }

        protected ArgumentConstraint()
        {
        }
        #endregion

        #region ensure
        internal void Ensure(CommandOption option)
        {
            T val = option.GetValue<T>();
            if (_constraint(val))
                return;

            //TODO: eliminate replaces with HTL template engine...
            string error = _errorTemplate;
            error = error.Replace("{option-flag}", option.Flag)
                         .Replace("{option-argument}", option.Argument)
                         .Replace("{option-key}", option.Key);

            throw new CommandArgumentException(error);
        }
        #endregion
    }
}
