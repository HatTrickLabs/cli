using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.ComponentModel.Design;
using System.Linq;

namespace HatTrick.CommandLine
{
    public class CommandDefinition
    {
        #region const
        public const string DefaultCommandName = "DEFAULT";
        #endregion

        #region internals
        private string _name;
        private string _help;
        private int _depth;
        private bool _hidden;
        private Action _mappedHanderValidators;
        private Action<Command> _handler;
        private Func<Command, Task> _asyncHandler;
        private SetOfOptionDefinition _options;
        private SetOf<CommandConstraint> _constraints;
        #endregion

        #region interface
        public static readonly int MaxNameLength;

        public string Name => _name;

        public string Help
        {
            get => _help;
            set => _help = value;
        }

        public bool Hidden => _hidden;

        internal int Depth => _depth;

        public Action<Command> Handler
        {
            get => _handler;
            set => _handler = value;
        }

        public Func<Command, Task> AsyncHandler
        {
            get => _asyncHandler;
            set => _asyncHandler = value;
        }

        public bool HasOptions => _options is not null && _options.Length > 0;

        internal SetOfOptionDefinition Options => _options;

        public OptionDefinition this[string key]
        {
            get
            {
                var op = _options.Find(o => o.Key == key);

                if (op == default)
                    throw new KeyNotFoundException($"Provided option key: '{key}' not found.");

                return op;
            }
        }

        internal SetOf<CommandConstraint> Constraints
        {
            get => _constraints;
            set => _constraints = value;
        }

        public bool HasConstraints => _constraints.Length > 0;
        #endregion

        #region constructors
        static CommandDefinition()
        {
            MaxNameLength = 64;
        }

        public CommandDefinition(string name)
        {
            if (name is null)
                throw new ArgumentNullException(nameof(name));

            _name = name;
            _options = new SetOfOptionDefinition();
            _constraints = new SetOf<CommandConstraint>();
        }
        #endregion

        #region hide
        public void Hide()
        {
            _hidden = true;
        }
        #endregion

        #region is valid command name char
        public static bool IsValidCommandNameChar(char c)
        {
            return (char.IsLetter(c) || char.IsDigit(c) || c == '.' || c == '-');
        }
        #endregion

        #region add option of T
        public void AddOption<T>(string key, string help, (string terse, string verbose) flags)
        {
            this.AddOption<T>(
                key: key,
                help: help,
                converter: OptionTypeMap.ParseOptionArgument<T>,
                flags: flags
            );
        }

        public void AddOption<T>(string key, T defaultArg, string help, (string terse, string verbose) flags)
        {
            this.AddOption<T>(
                key: key, 
                defaultArg: defaultArg, 
                help: help, 
                converter: OptionTypeMap.ParseOptionArgument<T>, 
                flags: flags
            );
        }

        public void AddOption<T>(string key, string help, Func<string, T> converter, (string terse, string verbose) flags)
        {
            var op = new OptionDefinition<T>(
                key: key,
                help: help,
                converter: converter,
                flags.terse, flags.verbose
            );
            this.Options.Add(op);
        }

        public void AddOption<T>(string key, T defaultArg, string help, Func<string, T> converter, (string terse, string verbose) flags)
        {
            var op = new OptionDefinition<T>(
                key: key, 
                defaultArg: defaultArg, 
                help: help, 
                converter: converter, 
                flags.terse, flags.verbose
            );
            this.Options.Add(op);
        }
        #endregion

        #region option exists
        internal bool OptionExists(string key)
        {
            bool exists = _options.Exists((o) => string.Compare(o.Key, key, false) == 0);
            return exists;
        }
        #endregion

        #region must assign one of
        public void MustAssignOneOf(params string[] optionKeys)
        {
            if (optionKeys is null)
                throw new ArgumentNullException(nameof(optionKeys));

            if (optionKeys.Length < 2)
                throw new ArgumentException("Argument must contain at least 2 values.", nameof(optionKeys));

            var opDefKeys = new (string key, string flag)[optionKeys.Length];
            for (int i = 0; i < optionKeys.Length; i++)
            {
                var opDef = this[optionKeys[i]];

                if (opDef.MustAssign)
                    throw new CommandDefinitionException($"Option '{opDef.Key}' is marked '{nameof(OptionDefinition.MustAssign)}'...'{MustAssignOneOfConstraint.ConstraintName}' constraint cannot be applied.");

                opDefKeys[i] = (optionKeys[i], opDef.MostVerboseFlag);
            }

            var constraint = new MustAssignOneOfConstraint(opDefKeys);

            this.Constraints.Add(constraint);
        }
        #endregion

        #region mutually exclusive set
        public void MutuallyExclusiveSet(params string[] optionKeys)
        {
            if (optionKeys is null)
                throw new ArgumentNullException(nameof(optionKeys));

            if (optionKeys.Length < 2)
                throw new ArgumentException("Argument must contain at least 2 values.", nameof(optionKeys));

            var opDefKeys = new (string key, string flag)[optionKeys.Length];
            for (int i = 0; i < optionKeys.Length; i++)
            {
                var opDef = this[optionKeys[i]];

                if (opDef.MustAssign)
                    throw new CommandDefinitionException($"Option '{opDef.Key}' is marked '{nameof(OptionDefinition.MustAssign)}'...'{MutuallyExclusiveSetConstraint.ConstraintName}' constraint cannot be applied.");

                opDefKeys[i] = (optionKeys[i], opDef.MostVerboseFlag);
            }

            var constraint = new MutuallyExclusiveSetConstraint(opDefKeys);

            this.Constraints.Add(constraint);
        }
        #endregion

