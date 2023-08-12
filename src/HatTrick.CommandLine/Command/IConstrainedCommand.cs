using System;
using System.Collections.Generic;

namespace HatTrick.CommandLine
{
    public interface IConstrainedCommand
    {
        public string Key { get; }

        public CommandOption this[string key] { get; }

        public CommandOption[] GetOptions(Predicate<CommandOption> where = null);
    }
}
