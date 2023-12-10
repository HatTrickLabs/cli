using System;
using System.Collections.Generic;

namespace HatTrick.CommandLine
{
    public interface ICommand
    {
        public string Name { get; }

        public IOption this[string key] { get; }

        public IOption[] GetOptions(Predicate<Option> where = null);
    }
}
