// SPDX-License-Identifier: Apache-2.0
// Copyright (c) HatTrick Labs, LLC

using System;

namespace HatTrick.CommandLine
{
    public class CommandParseException : Exception
    {
        public CommandParseException(string message) : base(message)
        {
        }
    }
}
