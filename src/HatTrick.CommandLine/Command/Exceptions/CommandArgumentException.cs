using System;

namespace Crypto.CommandLine
{
    public class CommandArgumentException : Exception
    {
        public CommandArgumentException(string message) : base(message)
        {
        }
    }
}
