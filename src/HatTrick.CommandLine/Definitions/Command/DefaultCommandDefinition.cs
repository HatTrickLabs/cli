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
                converter: OptionTypeMap.ParseOptionArgument<string>,
                "-?", "-h", "--help"
            );
            base.Options.Add(helpOp);

            var verOp = new CommandOptionDefinition<bool>(
                key: "version", 
                defaultArg: false, 
                help: "Displays version information.", 
                converter: OptionTypeMap.ParseOptionArgument<bool>,
                "-v", "--version");
            base.Options.Add(verOp);

            var runOp = new CommandOptionDefinition<bool>(
                key: "run", 
                defaultArg: false, 
                help: "Run a non-exiting command loop.", 
                converter: OptionTypeMap.ParseOptionArgument<bool>,
                "-r", "--run");
            base.Options.Add(runOp);

            base.MutaullyExclusiveSet("help", "version", "run");
        }
        #endregion

        #region default command handler
        private void DefaultCommandHandler(Command cmd)
        {
            //command def ensures only one of these can be true...
            bool version = cmd["version"].GetValue<bool>();
            bool run = cmd["run"].GetValue<bool>();
            bool help = cmd["help"] is not DefaultOption || !(run || version);

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
            CommandLoopHandler.GetInstance().Go(cmd);
        }
        #endregion

        #region validate
        internal override void Validate()
        {
            //base.Validate();
        }
        #endregion
    }
}
