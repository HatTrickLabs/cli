using System;
using System.Reflection.Metadata;

namespace HatTrick.CommandLine.Namespace
{
    public class NamespaceDefinition
    {
        #region internals
        private string _name;
        private string _help;
        private int _depth;
        private bool _hidden;
        #endregion

        #region interface
        public string Name => _name;

        public string Help => _help;

        internal int Depth => _depth;

        public bool Hidden => _hidden;
        #endregion

        #region constructors
        public NamespaceDefinition(string name, string help)
        {
            _name = name == null
                ? throw new ArgumentNullException(nameof(name))
                : name == string.Empty
                    ? throw new ArgumentException("Argument value cannot be empty.", nameof(name))
                    : name;

            _help = help == null
                ? throw new ArgumentNullException(nameof(help))
                : help == string.Empty
                    ? throw new ArgumentException("Argument value cannot be empty.", nameof(help))
                    : help;
        }
        #endregion

        #region hide
        public void Hide()
        {
            _hidden = true;
        }
        #endregion

        #region validate
        internal void Validate()
        {
            string name = _name;
            string help = _help;

            if (!char.IsLetter(name[0]))
                throw new NamespaceDefinitionException("Invalid namespace...Namespace definitions must begin with a letter.");

            int depth = 0;
            for (int i = 1; i < name.Length; i++)
            {
                char c = name[i];
                if (!(char.IsLetter(c) || char.IsDigit(c) || c == '.' || c == '-'))
                    throw new NamespaceDefinitionException("Invalid namespace...Namespace definitions can only contain letters, digits, '-' and '.'");

                if (c == '.')
                    depth += 1;
            }

            _depth = depth;
            //TODO: What to do about carriage returns or line feeds ???
        }
        #endregion
    }
}
