// SPDX-License-Identifier: Apache-2.0
// Copyright (c) HatTrick Labs, LLC

using System;

namespace HatTrick.CommandLine
{
    public class EmptyOption : Option
    {
        public bool IsEmpty => true;

        public EmptyOption(string key, string flag) : base(key, flag)
        { }
    }
}
