using System;

namespace HatTrick.CommandLine
{
    public static class Tokenizer
    {
        #region tokenize
        public static Token[] Tokenize(string[] args)
        {
            if (args is null)
                throw new ArgumentNullException(nameof(args));

            if (args.Length == 0)
                return Array.Empty<Token>();

            var instance = new Instance(args);

            SetOf<Token> tokens = instance.Tokenize();

            return tokens.ToArray();
        }
        #endregion

        #region instance [class]
        private sealed class Instance
        {
            #region internals
            private string[] _args;
            private int _argsLength;
            private int _index;
            #endregion

            #region constructors
            internal Instance(string[] args)
            {
                _args = args;
                _argsLength = args.Length;
            }
            #endregion

            #region peek
            private string Peek()
            {
                string arg = _argsLength > _index ? _args[_index] : null;

                return arg;
            }
            #endregion

            #region read
            private bool Read(out string arg)
            {
                if (_argsLength > _index)
                {
                    arg = _args[_index++];
                    return true;
                }
                arg = null;
                return false;
            }
            #endregion

            #region tokenize
            internal SetOf<Token> Tokenize()
            {
                var tokens = new SetOf<Token>();
                Token prev = null;

                while (this.Read(out string arg))
                {
                    if (arg.Length == 1 && (arg[0] == '=' || arg[0] == ':'))//must be explicit assign
                    {
                        tokens.Add(new ExplicitAssignToken(arg));
                    }
                    else if (arg[0] != '-')//def not a flag
                    {
                        if (tokens.Length == 0)//arg[0] is not a - and no other tokens, must assume arg is the command
                            tokens.Add(new CommandToken(arg));

                        else//already have a token in the set, arg must be an op argument
                            tokens.Add(new ArgumentToken(arg));
                    }
                    else if (arg.Length < 2)
                    {
                        throw new CommandInputException(this.ExceptionMessageHelper(arg));
                    }
                    else if (prev is ExplicitAssignToken)//must be arg
                    {
                        tokens.Add(new ArgumentToken(arg));
                    }
                    else if (arg[0] == '-')//must be a flag
                    {
                        if (arg.Length == 2)//must be single terse a flag
                        {
                            tokens.Add(new TerseFlagToken(arg));
                        }
                        else if (arg[1] == '-')//must be verbose flag
                        {
                            if (arg.Length == 3) //not verbose if only 3 chars.... --x NOPE
                                throw new CommandInputException(this.ExceptionMessageHelper(arg));

                            bool complete = false;
                            for (int i = 2; i < arg.Length; i++)
                            {
                                char c = arg[i];
                                if (c == '=' || c == ':')//explicit assign char
                                {
                                    string left = new string(arg.AsSpan(0, i));
                                    tokens.Add(new VerboseFlagToken(left));
                                    string assign = new string(arg[i], 1);
                                    tokens.Add(new ExplicitAssignToken(assign));
                                    string right = new string(arg.AsSpan(++i));
                                    tokens.Add(new ArgumentToken(right));
                                    complete = true;
                                }
                            }

                            if (!complete)
                                tokens.Add(new VerboseFlagToken(arg));
                        }
                        else
                        {
                            //unroll compound terse flags
                            for (int i = 1; i < arg.Length; i++)
                            {
                                if (arg[i] == '=' || arg[i] == ':')
                                {
                                    string assign = new string(arg[i], 1);
                                    tokens.Add(new ExplicitAssignToken(assign));
                                    if (++i < arg.Length)
                                    {
                                        string right = new string(arg.AsSpan(i));
                                        tokens.Add(new ArgumentToken(right));
                                    }
                                    break;
                                }
                                else
                                {
                                    string flag = "-" + arg[i];
                                    tokens.Add(new TerseFlagToken(flag));
                                }
                            }
                        }
                    }

                    prev = tokens[^1];
                }

                return tokens;
            }
            #endregion

            #region exception message helper
            private string ExceptionMessageHelper(string token)
            {
                return $"Invalid command line argument provided: '{token}' at postition: {_index}";
            }
            #endregion
        }
        #endregion
    }
}
