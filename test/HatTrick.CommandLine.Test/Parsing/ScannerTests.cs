// SPDX-License-Identifier: Apache-2.0
// Copyright (c) HatTrick Labs, LLC

using HatTrick.CommandLine;
using Xunit;

namespace HatTrick.CommandLine.Test
{
    public class ScannerTests
    {
        [Fact]
        public void Scan_IfInput_IsNull_ShouldThrow_ArgumentNullException()
        {
            Action action = () => Scanner.Scan(null);
            Assert.Throws<ArgumentNullException>(action);
        }

        [Fact]
        public void Scan_IfInput_IsEmpty_ShouldReturn_EmptyArrayOfString()
        {
            string[] result = Scanner.Scan(string.Empty);
            Assert.Equal(Array.Empty<string>(), result);
        }

        [Fact]
        public void Scan_ShouldSplit_SingleCharTokens_OnSpace()
        {
            string value = "x y z";
            string[] result = Scanner.Scan(value);
            Assert.Collection<string>(result, 
                (x) => Assert.Equal("x", x), 
                (y) => Assert.Equal("y", y), 
                (z) => Assert.Equal("z", z)
            );
        }

        [Fact]
        public void Scan_ShouldSplit_MultiCharTokens_OnSpace()
        {
            string value = "xxx yyy zzz";
            string[] result = Scanner.Scan(value);
            Assert.Collection<string>(result,
                (x) => Assert.Equal("xxx", x),
                (y) => Assert.Equal("yyy", y),
                (z) => Assert.Equal("zzz", z)
            );
        }

        [Fact]
        public void Scan_ShouldSplit_SingleCharTokens_OnTab()
        {
            string value = "x\ty\tz";
            string[] result = Scanner.Scan(value);
            Assert.Collection<string>(result,
                (x) => Assert.Equal("x", x),
                (y) => Assert.Equal("y", y),
                (z) => Assert.Equal("z", z)
            );
        }

        [Fact]
        public void Scan_ShouldSplit_MultiCharTokens_OnTab()
        {
            string value = "xxx\tyyy\tzzz";
            string[] result = Scanner.Scan(value);
            Assert.Collection<string>(result,
                (x) => Assert.Equal("xxx", x),
                (y) => Assert.Equal("yyy", y),
                (z) => Assert.Equal("zzz", z)
            );
        }

        [Fact]
        public void Scan_ShouldSplit_SingleCharTokens_OnNewLine()
        {
            string value = "x\ny\nz";
            string[] result = Scanner.Scan(value);
            Assert.Collection<string>(result,
                (x) => Assert.Equal("x", x),
                (y) => Assert.Equal("y", y),
                (z) => Assert.Equal("z", z)
            );
        }

        [Fact]
        public void Scan_ShouldSplit_MultiCharTokens_OnNewLine()
        {
            string value = "xxx\nyyy\nzzz";
            string[] result = Scanner.Scan(value);
            Assert.Collection<string>(result,
                (x) => Assert.Equal("xxx", x),
                (y) => Assert.Equal("yyy", y),
                (z) => Assert.Equal("zzz", z)
            );
        }

        [Fact]
        public void Scan_ShouldSplit_SingleCharTokens_OnCarriageReturn()
        {
            string value = "x\ry\rz";
            string[] result = Scanner.Scan(value);
            Assert.Collection<string>(result,
                (x) => Assert.Equal("x", x),
                (y) => Assert.Equal("y", y),
                (z) => Assert.Equal("z", z)
            );
        }

        [Fact]
        public void Scan_ShouldSplit_MultiCharTokens_OnCarriageReturn()
        {
            string value = "xxx\ryyy\rzzz";
            string[] result = Scanner.Scan(value);
            Assert.Collection<string>(result,
                (x) => Assert.Equal("xxx", x),
                (y) => Assert.Equal("yyy", y),
                (z) => Assert.Equal("zzz", z)
            );
        }

        [Fact]
        public void Scan_ShouldSplit_SingleCharTokens_OnCarriageReturnNewLine()
        {
            string value = "x\r\ny\r\nz";
            string[] result = Scanner.Scan(value);
            Assert.Collection<string>(result,
                (x) => Assert.Equal("x", x),
                (y) => Assert.Equal("y", y),
                (z) => Assert.Equal("z", z)
            );
        }

        [Fact]
        public void Scan_ShouldSplit_MultiCharTokens_OnCarriageReturnNewLine()
        {
            string value = "xxx\r\nyyy\r\nzzz";
            string[] result = Scanner.Scan(value);
            Assert.Collection<string>(result,
                (x) => Assert.Equal("xxx", x),
                (y) => Assert.Equal("yyy", y),
                (z) => Assert.Equal("zzz", z)
            );
        }

