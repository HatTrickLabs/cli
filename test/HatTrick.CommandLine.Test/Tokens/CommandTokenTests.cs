// SPDX-License-Identifier: Apache-2.0
// Copyright (c) HatTrick Labs, LLC

using Xunit;

namespace HatTrick.CommandLine.Test
{
    public class CommandTokenTests
    {
        #region is valid
        [Theory]
        [InlineData("@foo")]     // invalid first char
        [InlineData("foo@")]     // invalid last char
        [InlineData("a..b")]     // empty interior segment
        [InlineData(".foo")]     // leading dot
        [InlineData("foo.")]     // trailing dot
        [InlineData("-foo")]     // leading dash
        [InlineData("foo-")]     // trailing dash
        [InlineData("")]         // empty
        public void IsValid_WhenValueIsMalformed_ShouldReturnFalse(string value)
        {
            Assert.False(CommandToken.IsValid(value));
        }

        [Theory]
        [InlineData("abc")]
        [InlineData("a.b.c")]
        [InlineData("1abc")]     // digit-first is allowed for commands
        [InlineData("a+b")]      // '+' is a valid command char
        public void IsValid_WhenValueIsWellFormed_ShouldReturnTrue(string value)
        {
            Assert.True(CommandToken.IsValid(value));
        }
        #endregion
    }
}
