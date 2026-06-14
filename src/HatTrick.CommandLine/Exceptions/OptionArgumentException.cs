// SPDX-License-Identifier: Apache-2.0
// Copyright (c) HatTrick Labs, LLC

using System;

namespace HatTrick.CommandLine
{
    public class OptionArgumentException : Exception
    {
        public OptionArgumentException(string message) : base(message)
        {
        }
    }
}
