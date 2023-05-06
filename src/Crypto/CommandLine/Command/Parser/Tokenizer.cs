using System;
using System.Collections.Generic;

namespace Crypto.CommandLine
{
    public class Tokenizer
    {
        #region internals
        private readonly char _etx = '\x3'; //end of text
        private string _src;
        private int _srcLen;
        private int _idx;
        #endregion

        #region constructors
        public Tokenizer(string input)
        {
            _src = input ?? throw new ArgumentNullException(nameof(input));
            _srcLen = _src.Length;
            _idx = 0;
        }
        #endregion

        #region peek
        private char Peek()
        {
            char c = (_srcLen > _idx) ? _src[_idx] : _etx;

            return c;
        }
        #endregion

        #region read
        private char Read()
        {
            char c = (_srcLen > _idx) ? _src[_idx++] : _etx;

            return c;
        }
        #endregion

        #region tokenize
        public string[] Tokenize()
        {
            if (_srcLen == 0)
                return new string[0];

            char etx = _etx;
            char dblQuote = '\"';
            char space = ' ';
            char tab = '\t';
            char nl = '\n';
            char cr = '\r';
            char escape = '\\';
            char previous = '\0';

            Func<char, bool> isWhitespace = (char c) => (c == space || c == tab || c == nl || c == cr);

            bool inDblQuote = false;

            Span<char> token = stackalloc char[_srcLen];
            List<string> args = new List<string>(8);

            int at = 0;
            char c;
            while ((c = this.Read()) != etx)
            {
                if (c == dblQuote && previous != escape)
                {
                    inDblQuote = !inDblQuote;
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
                    if (this.Peek() != '"')
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
}
