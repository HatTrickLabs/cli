using System;
using System.Collections.Generic;

namespace HatTrick.CommandLine.Parsing
{
    public static class Tokenizer
    {
        #region tokenize
        public static string[] Tokenize(string input, bool keepLiteralQuotes = false)
        {
            //no sense in holding a tokenizer for multi-generations of GC...just use and release.
            var instance = new Instance(input, keepLiteralQuotes);
            return instance.Tokenize();
        }
        #endregion

        #region instance [class]
        private sealed class Instance
        {
            #region internals
            private readonly char _etx = '\x3'; //end of text
            private string _src;
            private bool _keepLitQuotes;
            private int _srcLen;
            private int _idx;
            #endregion

            #region constructors
            public Instance(string input, bool keepLiteralQuotes = false)
            {
                _src = input ?? throw new ArgumentNullException(nameof(input));
                _keepLitQuotes = keepLiteralQuotes;
                _srcLen = _src.Length;
                _idx = 0;
            }
            #endregion

            #region peek
            private char Peek()
            {
                char c = _srcLen > _idx ? _src[_idx] : _etx;

                return c;
            }
            #endregion

            #region read
            private char Read()
            {
                char c = _srcLen > _idx ? _src[_idx++] : _etx;

                return c;
            }
            #endregion

            #region tokenize
            public string[] Tokenize()
            {
                if (_srcLen == 0)
                    return new string[0];

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

                Span<char> token = stackalloc char[_srcLen];
                List<string> args = new List<string>(8);

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

                return args.ToArray();
            }
            #endregion
        }
        #endregion
    }
}
