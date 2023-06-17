using System;

namespace HatTrick.CommandLine
{
    public class EmptyCommandOption : CommandOption
    {
        public bool IsEmpty => true;

        public EmptyCommandOption(string key, string flag) : base(key, flag)
        { }
    }
}
