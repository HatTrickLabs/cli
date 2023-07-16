namespace HatTrick.CommandLine
{
    internal class DefaultCommandDefinition : CommandDefinition
    {
        #region constructors
        internal DefaultCommandDefinition() : base(name: CommandDefinition.DefaultCommandName)
        {
            base.Help = "???";
            base.Hide();
            base.Handler = this.DefaultCommandHandler;

            base.AddOption(
                key: "help", 
                mustAssign: false, 
                help: "Display help (accepts a command or namespace as argument).", 
                type: OpType.String, 
                "-?", "-h", "--help"
            );

            base.AddOption(
                key: "version", 
                mustAssign: false, 
                help: "Display crypto version information.", 
                type: OpType.Bool, 
                "-v", "--version"
            );

            base.AddOption(
                key: "run", 
                mustAssign: false, 
                help: "Run crypto in a non-exiting command loop.", 
                type: OpType.Bool, 
                "-r", "--run"
            );

            base.MutaullyExclusiveSet("help", "version", "run");
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

        #region default command handler
        private void DefaultCommandHandler(Command cmd)
        {
            //command def ensures only one of these can be true...
            bool version = cmd["version"].Value ?? false;
            bool run = cmd["run"].Value ?? false;
            bool help = cmd["help"] is not EmptyCommandOption || !(run || version);

            if (help)
                this.InvokeHelp(cmd);

            else if (version)
                this.InvokeVersion(cmd);

            else if (run)
                this.InvokeRun(cmd);
        }
        #endregion

        #region help
        private void InvokeHelp(Command cmd)
        {
            var registry = CommandDefinitionRegistry.GetInstance();
            var renderer = new RenderEngine();

            if (cmd["help"] is EmptyCommandOption) //no option flag provided at all...
            {
                renderer.RenderUsageHelp();
                return;
            }

            string argument = cmd["help"].Value;
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
                    throw new CommandInputException("Provided argument is a command...Command help does not support wildcards");

                renderer.RenderCommandHelp(cmdDef);
            }
            else
                throw new CommandInputException($"Provided argument is not a command or namespace: {input}");
        }
        #endregion

        #region version
        private void InvokeVersion(Command cmd)
        {
            //System.Reflection.Assembly.GetExecutingAssembly().GetName().Version
        }
        #endregion

        #region run command loop
        private void InvokeRun(Command cmd)
        {
        }
        #endregion
    }
}
