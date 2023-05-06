using System;

namespace Crypto.CommandLine
{
    public class EmptyCommandOption : CommandOption
    {
        public bool IsEmpty => true;
    }
}
