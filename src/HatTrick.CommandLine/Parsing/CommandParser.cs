using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace HatTrick.CommandLine
{
    public static class CommandParser
    {
        #region parse
        public static Command Parse(string[] tokens)
        {
            if (tokens is null)
                throw new ArgumentNullException(nameof(tokens));

            //no sense in holding a parser for multi-generations of GC...just use and release.
            var instance = new Instance(tokens);

            return instance.ParseCommand();
        }
        #endregion

        #region instance [class]
        private sealed class Instance
        {
            #region internals
            private string[] _tokens;
            #endregion

            #region constructors
            internal Instance(string[] tokens)
            {
                _tokens = tokens ?? throw new ArgumentNullException(nameof(tokens));
            }
            #endregion

            #region parse command
            internal Command ParseCommand()
            {
                string[] tokens = _tokens;

                if (tokens.Length == 0)//optionless default command...
                    return new Command(CommandDefinition.DefaultCommandName);

                bool isDefault = tokens[0][0] == '-';//no command, just jumps right into option flags

                int startAt = isDefault ? 0 : 1;
                var opTokens = new ReadOnlySpan<string>(tokens, startAt, tokens.Length - startAt);

                SetOf<Option> options = this.ParseCommandOptions(opTokens);

                string name = isDefault ? CommandDefinition.DefaultCommandName : tokens[0];
                return new Command(name, options);
            }
            #endregion

            #region parse command options
            private SetOf<Option> ParseCommandOptions(ReadOnlySpan<string> tokens)
            {
                var ops = new SetOf<Option>();

                var isExplicitAssign = (string tkn) => tkn == "=" || tkn == ":";

                string prev = null;
                for (int i = 0; i < tokens.Length; i++)
                {
                    string token = tokens[i];

                    if (isExplicitAssign(token))
                    {
                        if (i == 0)
                            throw new CommandInputException(this.ExceptionMessageHelper(token, i));

                        prev = token;
                        continue;
                    }

                    //starts with '-' then must be option flag unless prev arg is an explicit assignment char
                    if (token[0] == '-' && !isExplicitAssign(prev))
                    {
                        if (token.Length == 1)
                            throw new CommandInputException(this.ExceptionMessageHelper(token, i));

                        if (token[1] == '-') //verbose option flag
                            ops.Add(new Option(token));

                        else if (token.Length == 2) //single terse option flag
                            ops.Add(new Option(token));

                        else //must be a compound terse option flag, unroll into individual flags
                            UnrollCompoundFlag(token, (f) => ops.Add(f));
                    }
                    else //no '-' and prev not an explicit assign, must be an option argument
                    {
                        if (ops.Length == 0)
                            throw new CommandInputException($"{this.ExceptionMessageHelper(token, i)}...positional arguments not supported.");

                        Option op = ops[^1];
                        if (op.HasArgument)
                            throw new CommandInputException($"{this.ExceptionMessageHelper(token, i)}...multi value arguments not supported.");

                        op.SetArgument(token);//set current arg to the last op in the set
                    }

                    prev = token;
                }

                return ops;
            }
            #endregion

            #region unroll compound flag
            private void UnrollCompoundFlag(string flag, Action<Option> onFlagUnrolled)
            {
                //start at idx 1 to skip the '-'
                for (int i = 1; i < flag.Length; i++)
                {
                    var op = new Option(flag: "-" + flag[i]);
                    onFlagUnrolled(op);
                }
            }
            #endregion

            #region exception message helper
            private string ExceptionMessageHelper(string token, int index)
            {
                return $"Invalid command line argument provided: '{token}' at postition: {index + 1}";
            }
            #endregion
        }
        #endregion
    }
}
