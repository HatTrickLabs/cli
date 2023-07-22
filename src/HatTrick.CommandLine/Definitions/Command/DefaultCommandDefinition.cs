using System;

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

            var helpOp = new CommandOptionDefinition<string>(
                key: "help",
                defaultArg: null,
                help: "Display help (accepts a command or namespace as argument).",
                converter: (arg) => arg,
                "-?", "-h", "--help"
            );
            base.Options.Add(helpOp);

            var verOp = new CommandOptionDefinition<bool>(
                key: "version", 
                defaultArg: false, 
                help: "Displays version information.", 
                converter: BooleanConverter.ToBoolean, 
                "-v", "--version");
            base.Options.Add(verOp);

            var runOp = new CommandOptionDefinition<bool>(
                key: "run", 
                defaultArg: false, 
                help: "Run in a non-exiting command loop.", 
                converter: BooleanConverter.ToBoolean, 
                "-r", "--run");
            base.Options.Add(runOp);

            base.MutaullyExclusiveSet("help", "version", "run");
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
            new HelpHandler().Go(cmd);
        }
        #endregion

        #region version
        private void InvokeVersion(Command cmd)
        {
            new VersionInquiryHandler().Go(cmd);
        }
        #endregion

        #region run
        private void InvokeRun(Command cmd)
        {
            new CommandLoopHandler().Go(cmd);
        }
        #endregion
    }
}
