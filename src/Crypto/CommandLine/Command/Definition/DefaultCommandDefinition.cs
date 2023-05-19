using System;

namespace Crypto.CommandLine
{
    public class DefaultCommandDefinition : CommandDefinition
    {
        #region constructors
        public DefaultCommandDefinition() : base(name: DefaultCommandName)
        { }
        #endregion
    }
}
