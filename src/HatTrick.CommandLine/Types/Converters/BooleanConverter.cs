using System;
using System.Runtime.CompilerServices;

namespace HatTrick.CommandLine
{
    internal static class BooleanConverter
    {
        #region convert to boolean
        internal static bool ToBoolean(string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (value.Length == 1)
                return CharToBoolean(value[0]);
            else
                return StringToBoolean(value);
        }
        #endregion

        #region char to boolean
        private static bool CharToBoolean(char c)
        {
            switch (c)
            {
                case '0':
                    return false;
                case '1':
                    return true;
                case 'n':
                    return false;
                case 'y':
                    return true;
                case 'N':
                    return false;
                case 'Y':
                    return true;
                default:
                    throw new FormatException($"Cannot convert value to bool: '{c}'");
            }
        }
        #endregion

        #region string to boolean
        private static bool StringToBoolean(string value)
        {
            switch (value)
            {
                case "false":
                    return false;
                case "true":
                    return true;
                case "False":
                    return false;
                case "True":
                    return true;
                case "no":
                    return false;
                case "yes":
                    return true;
                case "No":
                    return false;
                case "Yes":
                    return true;
                default:
                    throw new FormatException($"Cannot convert value to bool: '{value}'");
            }
        }
        #endregion
    }
}
