using HatTrick.CommandLine;
using Xunit;

namespace HatTrick.CommandLine.Test
{
    public class TokenizerTests
    {
        [Fact]
        public void Tokenize_ShouldSplit_SingleChars_OnSpace()
        {
            string value = "x y z";
            string[] result = Tokenizer.Tokenize(value);
            Assert.Collection<string>(result, 
                (x) => Assert.Equal("x", x), 
                (y) => Assert.Equal("y", y), 
                (z) => Assert.Equal("z", z)
            );
        }

        [Fact]
        public void Tokenize_ShouldSplit_MultiCharTokens_OnSpace()
        {
            string value = "xxx yyy zzz";
            string[] result = Tokenizer.Tokenize(value);
            Assert.Collection<string>(result,
                (x) => Assert.Equal("xxx", x),
                (y) => Assert.Equal("yyy", y),
                (z) => Assert.Equal("zzz", z)
            );
        }

        [Fact]
        public void Tokenize_ShouldSplit_SingleChars_OnTab()
        {
            string value = "x\ty\tz";
            string[] result = Tokenizer.Tokenize(value);
            Assert.Collection<string>(result,
                (x) => Assert.Equal("x", x),
                (y) => Assert.Equal("y", y),
                (z) => Assert.Equal("z", z)
            );
        }

        [Fact]
        public void Tokenize_ShouldSplit_MultiCharTokens_OnTab()
        {
            string value = "xxx\tyyy\tzzz";
            string[] result = Tokenizer.Tokenize(value);
            Assert.Collection<string>(result,
                (x) => Assert.Equal("xxx", x),
                (y) => Assert.Equal("yyy", y),
                (z) => Assert.Equal("zzz", z)
            );
        }

        [Fact]
        public void Tokenize_ShouldSplit_SingleChars_OnNewLine()
        {
            string value = "x\ny\nz";
            string[] result = Tokenizer.Tokenize(value);
            Assert.Collection<string>(result,
                (x) => Assert.Equal("x", x),
                (y) => Assert.Equal("y", y),
                (z) => Assert.Equal("z", z)
            );
        }

        [Fact]
        public void Tokenize_ShouldSplit_MultiCharTokens_OnNewLine()
        {
            string value = "xxx\nyyy\nzzz";
            string[] result = Tokenizer.Tokenize(value);
            Assert.Collection<string>(result,
                (x) => Assert.Equal("xxx", x),
                (y) => Assert.Equal("yyy", y),
                (z) => Assert.Equal("zzz", z)
            );
        }

        [Fact]
        public void Tokenize_ShouldSplit_SingleChars_OnCarriageReturn()
        {
            string value = "x\ry\rz";
            string[] result = Tokenizer.Tokenize(value);
            Assert.Collection<string>(result,
                (x) => Assert.Equal("x", x),
                (y) => Assert.Equal("y", y),
                (z) => Assert.Equal("z", z)
            );
        }

        [Fact]
        public void Tokenize_ShouldSplit_MultiCharTokens_OnCarriageReturn()
        {
            string value = "xxx\ryyy\rzzz";
            string[] result = Tokenizer.Tokenize(value);
            Assert.Collection<string>(result,
                (x) => Assert.Equal("xxx", x),
                (y) => Assert.Equal("yyy", y),
                (z) => Assert.Equal("zzz", z)
            );
        }

        [Fact]
        public void Tokenize_ShouldSplit_SingleChars_OnCarriageReturnNewLine()
        {
            string value = "x\r\ny\r\nz";
            string[] result = Tokenizer.Tokenize(value);
            Assert.Collection<string>(result,
                (x) => Assert.Equal("x", x),
                (y) => Assert.Equal("y", y),
                (z) => Assert.Equal("z", z)
            );
        }

        [Fact]
        public void Tokenize_ShouldSplit_MultiCharTokens_OnCarriageReturnNewLine()
        {
            string value = "xxx\r\nyyy\r\nzzz";
            string[] result = Tokenizer.Tokenize(value);
            Assert.Collection<string>(result,
                (x) => Assert.Equal("xxx", x),
                (y) => Assert.Equal("yyy", y),
                (z) => Assert.Equal("zzz", z)
            );
        }

        [Fact]
        public void Tokenize_ShouldSplit_SingleChars_OnWhitespace_WhenWhitespace_MoreThanOneSpace()
        {
            string value = "x    y           z";
            string[] result = Tokenizer.Tokenize(value);
            Assert.Collection<string>(result,
                (x) => Assert.Equal("x", x),
                (y) => Assert.Equal("y", y),
                (z) => Assert.Equal("z", z)
            );
        }

