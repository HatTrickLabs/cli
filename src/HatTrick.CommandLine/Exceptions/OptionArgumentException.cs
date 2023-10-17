using System;

namespace HatTrick.CommandLine
{
    public class OptionArgumentException : Exception
    {
        public OptionArgumentException(string message) : base(message)
        {
        }
    }
}
