// SPDX-License-Identifier: Apache-2.0
// Copyright (c) HatTrick Labs, LLC

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HatTrick.CommandLine
{
    public interface IPreEnsureCommand
    {
        public string Name { get; }

        public IPreEnsureOption GetOption(Predicate<IPreEnsureOption> where);
    }
}
