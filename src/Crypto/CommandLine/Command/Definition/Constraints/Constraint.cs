using System;

namespace Crypto.CommandLine
{
    public abstract class Constraint
    {
        internal abstract bool ReferencesOption(string optionKey);
        internal abstract bool TryApply(CommandOption option);
    }
}
