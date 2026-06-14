// SPDX-License-Identifier: Apache-2.0
// Copyright (c) HatTrick Labs, LLC

using System;

namespace HatTrick.CommandLine
{
    public class RangeOverflowException : Exception
    {
        public RangeOverflowException(string message) : base(message)
        { }
    }
}
