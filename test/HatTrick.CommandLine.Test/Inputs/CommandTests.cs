// SPDX-License-Identifier: Apache-2.0
// Copyright (c) HatTrick Labs, LLC

using Xunit;

namespace HatTrick.CommandLine.Test
{
    public class CommandTests
    {
        #region constructors
        [Fact]
        public void Constructor_WithNullKeyArgument_ShouldThrow_ArgumentNullException()
        {
            Action action = () => new Command(null);
            Assert.Throws<ArgumentNullException>(action);
        }

        [Fact]
        public void Constructor_WithEmptyKeyArgument_ShouldThrow_ArgumentException()
        {
            Action action = () => new Command(string.Empty);
            Assert.Throws<ArgumentException>(action);
        }

        [Fact]
        public void Constructor_WithWhitespaceKeyArgument_ShouldThrow_ArgumentException()
        {
            Action action = () => new Command("   ");
            Assert.Throws<ArgumentException>(action);
        }

        [Fact]
        public void Constructor_WithValidKeyArgument_ShouldRetainKey()
        {
            var cmd = new Command("abc");
            Assert.Equal("abc", cmd.Name);
        }

        [Fact]
        public void Constructor_WithNullOptionsArgument_ShouldRetainEmptySetOfOptions()
        {
            var cmd = new Command("abc", null);
            Assert.Equal(Array.Empty<Option>(), cmd.Options.ToArray());
        }
        #endregion

        #region add empty option
        [Fact]
        public void AddEmptyOption_ShouldPersist_EmtpyOption()
        {
            var cmd = new Command("abc");
            var op = new EmptyOption("xyz", "-x");
            cmd.AddEmptyOption(op);
            Assert.Equal(op, cmd["xyz"]);
        }

        [Fact]
        public void AddEmptyOption_WhenOptionWithKeyAlreadyExists_ShouldThrow_ArgumentException()
        {
            var op1 = new Option("xyz", "--xyz");
            var cmd = new Command("abc", op1);
            var op2 = new EmptyOption("xyz", "-x");
            Action action = () => cmd.AddEmptyOption(op2);
            Assert.Throws<ArgumentException>(action);
        }
        #endregion

        #region get options
        [Fact]
        public void GetOptions_WhenWherePredicateIsNull_ShouldReturn_AllOptions_InOrderOptionsOriginallyAdded()
        {
            var cmd = new Command("abc", 
                new Option("key1", "--flag1"),
                new Option("key2", "--flag2"),
                new Option("xxx1", "--xxx"),
                new Option("key4", "--flag4"),
                new Option("key5", "--flag5")
            );

            var ops = cmd.GetOptions(null);

            Assert.Collection<Option>(ops,
                (o) => Assert.Equal("key1", o.Key),
                (o) => Assert.Equal("key2", o.Key),
                (o) => Assert.Equal("xxx1", o.Key),
                (o) => Assert.Equal("key4", o.Key),
                (o) => Assert.Equal("key5", o.Key)
            );
        }

        [Fact]
        public void GetOptions_WhenPredicateProvided_ShouldReturn_MatchedOptions_InOrderOptionsOriginallyAdded()
        {
            var cmd = new Command("abc",
                new Option("key1", "--flag1"),
                new Option("key2", "--flag2"),
                new Option("xxx1", "--xxx"),
                new Option("key4", "--flag4"),
                new Option("key5", "--flag5")
            );

            var ops = cmd.GetOptions((o) => o.Flag.StartsWith("--f"));

            Assert.Collection<Option>(ops,
                (o) => Assert.Equal("key1", o.Key),
                (o) => Assert.Equal("key2", o.Key),
                (o) => Assert.Equal("key4", o.Key),
                (o) => Assert.Equal("key5", o.Key)
            );
        }
        #endregion

        #region get option by flag
        [Fact]
        public void GetOptionByFlag_WhenNullArgumentProvided_ShouldThrow_ArgumentNullException()
        {
            var cmd = new Command("abc",
                new Option("key1", "--flag1"),
                new Option("key2", "--flag2"),
                new Option("xxx1", "--xxx"),
                new Option("key4", "--flag4"),
                new Option("key5", "--flag5")
            );


            Action action = () => cmd.GetOptionByFlag(null);
            Assert.Throws<ArgumentNullException>(action);
        }

        [Fact]
        public void GetOptionByFlag_WhenEmptyAarrayProvided_ShouldThrow_ArgumentException()
        {
            var cmd = new Command("abc",
                new Option("key1", "--flag1"),
                new Option("key2", "--flag2"),
                new Option("xxx1", "--xxx"),
                new Option("key4", "--flag4"),
                new Option("key5", "--flag5")
            );

            Action action = () => cmd.GetOptionByFlag(new string[] { });
            Assert.Throws<ArgumentException>(action);
        }

        [Fact]
        public void GetOptionByFlag_ShouldReturn_Null_IfNoOptionFlagsMatch()
        {
            var cmd = new Command("abc",
                new Option("key1", "--flag1"),
                new Option("key2", "--flag2"),
                new Option("xxx1", "--xxx"),
                new Option("key4", "--flag4"),
                new Option("key5", "--flag5")
            );

            Option o = cmd.GetOptionByFlag("--abc", "-a");
            Assert.Null(o);
        }

        [Fact]
        public void GetOptionByFlag_ShouldReturn_MatchedOption_WhenOptionFlagMatches()
        {
            var cmd = new Command("abc",
                new Option("key1", "--flag1"),
                new Option("key2", "--flag2"),
                new Option("xxx1", "--xxx"),
                new Option("key4", "--flag4"),
                new Option("key5", "--flag5")
            );

            Option o = cmd.GetOptionByFlag("--abc", "-a", "--xxx");
            Assert.Equal("xxx1", o.Key);
        }
        #endregion

        #region get option by ref
        [Fact]
        public void GetOptionByRef_WhenNullKeyProvided_ShouldThrow_ArgumentNullException()
        {
            var cmd = new Command("abc",
                new Option("key1", "--flag1"),
                new Option("key2", "--flag2")
            );

            Action action = () =>
            {
                ref Option o = ref cmd.GetOptionByRef(null);
            };

            Assert.Throws<ArgumentNullException>(action);
        }

        [Fact]
        public void GetOptionByRef_WhenEmptyKeyProvided_ShouldThrow_ArgumentException()
        {
            var cmd = new Command("abc",
                new Option("key1", "--flag1"),
                new Option("key2", "--flag2")
            );

            Action action = () =>
            {
                ref Option o = ref cmd.GetOptionByRef(string.Empty);
            };

            Assert.Throws<ArgumentException>(action);
        }

        [Fact]
        public void GetOptionByRef_WhenKeyMatch_ShouldReturn_PointerToInternalOption()
        {
            var cmd = new Command("abc",
                new Option("key1", "--flag1"),
                new Option("key2", "--flag2"),
                new Option("key3", "--flag3")
            );

            //get a pointer to index 2 of the cmd options internal array
            ref Option o = ref cmd.GetOptionByRef("key2");

            //set the local pointer = new option
            o = new Option("xxx1", "--xxx");

            //ensure index 2 of the cmd options internal array now references the new option
            Assert.Equal("xxx1", cmd.Options[1].Key);
        }
        #endregion
    }
}
