using System;
using System.Threading.Tasks;

namespace HatTrick.CommandLine
{
    public class CommandExecutor
    {
        #region internals
        private CommandDefinition _cmdDef;
        private Command _cmd;
        #endregion

        #region constructors
        internal CommandExecutor(CommandDefinition commandDefinition, Command command)
        {
            _cmdDef = commandDefinition ?? throw new ArgumentNullException(nameof(commandDefinition));
            _cmd = command ?? throw new ArgumentNullException(nameof(command));

            if (command.Name is null)
                throw new ArgumentNullException($"{nameof(command)}.{nameof(command.Name)}");

            if (command.Name == string.Empty)
                throw new ArgumentException("Property must contain a value.", $"{nameof(command)}.{nameof(command.Name)}");
        }
        #endregion

        #region execute
        public void Execute()
        {
            this.EnsureCommand(_cmd);
            _cmdDef.Handler(_cmd);
        }
        #endregion

        #region execute command async
        public async Task ExecuteCommandAsync(Command command)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

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
        private void EnsureCommandOptions(Command command)
        {
            this.EnsureCommandOptionsFullyHydrated(command);
            this.EnsureAllProvidedOptionsDefined(command);
            this.EnsureNoDuplicateOptions(command);
            this.EnsureOptionConstraints(command);
        }
        #endregion

        #region ensure options fully hydrated
        private void EnsureCommandOptionsFullyHydrated(Command command)
        {
            CommandDefinition cmdDef = _cmdDef;

            foreach (var opDef in cmdDef.Options)
            {
                var op = command.GetOptionByFlag(opDef.Flags.Verbose, opDef.Flags.Terse);

                if (op is null)
                {
                    //op not provided, just need an empty shell with correct key
                    command.AddEmptyOption(opDef.EmptyInstance());
                }
                else
                {
                    //apply the definition key to the option
                    op.SetKey(opDef.Key);

                    //apply the converted argument value to the option
                    this.EnsureOptionArgumentValue(op, opDef);
                }
            }
        }
        #endregion

        #region ensure option argument value
        private void EnsureOptionArgumentValue(Option option, OptionDefinition optionDef)
        {
            if (option.Key != optionDef.Key)
                throw new ArgumentException($"Key of option: {option.Key} does not match key of option definition: {optionDef.Key}");

            try
            {
                var converter = optionDef.GetArgumentConverter();
                var val = converter.Invoke(option.Argument);
                option.SetValue(val);
            }
            catch
            {
                var flag = option.Flag;
                var name = optionDef.GenericType.Name;
                var arg = option.Argument;
                throw new OptionArgumentException($"Option '{flag}' requires argument of type '{name}'...invalid value provided: '{arg}'");
            }
        }
        #endregion

        #region ensure all provided options defined
        private void EnsureAllProvidedOptionsDefined(Command command)
        {
            CommandDefinition cmdDef = _cmdDef;

            if (command.Options.Length > 0 && !cmdDef.HasOptions)
                throw new CommandInputException($"The '{cmdDef.Name}' command does not accept any options...provided options are invalid.");

            //if any options are defined for the command, confirm each option provided is valid
            for (int i = 0; i < command.Options.Length; i++)
            {
                var op = command.Options[i];

                //empty ops can always be assumed to be valid...because they were injected not provided
                if (op is EmptyOption)
                    continue;

                if (!cmdDef.Options.Exists(o => o.Flags.Terse == op.Flag || o.Flags.Verbose == op.Flag))
                    throw new CommandInputException($"Undefined option at position: {i + 1} ... option: {op.Flag}");
            }
        }
        #endregion

        #region ensure no duplicate options
        private void EnsureNoDuplicateOptions(Command command)
        {
            CommandDefinition cmdDef = _cmdDef;

            for (int i = 0; i < cmdDef.Options.Length; i++)
            {
                var opDef = cmdDef.Options[i];
                var opOne = command.GetOptionByFlag(opDef.Flags.Verbose, opDef.Flags.Terse);
                for (int j = 0; j < command.Options.Length; j++)
                {
                    var opTwo = command.Options[j];

                    if (opTwo == opOne)
                        continue;

                    if (opDef.Flags.Verbose == opTwo.Flag || opDef.Flags.Terse == opTwo.Flag)
                        throw new CommandInputException($"Duplicate options provided at positions: {i + 1} and {j + 1}...'{opOne.Flag}' and '{opTwo.Flag}'");
                }
            }
        }
        #endregion

        #region ensure option constraints
        private void EnsureOptionConstraints(Command command)
        {
            CommandDefinition cmdDef = _cmdDef;

            for (int i = 0; i < cmdDef.Options.Length; i++)
            {
                OptionDefinition opDef = cmdDef.Options[i];
                if (!opDef.HasConstraints)
                    continue;

                ref Option op = ref command.GetOptionByRef(opDef.Key);

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
            if (!_cmdDef.HasConstraints)
                return;

            SetOf<CommandConstraint> constraints = _cmdDef.Constraints;
            foreach (var c in constraints)
            {
                c.Ensure(command);
            }
        }
        #endregion
    }
}
