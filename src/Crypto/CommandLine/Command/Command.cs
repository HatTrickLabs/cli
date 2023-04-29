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
            _ops = options;
        }
        #endregion

        #region [static] parse
        public static Command Parse(string[] args)
        {
            if (args is null)
                throw new ArgumentNullException(nameof(args));

            if (args.Length == 0)//TODO: Need a default command for the exe ... maybe help
                throw new ArgumentException("Provided args[] cannot be empty.", nameof(args));

            if (args[0].StartsWith('-'))
                throw new CommandInputException("First argument starts with a '-' ... Input seems to be missing the command.");

            string command = args[0];

            var ops = new ReadOnlySpan<string>(args, 1, args.Length - 1);
            CommandOption.Parse(ops, out CommandOption[] options);

            var shellCmd = new Command(command, options);

            return shellCmd;
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
}
