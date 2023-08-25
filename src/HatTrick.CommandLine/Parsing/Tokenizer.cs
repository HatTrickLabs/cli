using System;
using System.Collections.Generic;
using System.IO;

namespace HatTrick.CommandLine.Parsing
{
    public static class Tokenizer
    {
        #region tokenize
        public static string[] Tokenize(string input, bool keepLiteralQuotes = false)
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));

            //no sense in holding a tokenizer for multi-generations of GC...just use and release.
            var instance = new Instance(input, keepLiteralQuotes);

            return instance.Tokenize();
        }
        #endregion

        #region instance [class]
        private sealed class Instance
        {
            #region internals
            private static readonly char _etx;
            private static readonly int _maxSrcLength;
            private static readonly string[] _empty;
            private string _src;
            private bool _keepLitQuotes;
            private int _srcLength;
            private int _index;
            #endregion

            #region constructors
            static Instance()
            {
                _etx = '\x3';//end of text
                _maxSrcLength = 2_048;//0x800
                _empty = Array.Empty<string>();
            }

            internal Instance(string input, bool keepLiteralQuotes = false)
            {
                if (input.Length > _maxSrcLength)
                    throw new InternalBufferOverflowException($"{nameof(Tokenizer)} has a maximum internal buffer length for {nameof(input)} of {_maxSrcLength}.");

                if (string.IsNullOrWhiteSpace(input))
                {
                    _src = string.Empty;
                    _srcLength = 0;
                }
                else
                {
                    _src = input;
                    _srcLength = input.Length;
                }

                _keepLitQuotes = keepLiteralQuotes;
                _index = 0;
            }
            #endregion

            #region peek
            private char Peek()
            {
                char c = _srcLength > _index ? _src[_index] : _etx;

                return c;
            }
            #endregion

            #region read
            private char Read()
            {
                char c = _srcLength > _index ? _src[_index++] : _etx;

                return c;
            }
            #endregion

            #region tokenize
            public string[] Tokenize()
            {
                if (_srcLength == 0)
                    return _empty;

                char etx = _etx;
                bool keepLitQuotes = _keepLitQuotes;
                char dblQuote = '\"';
                char space = ' ';
                char tab = '\t';
                char nl = '\n';
                char cr = '\r';
                char escape = '\\';
                char previous = '\0';

                Func<char, bool> isWhitespace = (c) => c == space || c == tab || c == nl || c == cr;

                bool inDblQuote = false;

                Span<char> token = stackalloc char[_srcLength];
                SetOf<string> args = new SetOf<string>();

                int at = 0;
                char c;
                while ((c = Read()) != etx)
                {
                    if (c == dblQuote && previous != escape)
                    {
                        inDblQuote = !inDblQuote;
                        if (keepLitQuotes)
                            token[at++] = c;
                    }
                    else if (isWhitespace(c) && !inDblQuote)
                    {
                        if (at > 0)
                        {
                            args.Add(new string(token.Slice(0, at)));
                            at = 0;
                        }
                    }
                    else if (c == escape)
                    {
                        if (Peek() != '"')
                        {
                            token[at++] = c;
                        }
                    }
                    else
                    {
                        token[at++] = c;
                    }
                    previous = c;
                }

                if (at > 0)
                    args.Add(new string(token.Slice(0, at)));

                return (string[])args;
            }
            #endregion
        }
        #endregion
    }
}
