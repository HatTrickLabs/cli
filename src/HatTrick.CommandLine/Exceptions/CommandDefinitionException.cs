using System;

namespace HatTrick.CommandLine
{
    public class CommandDefinitionException : Exception
    {
        public CommandDefinitionException(string message) : base(message)
        {
        }

        public CommandDefinitionException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
