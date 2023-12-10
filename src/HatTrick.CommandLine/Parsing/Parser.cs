using System;

namespace HatTrick.CommandLine
{
    internal static class Parser
    {
        #region parse
        internal static Command Parse(Token[] tokens)
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
            private Token[] _tokens;
            private int _tokensLength;
            private int _index;
            #endregion

            #region constructors
            internal Instance(Token[] tokens)
            {
                _tokens = tokens;
                _tokensLength = tokens.Length;
            }
            #endregion

            #region peek
            private Token Peek()
            {
                Token token = _tokensLength > _index ? _tokens[_index] : null;

                return token;
            }
            #endregion

            #region read
            private bool Read(out Token token)
            {
                if (_tokensLength > _index)
                {
                    token = _tokens[_index++];
                    return true;
                }
                token = null;
                return false;
            }
            #endregion

            #region parse command
            internal Command ParseCommand()
            {
                Token[] tokens = _tokens;

                if (tokens.Length == 0)//optionless default command...
                    return new Command(DefaultCommandDefinition.CommandName);

                CommandToken cmdToken = null;
                SetOf<Option> options = new SetOf<Option>();
                while(this.Read(out Token token))
                {
                    switch (token)
                    {
                        case CommandToken cmd:
                            cmdToken = cmd;
                            break;

                        case ExplicitAssignToken _:
                            if (options.Length == 0 || this.Peek() is not ArgumentToken || options[^1].HasArgument)
                                throw new CommandParseException(this.ExceptionHelper("Unexpected explicit assignment", token));
                            break;

                        case VerboseFlagToken _:
                            options.Add(new Option(token.Value));
                            break;

                        case TerseFlagToken _:
                            options.Add(new Option(token.Value));
                            break;

                        case CompoundTerseFlagToken compoundFlag:
                            foreach (TerseFlagToken flag in compoundFlag.Unroll())
                            {
                                options.Add(new Option(flag.Value));
                            }
                            break;

                        case ArgumentToken _:
                            if (options.Length == 0)
                            {
                                if (this.Peek() is not null)
                                    throw new CommandParseException(this.ExceptionHelper("Unexpected argument, positional arguments not supported", token));

                                options.Add(new UnflaggedOption(token.Value));
                            }
                            else
                            {
                                var op = options[^1];
                                if (op.HasArgument)
                                    throw new CommandParseException(this.ExceptionHelper("Unexpected argument, multi value arguments not supported", token));

                                op.SetArgument(token.Value);
                            }
                            break;

                        default:
                            throw new CommandParseException($"Encountered unknown token type: {token.GetType().Name}");
                    }
                }

                return new Command(cmdToken?.Value ?? DefaultCommandDefinition.CommandName, options.ToArray());
            }
            #endregion

            #region exception message helper
            private string ExceptionHelper(string message, Token token)
            {
                return $"{message}: '{token}' at postition: {_index}";
            }
            #endregion
        }
        #endregion
    }
}
