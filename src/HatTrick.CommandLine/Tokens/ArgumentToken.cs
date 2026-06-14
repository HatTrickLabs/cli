// SPDX-License-Identifier: Apache-2.0
// Copyright (c) HatTrick Labs, LLC

using System;

namespace HatTrick.CommandLine
{
    #region argument token
    public class ArgumentToken : Token
    {
        #region internals
        private bool _isMerged;
        #endregion

        #region interface
        internal bool IsMerged => _isMerged;
        #endregion

        #region constructor
        internal ArgumentToken(string value) : base(value)
        { }
        #endregion

        #region merge
        internal void Merge(string with)
        {
            base.Value = base.Value + with;
            _isMerged = true;
        }
        #endregion

        #region is valid
        public new static bool IsValid(string value)
        {
            return Token.IsValid(value);
        }
        #endregion
    }
    #endregion
}
