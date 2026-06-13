using System;

namespace HatTrick.CommandLine
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
        public static readonly int MaxNameLength;

        public string Name => _name;

        public string Help => _help;

        internal int Depth => _depth;

        public bool Hidden => _hidden;
        #endregion

        #region constructors
        static NamespaceDefinition()
        {
            MaxNameLength = CommandDefinition.MaxNameLength - 1;//one less than max command name length
        }

        public NamespaceDefinition(string name, string help)
        {
            if (name is null)
                throw new ArgumentNullException(nameof(name));

            if (help is null)
                throw new ArgumentNullException(nameof(help));

            _name = name;
            _help = help;
        }
        #endregion

        #region hide
        public void Hide()
        {
            _hidden = true;
        }
        #endregion

        #region is valid namespace char
        public static bool IsValidNamespaceChar(char c)
        {
            return char.IsLetter(c) || char.IsDigit(c) || c == '.' || c == '-';
        }
        #endregion

        #region validate
        internal void Validate()
        {
            string name = _name;
            string help = _help;

            if (name == string.Empty)
                throw new NamespaceDefinitionException($"Invalid namespace...'{nameof(Name)}' cannot be empty.");

            if (help == string.Empty)
                throw new NamespaceDefinitionException($"Invalid namespace...'{nameof(Help)}' cannot be empty.");

            if (!char.IsLetter(name[0]))
                throw new NamespaceDefinitionException($"Invalid namespace...'{nameof(Name)}' must begin with a letter.");

            if (name.Length > NamespaceDefinition.MaxNameLength)
                throw new NamespaceDefinitionException($"Invalid namespace...Length of '{nameof(Name)}' cannot exceed {MaxNameLength} characters.");

            int depth = 0;
            for (int i = 1; i < name.Length; i++)
            {
                char c = name[i];
                if (!NamespaceDefinition.IsValidNamespaceChar(c))
                    throw new NamespaceDefinitionException($"Invalid namespace...'{nameof(Name)}' can only contain letters, digits, '-' and '.'");

                if (c == '.')
                {
                    if (name[i - 1] == '.')
                        throw new NamespaceDefinitionException($"Invalid namespace...'{nameof(Name)}' cannot contain empty segments ('..').");

                    depth += 1;
                }
            }

            if (name[^1] == '.')
                throw new NamespaceDefinitionException($"Invalid namespace...'{nameof(Name)}' cannot end with '.'");

            _depth = depth;
        }
        #endregion
    }
}
