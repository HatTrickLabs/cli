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
            this.EnsureCommandOptions(command);

            this.EnsureCommandConstraints(command);
        }
        #endregion

        #region ensure command options
        private void EnsureCommandOptions(Command cmd)
        {
            this.EnsureCommandOptionsFullyHydrated(cmd);

            this.EnsureAllProvidedOptionsDefined(cmd);

            this.EnsureNoDuplicateOptions(cmd);

            this.EnsureOptionConstraints(cmd);
        }
        #endregion

        #region ensure options fully hydrated
        private void EnsureCommandOptionsFullyHydrated(Command cmd)
        {
            CommandDefinition cmdDef = _cmdDef;

            for (int i = 0; i < cmdDef.Options.Length; i++)
            {
                var opDef = cmdDef.Options[i];
                var op = cmd.GetOptionByFlag(opDef.Flags.Verbose, opDef.Flags.Terse);

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
                    opDef.ApplyConvertedValueTo(op/*, out string error*/);
                }
            }
        }
        #endregion

        #region ensure all provided options defined
        private void EnsureAllProvidedOptionsDefined(Command cmd)
        {
            CommandDefinition cmdDef = _cmdDef;

            if (cmd.Options.Length > 0 && !cmdDef.HasOptions)
                throw new CommandInputException($"The '{cmdDef.Name}' command does not accept any options...provided options are invalid.");

            //if any options are defined for the command, confirm each option provided is valid
            for (int i = 0; i < cmd.Options.Length; i++)
            {
                var op = cmd.Options[i];

                //empty ops can always be assumed to be valid...because they were injected not provided
                if (op is EmptyOption)
                    continue;

                if (!cmdDef.Options.Exists(o => o.Flags.Terse == op.Flag || o.Flags.Verbose == op.Flag))
                    throw new CommandInputException($"Undefined option at position: {i + 1} ... option: {op.Flag}");
            }
        }
        #endregion

        #region ensure no duplicate options
        private void EnsureNoDuplicateOptions(Command cmd)
        {
            CommandDefinition cmdDef = _cmdDef;

            for (int i = 0; i < cmdDef.Options.Length; i++)
            {
                var opDef = cmdDef.Options[i];
                var opOne = cmd.GetOptionByFlag(opDef.Flags.Verbose, opDef.Flags.Terse);
                for (int j = 0; j < cmd.Options.Length; j++)
                {
                    var opTwo = cmd.Options[j];

                    if (opTwo == opOne)
                        continue;

                    if (opDef.Flags.Verbose == opTwo.Flag || opDef.Flags.Terse == opTwo.Flag)
                        throw new CommandInputException($"Duplicate options provided at positions: {i + 1} and {j + 1}...'{opOne.Flag}' and '{opTwo.Flag}'");
                }
            }
        }
        #endregion

        #region ensure option constraints
        private void EnsureOptionConstraints(Command cmd)
        {
            CommandDefinition cmdDef = _cmdDef;

            for (int i = 0; i < cmdDef.Options.Length; i++)
            {
                OptionDefinition opDef = cmdDef.Options[i];
                if (!opDef.HasConstraints)
                    continue;

                ref Option op = ref cmd.GetOptionByRef(opDef.Key);

                foreach (var c in opDef.Constraints)
                {
                    //If EMPTY op and a DEFAUL constraint exists, EMPTY op will be swapped for a DEFAULT op...hence the ref param
                    c.Ensure(ref op);
                }
            }
        }
        #endregion

        #region ensure command constraints
        internal void EnsureCommandConstraints(Command command)
        {
            SetOf<CommandConstraint> constraints = _cmdDef.Constraints;
            if (constraints is null || constraints.Length == 0)
                return;

            foreach (var c in constraints)
            {
                c.Ensure(command);
            }
        }
        #endregion
    }
}
