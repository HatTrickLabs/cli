// SPDX-License-Identifier: Apache-2.0
// Copyright (c) HatTrick Labs, LLC

using Xunit;

namespace HatTrick.CommandLine.Test
{
    public class MaskedInputLineReaderTests
    {
        #region construction
        [Fact]
        public void Constructor_ShouldNotAccessConsole_AndShouldNotThrow()
        {
            //construction must be decoupled from console state so a reader can be created once
            //(globally / via DI) and reused later. all console access + per-read state reset
            //happens in ReadMaskedInput. under a redirected/headless host the old constructor
            //(which read Console.GetCursorPosition / Console.BufferWidth) would throw here.
            var reader = new MaskedInputLineReader();
            Assert.NotNull(reader);
        }
        #endregion
    }
}