        #region apply constraint
        public void ApplyConstraint(Func<IConstrainedCommand, bool> constraint, string name, string description)
        {
            if (constraint is null)
                throw new ArgumentNullException(nameof(constraint));

            if (name is null)
                throw new ArgumentNullException(nameof(name));

            if (description is null)
                throw new ArgumentNullException(nameof(description));

            if (name == string.Empty)
                throw new ArgumentException("Argument must contain a value.", nameof(name));

            if (description == string.Empty)
                throw new ArgumentException("Argument must contain a value.", nameof(description));

            var customConstraint = new CommandConstraint(constraint, name, description);

            this.Constraints.Add(customConstraint);
        }
        #endregion

        #region ensure constraints
        internal void EnsureConstraints(Command command, ref SetOf<string> feedback)
        {
            if (_constraints is null || _constraints.Length == 0)
                return;

            for(int i = 0; i < _constraints.Length; i++)
            {
                var c = _constraints[i];
                if (!c.Ensure(command, out string fb))
                    feedback.Add(fb);
            }
        }
        #endregion

        #region register mapped handler validator
        internal void RegisterMappedHandlerValidator(Action validator)
        {
            _mappedHanderValidators += validator;
        }
        #endregion

        #region validate
        internal virtual void Validate()
        {
            if (_name == string.Empty)
                throw new CommandDefinitionException($"{nameof(CommandDefinition)} must be provided a value for {nameof(Name)}.");

            if (_name[0] == '-')
                throw new CommandDefinitionException($"{nameof(CommandDefinition)} {nameof(Name)} cannot start with a '-'.");

            if (_name.Length > CommandDefinition.MaxNameLength)
                throw new CommandDefinitionException($"{nameof(CommandDefinition)}.{nameof(Name)}...max accepted char length is {CommandDefinition.MaxNameLength}.");

            if (_handler is null && _asyncHandler is null)
                throw new CommandDefinitionException($"{nameof(CommandDefinition)} must be provided a value for one of: {nameof(Handler)}, {nameof(AsyncHandler)}.");

            int depth = 0;
            for (int i = 1; i < _name.Length; i++)
            {
                char c = _name[i];
                if (!CommandDefinition.IsValidCommandNameChar(c))
                    throw new CommandDefinitionException($"{nameof(CommandDefinition)}.{nameof(Name)} can only contain letters, digits, '-' and '.'");

                if (c == '.')
                    depth += 1;
            }

            if (_name[^1] == '.')
                throw new CommandDefinitionException($"{nameof(CommandDefinition)} '{nameof(Name)}' cannot end with '.'");

            _depth = depth;

            _mappedHanderValidators?.Invoke();
        }
        #endregion

        #region ensure command
        internal void EnsureCommand(Command command)
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
            for (int i = 0; i < this.Options.Length; i++)
            {
                var opDef = this.Options[i];
                var op = cmd.GetOptionByFlag(opDef.Flags);

                if (op is null)
                {
                    //empty, just need an empty shell with correct key
                    op = opDef.EmptyInstance();
                    cmd.AddEmptyOption(op as EmptyOption);
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
            if (cmd.Options.Length > 0 && !this.HasOptions)
            {
                feedback.Add($"The '{this.Name}' command does not accept any options...provided options are invalid.");
                return;
            }

            //if any options are defined for the command, confirm each option provided is valid
            for (int i = 0; i < cmd.Options.Length; i++)
            {
                var op = cmd.Options[i];

                //empty ops can always be assumed to be valid...because they were injected not provided
                if (op is EmptyOption)
                    continue;

                if (!this.Options.Exists(o => o.Flags.Contains(op.Flag)))
                    feedback.Add($"Undefined option at position: {i + 1} ... option: {op.Flag}");
            }
        }
        #endregion

        #region ensure no duplicate options
        private void EnsureNoDuplicateOptions(Command cmd, ref SetOf<string> feedback)
        {
            //TODO: refactor, this looks fundamentally wrong
            for (int i = 0; i < this.Options.Length; i++)
            {
                var opDef = this.Options[i];
                var opUno = cmd.GetOptionByFlag(opDef.Flags);
                for (int j = 0; j < cmd.Options.Length; j++)
                {
                    var opDos = cmd.Options[j];

                    if (opDos == opUno)
                        continue;

                    if (opDef.Flags.Contains(opDos.Flag))
                        feedback.Add($"Duplicate options provided at positions: {i + 1} and {j + 1}...'{opUno.Flag}' and '{opDos.Flag}'");
                }
            }
        }
        #endregion

        #region ensure option constraints
        private void EnsureOptionConstraints(Command cmd, ref SetOf<string> feedback)
        {
            for (int i = 0; i < this.Options.Length; i++)
            {
                OptionDefinition opDef = this.Options[i];
                ref Option op = ref cmd.GetOptionByRef(opDef.Key);

                //If empty op and a default constraint exists, empty op will be swapped for a default...hence the ref param
                opDef.EnsureConstraints(ref op, ref feedback);
            }
        }
        #endregion
    }
}
