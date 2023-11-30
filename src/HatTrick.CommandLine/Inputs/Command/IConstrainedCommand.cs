using System;
using System.Collections.Generic;

namespace HatTrick.CommandLine
{
    public interface ICommand
    {
        public string Name { get; }

        public Option this[string key] { get; }

        public Option[] GetOptions(Predicate<Option> where = null);
    }
}
