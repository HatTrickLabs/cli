using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace Crypto.CommandLine
{
    public class Command
    {
        #region internals
        private readonly string _key;
        private readonly CommandOption[] _ops;
        #endregion

        #region interface
        public string Key => _key;

        internal CommandOption[] Options => _ops;

        public CommandOption this[string key]
        {
            get
            {
                var op = Array.Find(_ops, (o) => o.Key == key);
                return op;
            }
        }
        #endregion

        #region constructors
        public Command(string key, CommandOption[] options = null)
        {
            if (key is null)
                throw new ArgumentNullException(nameof(key));

            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException($"{nameof(key)} argument must contain a value.");

            _key = key;
            _ops = options ?? new CommandOption[0];
        }
        #endregion

        #region [static] parse
        public static Command Parse(string[] args)
        {
            if (args is null)
                throw new ArgumentNullException(nameof(args));

            if (args.Length == 0)
                return new DefaultCommand();//optionless default command...


            Command cmd = null;
            if (args[0][0] != '-') //no command, return the default...
            {
                CommandOption.Parse(new ReadOnlySpan<string>(args, 0, args.Length), out CommandOption[] options);
                cmd = new DefaultCommand(options);
            }
            else
            {
                CommandOption.Parse(new ReadOnlySpan<string>(args, 1, args.Length - 1), out CommandOption[] options);
                cmd = new Command(args[0], options);
            }

            return cmd;
        }
        #endregion

        #region get options
        public CommandOption[] GetOptions(Predicate<CommandOption> where = null)
        {
            return Array.FindAll(_ops, where == null ? (o) => true : where);
        }
        #endregion

        #region get option
        public CommandOption GetOption(string optionKey)
        {
            CommandOption op = Array.Find(this.Options, (o) => o.Key == optionKey);
            return op;
        }
        #endregion
    }

    public class DefaultCommand : Command
    {
        internal DefaultCommand(CommandOption[] options = null) : base(CommandDefinition.DefaultCommandName, options)
        {

        }
    }
}
