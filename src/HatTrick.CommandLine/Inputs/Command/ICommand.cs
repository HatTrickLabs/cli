// SPDX-License-Identifier: Apache-2.0
// Copyright (c) HatTrick Labs, LLC

using System;
using System.Collections.Generic;

namespace HatTrick.CommandLine
{
    public interface ICommand
    {
        public string Name { get; }

        public IOption this[string key] { get; }

        public IOption GetOption(Predicate<IOption> where);
    }
}
