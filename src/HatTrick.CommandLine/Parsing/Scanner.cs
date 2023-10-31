using System;
using System.Collections.Generic;
using System.IO;

namespace HatTrick.CommandLine
{
    public static class Scanner
    {
        #region scan
        public static string[] Scan(string input, bool keepLiteralQuotes = false)
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));

            //no sense in holding a scanner for multi-generations of GC...just use and release.
            var instance = new Instance(input, keepLiteralQuotes);

            return instance.Scan();
        }
        #endregion

        #region instance [class]
        private sealed class Instance
        {
            #region internals
            private static readonly char _null;
            private static readonly int _maxSrcLength;
            private static readonly int _maxStackallocLength;
            private static readonly string[] _empty;
            private string _src;
            private bool _keepLitQuotes;
            private int _srcLength;
            private int _index;
            #endregion

            #region constructors
            static Instance()
            {
                _null = '\0';//null char
                _maxSrcLength = 2_048;//0x800
                _maxStackallocLength = 1_024;//0x400
                _empty = Array.Empty<string>();
            }

            internal Instance(string input, bool keepLiteralQuotes = false)
            {
                if (input.Length > _maxSrcLength)
                    throw new RangeOverflowException($"{nameof(Scanner)} has a maximum internal buffer length for {nameof(input)} of {_maxSrcLength}.");

                _src = input;
                _srcLength = input.Length;

                _keepLitQuotes = keepLiteralQuotes;
                _index = 0;
            }
            #endregion

            #region peek
            private char Peek()
            {
                char c = _srcLength > _index ? _src[_index] : _null;

                return c;
            }
            #endregion

            #region read
            private bool Read(out char c)
            {
                if (_srcLength > _index)
                {
                    c = _src[_index++];
                    return true;
                }
                c = _null;
                return false;
            }
            #endregion

            #region scan
            public string[] Scan()
            {
                if (_srcLength == 0)
                    return _empty;

                bool keepLitQuotes = _keepLitQuotes;
                const char dblQuote = '\"';
                const char space = ' ';
                const char tab = '\t';
                const char nl = '\n';
                const char cr = '\r';
                const char escape = '\\';
                char previous = '\0';

                Func<char, bool> isWhitespace = (c) => c == space || c == tab || c == nl || c == cr;

                bool inDblQuote = false;

                Span<char> argument = _srcLength > _maxStackallocLength ? new char[_srcLength] : stackalloc char[_srcLength];
                SetOf<string> arguments = new SetOf<string>();

                int at = 0;
                char c;
                while (Read(out c))
                {
                    if (c == dblQuote && previous != escape)
                    {
                        inDblQuote = !inDblQuote;
                        if (keepLitQuotes)
                        {
                            argument[at++] = c;
                        }
                        if (at > 1)//the 1 could be the open double quote we just added in 'keepLitQuotes'
                        {
                            arguments.Add(new string(argument.Slice(0, at)));
                            at = 0;
                        }
                    }
                    else if (isWhitespace(c) && !inDblQuote)
                    {
                        if (at > 0)
                        {
                            arguments.Add(new string(argument.Slice(0, at)));
                            at = 0;
                        }
                    }
                    else if (c == escape)
                    {
                        if (Peek() != '"')
                        {
                            argument[at++] = c;
                        }
                    }
                    else
                    {
                        argument[at++] = c;
                    }
                    previous = c;
                }

                if (at > 0)
                {
                    arguments.Add(new string(argument.Slice(0, at)));
                }

                return arguments.ToArray();
            }
            #endregion
        }
        #endregion
    }
}
