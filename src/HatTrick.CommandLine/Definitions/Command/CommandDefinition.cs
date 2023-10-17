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
                (flags.terse, flags.verbose)
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
                (flags.terse, flags.verbose)
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

        #region get options
        public OptionDefinition[] GetOptions(bool includeHidden = false)
        {
            return includeHidden ? _options.ToArray() : _options.FindAll(o => !o.Hidden);
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

                opDefKeys[i] = (optionKeys[i], opDef.Flags.verbose);
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

                opDefKeys[i] = (optionKeys[i], opDef.Flags.verbose);
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
    }
}
