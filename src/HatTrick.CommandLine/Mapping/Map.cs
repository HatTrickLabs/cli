using System;

namespace HatTrick.CommandLine
{
    public abstract class Map
    {
        #region internals
        private CommandDefinition _cmdDef;
        private (string optionKey, string to)[] _correlations;
        #endregion

        #region interface
        protected CommandDefinition CommandDefinition => _cmdDef;

        protected (string optionKey, string to)[] Correlations => _correlations;
        #endregion

        #region constructors
        public Map(CommandDefinition commandDef) : this(commandDef, null)
        {
        }

        public Map(CommandDefinition commandDef, (string optionKey, string to)[] correlations)
        {
            _cmdDef = commandDef ?? throw new ArgumentNullException(nameof(commandDef));
            _correlations = correlations;
        }
        #endregion

        #region register validator
        protected abstract void RegisterValidator();
        #endregion

        #region validate
        public virtual void Validate()
        {
            EnsureCorrelations();
        }
        #endregion

        #region correlation exists for option key
        public bool CorrelationExistsForOptionKey(string optionKey, out (string optionKey, string to) correlation)
        {
            correlation = _correlations is null || _correlations.Length == 0
                ? default
                : Array.Find(_correlations, (c) => c.optionKey == optionKey);

            return correlation != default;
        }
        #endregion

        #region correlation exists for map target
        public bool CorrelationExistsForMapTarget(string to, out (string optionKey, string to) correlation)
        {
            correlation = _correlations is null || _correlations.Length == 0
                ? default
                : Array.Find(_correlations, (c) => c.to == to);

            return correlation != default;
        }
        #endregion

        #region ensure correlations
        protected void EnsureCorrelations()
        {
            var correlations = _correlations;

            if (correlations is null || correlations.Length == 0)
                return;

            var cmdDef = _cmdDef;

            foreach (var c in correlations)
            {
                if (cmdDef.GetOption(c.optionKey) is null)
                    throw new CommandMappingException($"Command '{cmdDef.Name} does not contain an option key that matches provided correlation: {c}");
            }
        }
        #endregion
    }
}
