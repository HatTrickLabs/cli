using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace Crypto.CommandLine
{
    public class ShellCommand
    {
        #region internals
        private readonly string _cmd;
        private readonly ShellCommandOption[] _ops;
        #endregion

        #region interface
        public string Command => _cmd;

        internal ShellCommandOption[] Options => _ops;

        public ShellCommandOption this[string key]
        {
            get
            {
                var op = Array.Find(_ops, (o) => o.Key == key);
                return op;
            }
        }
        #endregion

        #region constructors
        public ShellCommand(string command, ShellCommandOption[] options = null)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            if (string.IsNullOrWhiteSpace(command))
                throw new ArgumentException($"{nameof(command)} argument must contain a value.");

            _cmd = command;
            _ops = options;
        }
        #endregion

        #region [static] parse
        public static ShellCommand Parse(string[] args)
        {
            if (args is null)
                throw new ArgumentNullException(nameof(args));

            if (args.Length == 0)//TODO: Need a default command for the exe ... maybe help
                throw new ArgumentException("Provided args[] cannot be empty.", nameof(args));

            if (args[0].StartsWith('-'))
                throw new CommandInputException("First argument starts with a '-' ... Input seems to be missing the command.");

            string command = args[0];

            var ops = new ReadOnlySpan<string>(args, 1, args.Length - 1);
            ShellCommandOption.Parse(ops, out ShellCommandOption[] options);

            var shellCmd = new ShellCommand(command, options);

            return shellCmd;
        }
        #endregion

        #region get options
        public ShellCommandOption[] GetOptions(Predicate<ShellCommandOption> where = null)
        {
            return Array.FindAll(_ops, where == null ? (o) => true : where);
        }
        #endregion

        #region get option
        public ShellCommandOption GetOption(string optionKey)
        {
            ShellCommandOption op = Array.Find(this.Options, (o) => o.Key == optionKey);
            return op;
        }
        #endregion
    }
}