        [Fact]
        public void Tokenize_ShouldSplit_MultiCharTokens_OnWhitespace_WhenWhitespace_MoreThanOneSpace()
        {
            string value = "xxx   yyy       zzz";
            string[] result = Tokenizer.Tokenize(value);
            Assert.Collection<string>(result,
                (x) => Assert.Equal("xxx", x),
                (y) => Assert.Equal("yyy", y),
                (z) => Assert.Equal("zzz", z)
            );
        }

        [Fact]
        public void Tokenize_ShouldSplit_SingleChars_OnWhitespace_WhenWhitespace_Chaos()
        {
            string value = "x \t\r\n   y  \n         z";
            string[] result = Tokenizer.Tokenize(value);
            Assert.Collection<string>(result,
                (x) => Assert.Equal("x", x),
                (y) => Assert.Equal("y", y),
                (z) => Assert.Equal("z", z)
            );
        }

        [Fact]
        public void Tokenize_ShouldSplit_MultiCharTokens_OnWhitespace_WhenWhitespace_Chaos()
        {
            string value = "xxx  \r\n\t  yyy    \n   zzz";
            string[] result = Tokenizer.Tokenize(value);
            Assert.Collection<string>(result,
                (x) => Assert.Equal("xxx", x),
                (y) => Assert.Equal("yyy", y),
                (z) => Assert.Equal("zzz", z)
            );
        }

        [Fact]
        public void Tokenize_ShouldMaintain_QuotedWhitespace()
        {
            string value = "This is unquoted... \"This is quoted\"...";
            string[] result = Tokenizer.Tokenize(value);
            Assert.Collection<string>(result,
                (x) => Assert.Equal("This", x),
                (x) => Assert.Equal("is", x),
                (x) => Assert.Equal("unquoted...", x),
                (x) => Assert.Equal("This is quoted...", x)
            );
        }

        [Fact]
        public void Tokenize_ShouldMaintain_QuotedWhitespace_AndQuotes_WhenKeepLiteralQuotesOptionEnabled()
        {
            string value = "This is unquoted... \"This is quoted\"...";
            string[] result = Tokenizer.Tokenize(value, true);
            Assert.Collection<string>(result,
                (x) => Assert.Equal("This", x),
                (x) => Assert.Equal("is", x),
                (x) => Assert.Equal("unquoted...", x),
                (x) => Assert.Equal("\"This is quoted\"...", x)
            );
        }

        [Fact]
        public void Tokenize_ShouldTreat_QuoteAsLiteral_WhenEscaped_WhenKeepLiteralQuotesOptionDisabled()
        {
            string value = "\"This is \\\"quoted\\\".\"";
            string[] result = Tokenizer.Tokenize(value);
            Assert.Equal("This is \"quoted\".", result[0]);
        }

        [Fact]
        public void Tokenize_ShouldTreat_QuoteAsLiteral_WhenEscaped_WhenKeepLiteralQuotesOptionEnabled()
        {
            string value = "\"This is \\\"quoted\\\".\"";
            string[] result = Tokenizer.Tokenize(value, true);
            Assert.Equal("\"This is \"quoted\".\"", result[0]);
        }

        [Fact]
        public void Tokenize_ShouldHandle_Inputs_LargerThan_MaxStackAllocLength()
        {
            string part = new string('x', 500);
            string whole = part + " " + part + " " + part + " " + part;//2003 chars
            string[] result = Tokenizer.Tokenize(whole);
            Assert.Collection<string>(result,
                (x) => Assert.Equal(part, x),
                (x) => Assert.Equal(part, x),
                (x) => Assert.Equal(part, x),
                (x) => Assert.Equal(part, x)
            );
        }

        [Fact]
        public void Tokenize_IfInput_IsNull_ShouldThrow_ArgumentNullException()
        {
            Action action = () => Tokenizer.Tokenize(null);
            Assert.Throws<ArgumentNullException>(action);
        }

        [Fact]
        public void Tokenize_IfInput_IsEmpty_ShouldReturn_EmptyArrayOfString()
        {
            string[] result = Tokenizer.Tokenize(string.Empty);
            Assert.Equal(Array.Empty<string>(), result);
        }

        [Fact]
        public void Tokenize_IfInput_GreaterThan_MaxSourceLength_ShouldThrow_RangeOverflowException()
        {
            string part = new string('x', 500);
            string whole = part + " " + part + " " + part + " " + part + " " + part;//2504 chars

            Action action = () => Tokenizer.Tokenize(whole);
            Assert.Throws<RangeOverflowException>(action);
        }

        [Fact]
        public void Tokenize_IfInput_ContainsEscapeChar_ShouldTreat_EscapeAsLiteral_IfNotFollowedBy_DoubleQuotes()
        {
            string value = "x \\escape\\ x";
            string[] result = Tokenizer.Tokenize(value);
            Assert.Collection<string>(result,
                (x) => Assert.Equal("x", x),
                (x) => Assert.Equal("\\escape\\", x),
                (x) => Assert.Equal("x", x)
            );
        }
    }
}
