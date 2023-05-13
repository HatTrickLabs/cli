using System;

namespace Crypto.CommandLine
{
    public static class BooleanConverter
    {
        #region internals
        private static readonly string[] _validBools = { "false", "no", "n", "0", "true", "yes", "y", "1" };
        #endregion

        #region is valid bool
        public static bool IsValidBool(string value, out bool result)
        {
            int idx = Array.FindIndex(_validBools, (b) => string.Compare(b, value, true) == 0);
            result = idx < 4 ? false : true;
            return idx > -1;
        }
        #endregion

        #region convert to boolean
        public static bool ConvertToBoolean(string value)
        {
            //boolean is the only option that defaults to true (flag does not require an arg)
            if (value == null || value == string.Empty)
                return true;

            if (BooleanConverter.IsValidBool(value, out bool boolean))
                return boolean;

            throw new FormatException($"Cannot convert value to bool: '{value}'...valid bool values (case insensitive): {string.Join(", ", _validBools)}");
        }
        #endregion
    }
}
