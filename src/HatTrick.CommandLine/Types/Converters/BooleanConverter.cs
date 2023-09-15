using System;

namespace HatTrick.CommandLine
{
    internal static class BooleanConverter
    {
        #region internals
        private static readonly string[] _validBools = { "false", "no", "n", "0", "true", "yes", "y", "1" };
        #endregion

        #region is valid bool
        private static bool IsValidBool(string value, out bool result)
        {
            int idx = Array.FindIndex(_validBools, (b) => string.Compare(b, value, true) == 0);
            result = idx < 4 ? false : true;
            return idx > -1;
        }
        #endregion

        #region convert to boolean
        internal static bool ToBoolean(string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (value != string.Empty)
            {
                if (BooleanConverter.IsValidBool(value, out bool boolean))
                    return boolean;
            }

            throw new FormatException($"Cannot convert value to bool: '{value}'...valid bool values (case insensitive): {string.Join("|", _validBools)}");
        }
        #endregion
    }
}
