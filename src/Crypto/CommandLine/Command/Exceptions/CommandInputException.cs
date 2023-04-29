using System;
using System.Collections.Generic;

namespace Crypto.CommandLine
{
    public class CommandInputException : Exception
    {
        public CommandInputException(params string[] messages) 
            : base($"Invalid input:{Environment.NewLine}{string.Join(Environment.NewLine, messages)}{Environment.NewLine}")
        {
        }
    }
}
