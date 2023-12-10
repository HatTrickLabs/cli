using System;

namespace HatTrick.CommandLine
{
    public interface IOption
    {
        public string Key { get; }

        public string Flag { get; }

        public string Argument { get; }

        public bool HasValue { get; }

        public T GetValue<T>();
    }
}
