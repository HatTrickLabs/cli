using System;

namespace HatTrick.CommandLine
{
    public class CommandParseException : Exception
    {
        public CommandParseException(string message) : base(message)
        {
        }
    }
}
