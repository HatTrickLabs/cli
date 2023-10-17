using System;
using System.Threading.Tasks;

namespace HatTrick.CommandLine
{
    public class CommandExecutor
    {
        #region internals
        private CommandDefinition _cmdDef;
        #endregion

        #region constructors
        internal CommandExecutor(CommandDefinition commandDefinition)
        {
            _cmdDef = commandDefinition ?? throw new ArgumentNullException(nameof(commandDefinition));
        }
        #endregion

        #region execute command
        public void ExecuteCommand(Command command)
        {
            if (command.Name is null || string.IsNullOrEmpty(command.Name))
                throw new CommandInputException("No command provided.");

            this.EnsureCommand(command);

            _cmdDef.Handler(command);
        }
        #endregion

        #region execute command async
        public async Task ExecuteCommandAsync(Command command)
        {
            if (command.Name is null || string.IsNullOrEmpty(command.Name))
                throw new CommandInputException("No command provided.");

            this.EnsureCommand(command);

            await _cmdDef.AsyncHandler(command);
        }
        #endregion

        #region ensure command
        private void EnsureCommand(Command command)
        {
            var feedback = new SetOf<string>();

            this.EnsureCommandOptions(command, ref feedback);

            if (feedback.Length == 0)
                this.EnsureConstraints(command, ref feedback);

            if (feedback.Length > 0)
                throw new CommandInputException(feedback.ToArray());
        }
        #endregion

        #region ensure command options
        private void EnsureCommandOptions(Command cmd, ref SetOf<string> feedback)
        {
            this.EnsureCommandOptionsFullyHydrated(cmd, ref feedback);
            if (feedback.Length > 0)
                return;

            this.EnsureAllProvidedOptionsDefined(cmd, ref feedback);
            if (feedback.Length > 0)
                return;

            this.EnsureNoDuplicateOptions(cmd, ref feedback);
            if (feedback.Length > 0)
                return;

            this.EnsureOptionConstraints(cmd, ref feedback);
        }
        #endregion

        #region ensure options fully hydrated
        private void EnsureCommandOptionsFullyHydrated(Command cmd, ref SetOf<string> feedback)
        {
            CommandDefinition cmdDef = _cmdDef;

            for (int i = 0; i < cmdDef.Options.Length; i++)
            {
                var opDef = cmdDef.Options[i];
                var op = cmd.GetOptionByFlag(opDef.Flags.verbose, opDef.Flags.terse);

                if (op is null)
                {
                    //empty, just need an empty shell with correct key
                    cmd.AddEmptyOption(opDef.EmptyInstance());
                }
                else
                {
                    //apply the definition key to the option
                    op.ApplyKey(opDef.Key);

                    //pass op to opDef to set the value (passing in because only the def knows what T is).
                    opDef.ApplyConvertedValueTo(op, out string error);
                    if (error is not null)
                        feedback.Add(error);
                }
            }
        }
        #endregion

        #region ensure all provided options defined
        private void EnsureAllProvidedOptionsDefined(Command cmd, ref SetOf<string> feedback)
        {
            CommandDefinition cmdDef = _cmdDef;

            if (cmd.Options.Length > 0 && !cmdDef.HasOptions)
            {
                feedback.Add($"The '{cmdDef.Name}' command does not accept any options...provided options are invalid.");
                return;
            }

            //if any options are defined for the command, confirm each option provided is valid
            for (int i = 0; i < cmd.Options.Length; i++)
            {
                var op = cmd.Options[i];

                //empty ops can always be assumed to be valid...because they were injected not provided
                if (op is EmptyOption)
                    continue;

                if (!cmdDef.Options.Exists(o => o.Flags.terse == op.Flag || o.Flags.verbose == op.Flag))
                    feedback.Add($"Undefined option at position: {i + 1} ... option: {op.Flag}");
            }
        }
        #endregion

        #region ensure no duplicate options
        private void EnsureNoDuplicateOptions(Command cmd, ref SetOf<string> feedback)
        {
            CommandDefinition cmdDef = _cmdDef;

            //TODO: refactor, this looks fundamentally wrong
            for (int i = 0; i < cmdDef.Options.Length; i++)
            {
                var opDef = cmdDef.Options[i];
                var opUno = cmd.GetOptionByFlag(opDef.Flags.verbose, opDef.Flags.terse);
                for (int j = 0; j < cmd.Options.Length; j++)
                {
                    var opDos = cmd.Options[j];

                    if (opDos == opUno)
                        continue;

                    if (opDef.Flags.verbose == opDos.Flag || opDef.Flags.terse == opDos.Flag)
                        feedback.Add($"Duplicate options provided at positions: {i + 1} and {j + 1}...'{opUno.Flag}' and '{opDos.Flag}'");
                }
            }
        }
        #endregion

        #region ensure option constraints
        private void EnsureOptionConstraints(Command cmd, ref SetOf<string> feedback)
        {
            CommandDefinition cmdDef = _cmdDef;

            for (int i = 0; i < cmdDef.Options.Length; i++)
            {
                OptionDefinition opDef = cmdDef.Options[i];
                ref Option op = ref cmd.GetOptionByRef(opDef.Key);

                //If empty op and a default constraint exists, empty op will be swapped for a default...hence the ref param
                opDef.EnsureConstraints(ref op, ref feedback);
            }
        }
        #endregion

        #region ensure constraints
        internal void EnsureConstraints(Command command, ref SetOf<string> feedback)
        {
            SetOf<CommandConstraint> constraints = _cmdDef.Constraints;
            if (constraints is null || constraints.Length == 0)
                return;

            for (int i = 0; i < constraints.Length; i++)
            {
                var c = constraints[i];
                if (!c.Ensure(command, out string fb))
                    feedback.Add(fb);
            }
        }
        #endregion
    }
}
