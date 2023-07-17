using System;

namespace HatTrick.CommandLine
{
    public class CommandMappingException : Exception
    {
        public CommandMappingException(string message) : base(message)
        { }
    }
}
