using System;

namespace HatTrick.CommandLine
{
    internal static class Tokenizer
    {
        #region tokenize
        internal static Token[] Tokenize(string[] args)
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
                Token tkn = null;
                while (this.Read(out string arg))
                {
                    if (ExplicitAssignToken.IsValid(arg))
                        tkn = new ExplicitAssignToken(arg);

                    else if (tokens.Length == 0 && CommandToken.IsValid(arg))//command must be first
                        tkn = new CommandToken(arg);

                    else if (TerseFlagToken.IsValid(arg))
                        tkn = new TerseFlagToken(arg);

                    else if (CompoundTerseFlagToken.IsValid(arg))
                        tkn = new CompoundTerseFlagToken(arg);

                    else if (VerboseFlagToken.IsValid(arg))
                        tkn = new VerboseFlagToken(arg);

                    else if (ArgumentToken.IsValid(arg))
                        tkn = new ArgumentToken(arg);

                    else
                        throw new CommandInputException(this.ExceptionMessageHelper(arg));

                    tokens.Add(tkn);
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
