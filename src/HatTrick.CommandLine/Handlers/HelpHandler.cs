using System;

namespace HatTrick.CommandLine
{
    internal class HelpHandler
    {
        #region constructors
        internal HelpHandler()
        {
        }
        #endregion

        #region ensure help argument
        private string EnsureHelpArgument(string argument, out bool hasWildcard)
        {
            string input = argument;

            hasWildcard = input[^1] == '*';
            if (hasWildcard)
                input = input[..^1];

            bool hasDot = input[^1] == '.';
            if (hasDot)
                input = input[..^1];

            return input;
        }
        #endregion

        #region go
        internal void Go(Command cmd)
        {
            var registry = DefinitionRegistry.GetInstance();
            var renderer = new RenderEngine();

            if (cmd["help"] is DefaultOption) //no option flag provided at all...
            {
                renderer.RenderUsageHelp();
                return;
            }

            string argument = cmd["help"].GetValue<string>();
            if (argument is null)//no argument provided for the help option
            {
                renderer.RenderRootHelp();
                return;
            }

            //an arg was provided for the help option
            string input = this.EnsureHelpArgument(argument, out bool hasWildcard);
            if (registry.TryGetNamespaceDefinition(input, out NamespaceDefinition namespaceDef))
            {
                if (hasWildcard)
                    renderer.RenderNamespaceWildcardHelp(namespaceDef);
                else
                    renderer.RenderNamespaceHelp(namespaceDef);
            }
            else if (registry.TryGetCommandDefinition(input, out CommandDefinition cmdDef))
            {
                if (hasWildcard)
                    throw new CommandInputException("Provided argument is a command...Command help does not support wildcards.");

                renderer.RenderCommandHelp(cmdDef);
            }
            else
                throw new CommandInputException($"Provided argument '{input}' is not a command or namespace.");
        }
        #endregion
    }
}
