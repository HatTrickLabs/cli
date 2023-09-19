using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.ComponentModel.Design;

namespace HatTrick.CommandLine
{
    public class CommandDefinition
    {
        #region const
        public const int MaxNameLength = 40;
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
        private SetOf<CommandOptionDefinition> _options;
        private SetOf<CommandConstraint> _constraints;
        #endregion

        #region interface
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

        public SetOf<CommandOptionDefinition> Options => _options;

        public CommandOptionDefinition this[string key]
        {
            get
            {
                var op = _options.Find(o => o.Key == key);

                if (op == default)
                    throw new KeyNotFoundException($"Provided option key: '{key}' not found.");

                return op;
            }
        }

        public SetOf<CommandConstraint> Constraints
        {
            get => _constraints;
            set => _constraints = value;
        }

        public bool HasConstraints => _constraints.Length > 0;
        #endregion

        #region constructors
        public CommandDefinition(string name)
        {
            _name = name;
            _options = new SetOf<CommandOptionDefinition>();
            _constraints = new SetOf<CommandConstraint>();
        }
        #endregion

        #region hide
        public void Hide()
        {
            _hidden = true;
        }
        #endregion

        #region add option of T
        public void AddOption<T>(string key, string help, (string terse, string verbose) flags)
        {
            var op = new CommandOptionDefinition<T>(
                key: key, 
                help: help, 
                converter: OptionTypeMap.ParseOptionArgument<T>,
                flags.terse, flags.verbose
            );
            this.Options.Add(op);
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
            var op = new CommandOptionDefinition<T>(
                key: key,
                help: help,
                converter: converter,
                flags.terse, flags.verbose
            );
            this.Options.Add(op);
        }

        public void AddOption<T>(string key, T defaultArg, string help, Func<string, T> converter, (string terse, string verbose) flags)
        {
            var op = new CommandOptionDefinition<T>(
                key: key, 
                defaultArg: defaultArg, 
                help: help, 
                converter: converter, 
                flags.terse, flags.verbose
            );
            this.Options.Add(op);
        }
        #endregion

        #region get option
        internal CommandOptionDefinition GetOption(string key)
        {
            var op = _options.Find(o => o.Key == key);
            return op;
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
                    throw new CommandDefinitionException($"Option '{opDef.Key}' is marked '{nameof(CommandOptionDefinition.MustAssign)}'...'{MustAssignOneOfConstraint.ConstraintName}' constraint cannot be applied.");

                opDefKeys[i] = (optionKeys[i], opDef.MostVerboseFlag);
            }

            var constraint = new MustAssignOneOfConstraint(opDefKeys);

            this.Constraints.Add(constraint);
        }
        #endregion

        #region mutually exclusive set
        public void MutaullyExclusiveSet(params string[] optionKeys)
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
                    throw new CommandDefinitionException($"Option '{opDef.Key}' is marked '{nameof(CommandOptionDefinition.MustAssign)}'...'{MutuallyExclusiveSetConstraint.ConstraintName}' constraint cannot be applied.");

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
            if (_name is null || _name == string.Empty)
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
                if (!(char.IsLetter(c) || char.IsDigit(c) || c == '.' || c == '-'))
                    throw new NamespaceDefinitionException($"{nameof(CommandDefinition)}.{nameof(Name)} can only contain letters, digits, '-' and '.'");

                if (c == '.')
                    depth += 1;
            }

            _depth = depth;

            this.ValidateOptions();

            _mappedHanderValidators?.Invoke();
        }
        #endregion

        #region validate options
        private void ValidateOptions()
        {
            int opLen = _options?.Length ?? 0;
            if (opLen > 0)
            {
                string key = null;
                string flag = null;
                var flagsMap = new string[opLen, 2];
                for (int i = 0; i < opLen; i++)
                {
                    var op = _options[i];
                    op.Validate();
                    key = op.Key;

                    for (int j = 0; j < i; j++)
                    {
                        if (op.Key == _options[j].Key)
                            throw new CommandDefinitionException($"Command option key collision command key '{op.Key}'.");

                        flag = op.Flags[0];
                        if (flagsMap[j, 0] == flag || flagsMap[j, 1] == flag)
                            throw new CommandDefinitionException($"Option flag collision in command options '{op.Key}' and '{_options[j].Key}' for flag '{flag}'.");

                        flag = op.Flags[1];
                        if (flagsMap[j, 0] == flag || flagsMap[j, 1] == flag)
                            throw new CommandDefinitionException($"Option flag collision in command options '{op.Key}' and '{_options[j].Key}' for flag '{flag}'.");
                    }

                    //Note: technically do not need x markers as op.Validate() ensures
                    //neither of the flags is null or empty...leave this logic just in case 
                    //I make the decision to allow only one valid flag and the other null/empty
                    //Note: x is not a valid flag and we wouldn't make it past op.Validate() if it 
                    //was attempted as a flag...the right side of the above conditions (ops.Flags[?])
                    //can never be x
                    flagsMap[i, 0] = string.IsNullOrEmpty(op.Flags[0]) ? "x" : op.Flags[0];
                    flagsMap[i, 1] = string.IsNullOrEmpty(op.Flags[1]) ? "x" : op.Flags[1];
                }
            }
        }
        #endregion
    }
}
