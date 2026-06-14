// SPDX-License-Identifier: Apache-2.0
// Copyright (c) HatTrick Labs, LLC

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
            switch (value.ToLowerInvariant())
            {
                case "false":
                case "no":
                    return false;
                case "true":
                case "yes":
                    return true;
                default:
                    throw new FormatException($"Cannot convert value to bool: '{value}'");
            }
        }
        #endregion
    }
}
