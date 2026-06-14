// SPDX-License-Identifier: Apache-2.0
// Copyright (c) HatTrick Labs, LLC

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
                bool flagFound = false;
                Token tkn = null;
                while (this.Read(out string arg))
                {
                    var left = tokens.Length > 0 ? tokens[^1] : null;
                    if (left is ExplicitAssignToken)
                    {
                        //must be op arg
                        tkn = new ArgumentToken(arg);
                    }
                    else if (ExplicitAssignToken.IsValid(arg))
                    {
                        //var left = tokens.Length > 0 ? tokens[^1] : null;
                        //if left is arg...because underlying framework strips dbl quotes, we assume it was originally quoted
                        if (left is ArgumentToken a)
                            a.Merge(arg);

                        else
                            tkn = new ExplicitAssignToken(arg);
                    }
                    else if (tokens.Length == 0 && CommandToken.IsValid(arg))//command must be first
                        tkn = new CommandToken(arg);

                    else if (TerseFlagToken.IsValid(arg))
                        tkn = new TerseFlagToken(arg);

                    else if (CompoundTerseFlagToken.IsValid(arg))
                        tkn = new CompoundTerseFlagToken(arg);

                    else if (VerboseFlagToken.IsValid(arg))
                        tkn = new VerboseFlagToken(arg);

                    else if (ArgumentToken.IsValid(arg))
                    {
                        //var left = tokens.Length > 0 ?  tokens[^1] : null;
                        //if left is flag or explicit assign, this is the arg
                        if (left is FlagToken/* || left is ExplicitAssignToken*/)
                            tkn = new ArgumentToken(arg);

                        //if left is arg token that has been merged, logical that this is just another part of the previous arg.
                        else if (left is ArgumentToken a && a.IsMerged)
                            a.Merge(arg);

                        //this is an unflagged argument
                        else if (!flagFound && left is CommandToken || left is ArgumentToken)
                            tkn = new ArgumentToken(arg);

                        else
                            throw new CommandInputException(this.ExceptionMessageHelper(arg));
                    }

                    else
                        throw new CommandInputException(this.ExceptionMessageHelper(arg));

                    if (tkn is FlagToken)
                        flagFound = true;

                    if (tkn is not null)
                        tokens.Add(tkn);

                    tkn = null;
                }

                return tokens;
            }
            #endregion

            #region exception message helper
            private string ExceptionMessageHelper(string token)
            {
                return $"Invalid command line argument provided...unexpected token '{token}' at postition {_index}";
            }
            #endregion
        }
        #endregion
    }
}
