// SPDX-License-Identifier: Apache-2.0
// Copyright (c) HatTrick Labs, LLC

using System;

namespace HatTrick.CommandLine
{
    public class NamespaceDefinitionException : Exception
    {
        public NamespaceDefinitionException(string message) : base(message)
        { }
    }
}
