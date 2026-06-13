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
        private bool _synthetic;
        #endregion

        #region interface
        public static readonly int MaxNameLength;

        public string Name => _name;

        public string Help => _help;

        internal int Depth => _depth;

        public bool Hidden => _hidden;

        internal bool Synthetic => _synthetic;
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

        private NamespaceDefinition(string name, int depth)
        {
            _name = name;
            _depth = depth;
            _synthetic = true;
        }
        #endregion

        #region synthetic
        //a synthetic namespace is an auto-created structural placeholder for an unregistered
        //ancestor of a registered namespace. it carries no help text until promoted.
        internal static NamespaceDefinition CreateSynthetic(string name)
        {
            int depth = 0;
            for (int i = 0; i < name.Length; i++)
                if (name[i] == '.')
                    depth += 1;

            return new NamespaceDefinition(name, depth);
        }

        //promote a synthetic placeholder into a real, author-declared namespace.
        internal void Promote(string help)
        {
            if (help is null)
                throw new ArgumentNullException(nameof(help));

            _help = help;
            _synthetic = false;
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

                if (c == '-' && (name[i - 1] == '.' || (i < name.Length - 1 && name[i + 1] == '.')))
                    throw new NamespaceDefinitionException($"Invalid namespace...'{nameof(Name)}' segments cannot start or end with '-'.");
            }

            if (name[^1] == '.' || name[^1] == '-')
                throw new NamespaceDefinitionException($"Invalid namespace...'{nameof(Name)}' cannot end with '.' or '-'.");

            _depth = depth;
        }
        #endregion
    }
}
