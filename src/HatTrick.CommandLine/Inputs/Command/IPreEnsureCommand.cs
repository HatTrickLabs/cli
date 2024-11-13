using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HatTrick.CommandLine
{
    public interface IPreEnsureCommand
    {
        public string Name { get; }

        public IPreEnsureOption GetOption(Predicate<IPreEnsureOption> where);
    }
}
