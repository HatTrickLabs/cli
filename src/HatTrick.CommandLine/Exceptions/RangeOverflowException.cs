using System;

namespace HatTrick.CommandLine
{
    public class RangeOverflowException : Exception
    {
        public RangeOverflowException(string message) : base(message)
        { }
    }
}
