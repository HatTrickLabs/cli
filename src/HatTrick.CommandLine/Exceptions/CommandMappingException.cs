// SPDX-License-Identifier: Apache-2.0
// Copyright (c) HatTrick Labs, LLC

using System;

namespace HatTrick.CommandLine
{
    public class CommandMappingException : Exception
    {
        public CommandMappingException(string message) : base(message)
        { }
    }
}