        [Fact]
        public void Scan_ShouldSplit_SingleCharTokens_OnWhitespace_WhenWhitespace_MoreThanOneSpace()
        {
            string value = "x    y           z";
            string[] result = Scanner.Scan(value);
            Assert.Collection<string>(result,
                (x) => Assert.Equal("x", x),
                (y) => Assert.Equal("y", y),
                (z) => Assert.Equal("z", z)
            );
        }

        [Fact]
        public void Scan_ShouldSplit_MultiCharTokens_OnWhitespace_WhenWhitespace_MoreThanOneSpace()
        {
            string value = "xxx   yyy       zzz";
            string[] result = Scanner.Scan(value);
            Assert.Collection<string>(result,
                (x) => Assert.Equal("xxx", x),
                (y) => Assert.Equal("yyy", y),
                (z) => Assert.Equal("zzz", z)
            );
        }

        [Fact]
        public void Scan_ShouldSplit_SingleCharTokens_OnWhitespace_WhenWhitespace_Chaos()
        {
            string value = "x \t\r\n   y  \n         z";
            string[] result = Scanner.Scan(value);
            Assert.Collection<string>(result,
                (x) => Assert.Equal("x", x),
                (y) => Assert.Equal("y", y),
                (z) => Assert.Equal("z", z)
            );
        }

        [Fact]
        public void Scan_ShouldSplit_MultiCharTokens_OnWhitespace_WhenWhitespace_Chaos()
        {
            string value = "xxx  \r\n\t  yyy    \n   zzz";
            string[] result = Scanner.Scan(value);
            Assert.Collection<string>(result,
                (x) => Assert.Equal("xxx", x),
                (y) => Assert.Equal("yyy", y),
                (z) => Assert.Equal("zzz", z)
            );
        }

        [Fact]
        public void Scan_ShouldMaintain_QuotedWhitespace()
        {
            string value = "This is unquoted... \"This is quoted\"...";
            string[] result = Scanner.Scan(value);
            Assert.Collection<string>(result,
                (x) => Assert.Equal("This", x),
                (x) => Assert.Equal("is", x),
                (x) => Assert.Equal("unquoted...", x),
                (x) => Assert.Equal("This is quoted", x),
                (x) => Assert.Equal("...", x)
           );
        }

        [Fact]
        public void Scan_ShouldSplit_OnEqual_ExplicitAssign()
        {
            string value = "abc=123 abc =123 abc= 123 abc = 123";
            string[] result = Scanner.Scan(value);
            Assert.Collection<string>(result,
                 (x) => Assert.Equal("abc", x),
                (x) => Assert.Equal("=", x),
                (x) => Assert.Equal("123", x),
                (x) => Assert.Equal("abc", x),
                (x) => Assert.Equal("=", x),
                (x) => Assert.Equal("123", x),
                (x) => Assert.Equal("abc", x),
                (x) => Assert.Equal("=", x),
                (x) => Assert.Equal("123", x),
                (x) => Assert.Equal("abc", x),
                (x) => Assert.Equal("=", x),
                (x) => Assert.Equal("123", x)
            );
        }

        [Fact]
        public void Scan_ShouldTreat_QuoteAsLiteral_WhenEscaped()
        {
            string value = "\"This is \\\"quoted\\\".\"";
            string[] result = Scanner.Scan(value);
            Assert.Equal("This is \"quoted\".", result[0]);
        }

        [Fact]
        public void Scan_ShouldHandle_Inputs_LargerThan_MaxStackAllocLength()
        {
            string part = new string('x', 500);
            string whole = part + " " + part + " " + part + " " + part;//2003 chars
            string[] result = Scanner.Scan(whole);
            Assert.Collection<string>(result,
                (x) => Assert.Equal(part, x),
                (x) => Assert.Equal(part, x),
                (x) => Assert.Equal(part, x),
                (x) => Assert.Equal(part, x)
            );
        }

        [Fact]
        public void Scan_IfInput_GreaterThan_MaxSourceLength_ShouldThrow_RangeOverflowException()
        {
            string part = new string('x', 500);
            string whole = part + " " + part + " " + part + " " + part + " " + part;//2504 chars

            Action action = () => Scanner.Scan(whole);
            Assert.Throws<RangeOverflowException>(action);
        }

        [Fact]
        public void Scan_IfInput_ContainsEscapeChar_ShouldTreat_EscapeAsLiteral_IfNotFollowedBy_DoubleQuotes()
        {
            string value = "x \\escape\\ x";
            string[] result = Scanner.Scan(value);
            Assert.Collection<string>(result,
                (x) => Assert.Equal("x", x),
                (x) => Assert.Equal("\\escape\\", x),
                (x) => Assert.Equal("x", x)
            );
        }
    }
}
