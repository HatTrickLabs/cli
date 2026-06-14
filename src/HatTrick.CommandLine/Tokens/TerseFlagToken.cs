// SPDX-License-Identifier: Apache-2.0
// Copyright (c) HatTrick Labs, LLC

using System;

namespace HatTrick.CommandLine
{
    public class TerseFlagToken : FlagToken
    {
        #region constructor
        //HACK: Determined to allow -?, just swap in the -h
        internal TerseFlagToken(string value) : base(value == "-?" ? "-h" : value)
        { }
        #endregion

        #region is valid
        public new static bool IsValid(string value)
        {
            if (!FlagToken.IsValid(value))
                return false;

            if (value.Length != 2)
                return false;

            if (value[1] == '-')
                return false;

            //HACK: after much usage, this is an absolutely necessity for ME...
            //the -? will be reserved for the baked in -h|--help|-? flag
            if (value[1] == '?')
                return true;

            if (!char.IsLetter(value[1]))
                return false;

            return true;
        }
        #endregion
    }
}
