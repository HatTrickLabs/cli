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
                            ops.Add(this.EnsureVerboseFlag(token));

                        else if (token.Length == 2) //single terse option flag
                            ops.Add(new Option(token));

                        else //must be a compound terse option flag, unroll into individual flags
                            UnrollCompoundFlag(token, ref ops);
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

            #region ensure verbose flag
            private Option EnsureVerboseFlag(string token)
            {
                //we are checking for embedded explicit assign ie: --format=N or --format:N
                int index = 2;//start at 2 to skip the leading --
                const char eq = '=';
                const char col = ':';

                do
                {
                    char c = token[index];
                    if (c == eq || c == col)
                    {
                        string flag = new string(token.AsSpan(0, index));
                        string arg = new string(token.AsSpan(++index));
                        Option op = new Option(flag);
                        op.SetArgument(arg);
                        return op;
                    }

                } while (++index < token.Length);

                return new Option(token);
            }
            #endregion

            #region unroll compound flag
            private void UnrollCompoundFlag(string token, ref SetOf<Option> ops)
            {
                //we are unrolling compound terse flags with the last flag optinally having an exlicit arg assign
                //ie:  -fq is two flags -f and -q
                //ie:  -fql=debug is 3 flags -f -q -l with the arg of -l being debug
                int index = 1;//start at 1 to skip the leading -
                const char eq = '=';
                const char col = ':';

                do
                {
                    char c = token[index];
                    if (c == eq || c == col)
                    {
                        string arg = new string(token.AsSpan(++index));
                        ops[^1].SetArgument(arg);
                        break;
                    }
                    else
                    {
                        ops.Add(new Option("-" + token[index]));
                    }

                } while (++index < token.Length);
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
