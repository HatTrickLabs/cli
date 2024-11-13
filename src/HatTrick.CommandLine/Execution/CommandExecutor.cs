using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace HatTrick.CommandLine
{
    public class CommandExecutor
    {
        #region internals
        private CommandDefinition _cmdDef;
        private Command _cmd;
        private bool _ensured;
        #endregion

        #region constructors
        internal CommandExecutor(CommandDefinition commandDefinition, Command command)
        {
            _cmdDef = commandDefinition ?? throw new ArgumentNullException(nameof(commandDefinition));
            _cmd = command ?? throw new ArgumentNullException(nameof(command));
        }
        #endregion

        #region execute
        public void Execute()
        {
            Action<Command> handler = _cmdDef.Handler;
            if (handler is null)
                throw new CommandExecutionException($"Command definition '{_cmdDef.Name}' ... {_cmdDef.GetType().Name}.{nameof(CommandDefinition.Handler)} is null.");

            _cmdDef.OnPreEnsure?.Invoke(_cmd);
            this.EnsureCommand(_cmd);
            //_cmdDef.OnPostEnsure?.Invoke(_cmd);

            _cmdDef.Handler(_cmd);
        }
        #endregion

        #region execute async
        public async Task ExecuteAsync()
        {
            this.EnsureCommand(_cmd);

            Func<Command, Task> function = _cmdDef.AsyncHandler;

            if (function is null)
                throw new CommandExecutionException($"{nameof(CommandDefinition)}.{nameof(CommandDefinition.AsyncHandler)} is null for command definition '{_cmdDef.Name}'.");

            await _cmdDef.AsyncHandler(_cmd);
        }
        #endregion

        #region ensure command
        private void EnsureCommand(Command command)
        {
            if (!_ensured)
            {
                this.EnsureCommandOptions(command);
                this.EnsureCommandConstraints(command);
                _ensured = true;
            }
        }
        #endregion

        #region ensure command options
        private void EnsureCommandOptions(Command command)
        {
            this.TryHydrateUnflaggedOptions(command);
            this.EnsureCommandOptionsFullyHydrated(command);
            this.EnsureAllProvidedOptionsDefined(command);
            this.EnsureNoDuplicateOptions(command);
            this.EnsureOptionConstraints(command);
        }
        #endregion

        #region try hydrate unflagged options
        private void TryHydrateUnflaggedOptions(Command command)
        {
            CommandDefinition cmdDef = _cmdDef;

            int definedLen = cmdDef.Options.Length;
            int suppliedLen = command.Options.Length;

            for (int i = 0; i < suppliedLen; i++)
            {
                if (definedLen > i)
                {
                    ref Option op = ref command.Options.GetPointerTo(i);
                    bool isUnflagged = op is UnflaggedOption;
                    if (!isUnflagged)
                        break;

                    //if the parser parsed it unflagged, swap it with a proper option
                    var opDef = cmdDef.Options[i];
                    op = new Option(opDef.Key, opDef.Flags.Verbose, op.Argument);

                    //apply the converted argument value to the option
                    this.EnsureOptionValue(op, opDef);
                }
            }
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
                    this.EnsureOptionValue(op, opDef);
                }
            }
        }
        #endregion

        #region ensure option value
        private void EnsureOptionValue(Option option, OptionDefinition optionDef)
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

                if (!cmdDef.Options.Exists(o => o.Flags.IsMatch(op.Flag)))
                    throw new CommandInputException($"Undefined option at position: {i + 1} ... option: {op.Flag} for command definition '{cmdDef.Name}'.");
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

                    if (opDef.Flags.IsMatch(opTwo.Flag))
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
                    //If EMPTY op and a DEFAULT constraint exists, EMPTY op will be swapped for a DEFAULT op...hence the ref param
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
