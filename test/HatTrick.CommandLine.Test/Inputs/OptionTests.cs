// SPDX-License-Identifier: Apache-2.0
// Copyright (c) HatTrick Labs, LLC

using Xunit;

namespace HatTrick.CommandLine.Test
{
    public class OptionTests
    {
        #region constructor
        [Fact]
        public void Constructor_WhenNullProvided_ForFlag_ShouldThrow_ArgumentNullException()
        {
            Action action = () => new Option(null);
            Assert.Throws<ArgumentNullException>(action);
        }

        [Fact]
        public void Constructor_WhenEmptyProvided_ForFlag_ShouldThrow_ArgumentException()
        {
            Action action = () => new Option(string.Empty);
            Assert.Throws<ArgumentException>(action);
        }

        [Fact]
        public void Constructor_WhenNullProvided_ForKey_ShouldThrow_ArgumentNullException()
        {
            Action action = () => new Option(null, "-x");
            Assert.Throws<ArgumentNullException>(action);
        }

        [Fact]
        public void Constructor_WhenEmptyProvided_ForKey_ShouldThrow_ArgumentException()
        {
            Action action = () => new Option(string.Empty, "-x");
            Assert.Throws<ArgumentException>(action);
        }
        #endregion

        #region set key
        [Fact]
        public void SetKey_WhenNullProvided_ShouldThrow_ArgumentNullException()
        {
            var op = new Option("-x");
            Action action = () => op.SetKey(null);
            Assert.Throws<ArgumentNullException>(action);
        }

        [Fact]
        public void SetKey_WhenEmptyProvided_ShouldThrow_ArgumentException()
        {
            var op = new Option("-x");
            Action action = () => op.SetKey(string.Empty);
            Assert.Throws<ArgumentException>(action);
        }

        [Fact]
        public void SetKey_WhenValueProvided_ShouldRetainValue()
        {
            var op = new Option("-x");
            op.SetKey("xxx");
            Assert.Equal("xxx", op.Key);
        }
        #endregion

        #region set argument
        [Fact]
        public void SetArgument_WhenNullProvided_ShouldThrow_ArgumentNullException()
        {
            var op = new Option("-x");
            Action action = () => op.SetArgument(null);
            Assert.Throws<ArgumentNullException>(action);
        }

        [Fact]
        public void SetArgument_WhenEmptyProvided_ShouldRetainEmptyArgument()
        {
            var op = new Option("-x");
            op.SetArgument(string.Empty);
            Assert.Equal(string.Empty, op.Argument);
        }

        [Fact]
        public void SetArgument_WhenValueProvided_ShouldRetainValue()
        {
            var op = new Option("-x");
            op.SetArgument("arg");
            Assert.Equal("arg", op.Argument);
        }
        #endregion

        #region set value / get value
        [Fact]
        public void SetValue_WhenTypeValueProvided_ShouldRetain_ProvidedValue_AsDynamic()
        {
            var op = new Option("-x");
            op.SetValue(888);
            Assert.Equal(888, op.GetValue<int>());
        }

        [Fact]
        public void GetValue_WhenTypeValueSet_ShouldReturn_TypedValue()
        {
            var op = new Option("-x");
            op.SetValue(888);
            int result = op.GetValue<int>();
            Assert.Equal(888, result);
        }
        #endregion
    }
}
