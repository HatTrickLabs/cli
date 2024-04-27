using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HatTrick.CommandLine
{
    public struct OptionFlags
    {
        #region internals
        private string _terse;
        private string _verbose;
        #endregion

        #region interface
        public string Terse => _terse;

        public string Verbose => _verbose;
        #endregion

        #region constructors
        public OptionFlags(string verbose) : this(null, verbose)
        { }

        public OptionFlags(string terse, string verbose)
        {
            _terse = terse == string.Empty ? null : terse;
            _verbose = verbose ?? throw new ArgumentNullException(nameof(verbose));
        }
        #endregion

        #region is match
        public bool IsMatch(string flag)
        {
            if (_terse is not null && _terse == flag)
                return true;

            if (_verbose == flag)
                return true;

            return false;
        }
        #endregion

        #region implicit conversions
        public static implicit operator OptionFlags((string terse, string verbose) flags)
        {
            return new OptionFlags(flags.terse, flags.verbose);
        }
        #endregion
    }
}
