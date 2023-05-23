using System;

namespace Crypto.CommandLine
{
    public class ArgumentConstraintException : Exception
    {
        public ArgumentConstraintException(string optionKey, string argument, string message) : base(message)
        {
        }
    }
}
