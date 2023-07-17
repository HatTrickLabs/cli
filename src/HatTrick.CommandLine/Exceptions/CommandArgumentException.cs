using System;

namespace HatTrick.CommandLine
{
    public class CommandArgumentException : Exception
    {
        public CommandArgumentException(string message) : base(message)
        {
        }
    }
}
