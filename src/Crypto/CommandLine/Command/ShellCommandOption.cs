using System;
using System.Collections.Generic;

namespace Crypto.CommandLine
{
    public class ShellCommandOption
    {
        #region internals
        private string _key;
        private string _flag;
        private string _arg;
        private object _value;
        #endregion

        #region interface
        public string Key => _key;
        public string Flag => _flag;
        public string Argument => _arg;
        public dynamic Value => _value;
        #endregion

        #region constructors
        public ShellCommandOption(string flag)
        {
            _flag = flag;
        }

        public ShellCommandOption(string flag, string argument)
        {
            _flag = flag;
            _arg = argument;
        }

        public ShellCommandOption(string key, string flag, string argument)
        {
            _key = key;
            _flag = flag;
            _arg = argument;
        }
        #endregion

        #region parse
        public static void Parse(ReadOnlySpan<string> arguments, out ShellCommandOption[] options)
        {
            var ops = new List<ShellCommandOption>();

            Func<string, bool> isExplicitAssign = (a) => a == "=" || a == ":";

            string prev = null;
            for (int i = 0; i < arguments.Length; i++)
            {
                string arg = arguments[i];

                if (!isExplicitAssign(arg))
                {
                    //starts with '-' then must be option unless prev arg is an explicit assignment char
                    if (arg[0] == '-' && !isExplicitAssign(prev))
                    {
                        if (arg.Length == 1)
                            throw new CommandInputException($"Invalid command line argument provided: '{arg}' at position:{i + 1}");

                        if (arg[1] == '-') //verbose option
                        {
                            ops.Add(new ShellCommandOption(arg));
                        }
                        else if (arg.Length == 2) //single terse option
                        {
                            ops.Add(new ShellCommandOption(arg));
                        }
                        else //must be a compound terse option, split into individual flags
                        {
                            for (int j = 1; j < arg.Length; j++) //start at idx 1 to skip the '-'
                            {
                                var flag = "-" + arg[j];
                                ops.Add(new ShellCommandOption(flag));
                            }
                        }
                    }
                    else //no '-' and prev not an explicit assign, must be an option argument
                    {
                        ops[^1].SetArgument(arg);//set the last option in the list with the current arg
                    }
                }
                prev = arg;
            }
            options = ops.ToArray();
        }
        #endregion

        #region set key
        public void SetKey(string key)
        {
            _key = key;
        }
        #endregion

        #region set arg
        internal void SetArgument(string argument)
        {
            _arg = argument;
        }
        #endregion

        #region set value
        internal void SetValue<T>(T value)
        {
            _value = value;
        }
        #endregion

        #region get value
        public T GetValue<T>()
        {
            return (T)_value;
        }
        #endregion
    }
}
