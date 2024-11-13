using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HatTrick.CommandLine
{
    public interface IPreEnsureOption
    {
        public string Flag { get; }

        public string Argument { get; }

        public void OverrideArgument(string argument);
    }
}
