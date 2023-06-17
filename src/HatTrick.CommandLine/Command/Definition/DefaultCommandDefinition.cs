using System;

namespace HatTrick.CommandLine
{
    public class DefaultCommandDefinition : CommandDefinition
    {
        #region constructors
        public DefaultCommandDefinition() : base(name: DefaultCommandName)
        { }
        #endregion
    }
}
