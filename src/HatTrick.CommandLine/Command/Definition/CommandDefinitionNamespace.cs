using System;

namespace Crypto.CommandLine
{
    public class CommandDefinitionNamespace
    {
        #region internals
        private string _name;
        private string _help;
        #endregion

        #region interface
        public string Name => _name;

        public string Help => _help;
        #endregion

        #region constructors
        public CommandDefinitionNamespace(string name, string help)
        {
            _name = (name == null)
                ? throw new ArgumentNullException(nameof(name))
                : (name == string.Empty)
                    ? throw new ArgumentException("Argument value cannot be empty.", nameof(name))
                    : name;

            _help = (help == null)
                ? throw new ArgumentNullException(nameof(help))
                : (help == string.Empty)
                    ? throw new ArgumentException("Argument value cannot be empty.", nameof(help))
                    : help;
        }
        #endregion

        #region validate
        internal void Validate()
        {
            string name = _name;
            string help = _help;

            if (!char.IsLetter(name[0]))
                throw new CommandDefinitionException("Invalid namespace...Namespace definitions must begin with a letter.");

            for (int i = 1; i < name.Length; i++)
            {
                char c = name[i];
                if (!(char.IsLetter(c) || char.IsDigit(c) || c == '.' || c == '-'))
                    throw new CommandDefinitionException("Invalid namespace...Namespace definitions can only contain letters, digits, '-' and '.'");
            }

            if (string.IsNullOrWhiteSpace(help))
            {
                help = "No help content provided.";
            }
            else
            {
                //TODO: What to do about carriage returns or line feeds ???
            }
        }
        #endregion
    }
}
