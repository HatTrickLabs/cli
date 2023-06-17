using System;

namespace Crypto.CommandLine
{
    public class CommandMappingException : Exception
    {
        public CommandMappingException(string message) : base(message)
        { }
    }
}
