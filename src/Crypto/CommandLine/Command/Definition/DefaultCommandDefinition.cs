using System;

namespace Crypto.CommandLine
{
    public class DefaultCommandDefinition : CommandDefinition
    {
        #region constructors
        public DefaultCommandDefinition() : base(name: DefaultCommandName)
        { }

        public DefaultCommandDefinition(string help, Action<Command> entryPoint, params CommandOptionDefinition[] options)
            : base(DefaultCommandName, help, entryPoint, options)
        { }
        #endregion
    }
}
