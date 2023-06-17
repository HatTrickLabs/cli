using System;

namespace Crypto.CommandLine
{
    public class CommandDefinitionException : Exception
    {
        public CommandDefinitionException(string message) : base(message)
        {
        }
    }
}
