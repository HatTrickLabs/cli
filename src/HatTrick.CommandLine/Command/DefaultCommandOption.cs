using System;

namespace HatTrick.CommandLine
{
    public class DefaultCommandOption : CommandOption
    {
        public bool IsDefault => true;

        public DefaultCommandOption(string key, string flag) : base(key, flag)
        { }
    }
}
