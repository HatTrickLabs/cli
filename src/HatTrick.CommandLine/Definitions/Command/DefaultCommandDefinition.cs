using System;

namespace HatTrick.CommandLine
{
    internal class DefaultCommandDefinition : CommandDefinition
    {
        #region const
        public const string CommandName = "DEFAULT";
        #endregion

        #region constructors
        internal DefaultCommandDefinition() : base(name: DefaultCommandDefinition.CommandName)
        {
            base.Help = "???";
            base.Hide();
            base.Handler = this.DefaultCommandHandler;

            var helpOp = new OptionDefinition<string>(
                key: "help",
                defaultArg: null,
                help: "Display help (accepts a command or namespace as argument).",
                converter: OptionTypeMap.ParseOptionArgument<string>,
                ("-h", "--help")
            );
            base.Options.Add(helpOp);

            var verOp = new OptionDefinition<bool>(
                key: "version", 
                defaultArg: false, 
                help: "Displays version information.", 
                converter: OptionTypeMap.ParseOptionArgument<bool>,
                ("-v", "--version")
            );
            base.Options.Add(verOp);

            var runOp = new OptionDefinition<bool>(
                key: "run", 
                defaultArg: false, 
                help: "Run a non-exiting command loop (accepts a namespace as argument).", 
                converter: OptionTypeMap.ParseOptionArgument<bool>,
                ("-r", "--run")
            );
            base.Options.Add(runOp);

            base.MutuallyExclusiveSet("help", "version", "run");
        }
        #endregion

        #region default command handler
        private void DefaultCommandHandler(ICommand cmd)
        {
            //command definition ensures only one of these can be true...
            bool version = cmd["version"].GetValue<bool>();
            bool run = !version && cmd["run"].GetValue<bool>();
            bool help = !run && !version;

            if (help)
                this.InvokeHelp(cmd as Command);

            else if (version)
                this.InvokeVersion(cmd as Command);

            else if (run)
                this.InvokeRun(cmd as Command);
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
            base.Validate();
        }
        #endregion
    }
}
