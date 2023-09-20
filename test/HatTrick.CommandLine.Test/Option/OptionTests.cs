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

        #region apply key
        [Fact]
        public void ApplyKey_WhenNullProvided_ShouldThrow_ArgumentNullException()
        {
            var op = new Option("-x");
            Action action = () => op.ApplyKey(null);
            Assert.Throws<ArgumentNullException>(action);
        }

        [Fact]
        public void ApplyKey_WhenEmptyProvided_ShouldThrow_ArgumentException()
        {
            var op = new Option("-x");
            Action action = () => op.ApplyKey(string.Empty);
            Assert.Throws<ArgumentException>(action);
        }

        [Fact]
        public void ApplyKey_WhenValueProvided_ShouldRetainValue()
        {
            var op = new Option("-x");
            op.ApplyKey("xxx");
            Assert.Equal("xxx", op.Key);
        }
        #endregion

        #region apply argument
        [Fact]
        public void ApplyArgument_WhenNullProvided_ShouldThrow_ArgumentNullException()
        {
            var op = new Option("-x");
            Action action = () => op.ApplyArgument(null);
            Assert.Throws<ArgumentNullException>(action);
        }

        [Fact]
        public void ApplyArgument_WhenEmptyProvided_ShouldThrow_ArgumentException()
        {
            var op = new Option("-x");
            Action action = () => op.ApplyArgument(string.Empty);
            Assert.Throws<ArgumentException>(action);
        }

        [Fact]
        public void ApplyArgument_WhenValueProvided_ShouldRetainValue()
        {
            var op = new Option("-x");
            op.ApplyArgument("arg");
            Assert.Equal("arg", op.Argument);
        }
        #endregion

        #region set value / get value
        [Fact]
        public void SetValue_WhenTypeValueProvided_ShouldRetain_ProvidedValue_AsDynamic()
        {
            var op = new Option("-x");
            op.SetValue<int>(888);
            Assert.Equal(888, op.Value);
        }

        [Fact]
        public void GetValue_WhenTypeValueSet_ShouldReturn_TypedValue()
        {
            var op = new Option("-x");
            op.SetValue<int>(888);
            int result = op.GetValue<int>();
            Assert.Equal(888, result);
        }
        #endregion
    }
}
