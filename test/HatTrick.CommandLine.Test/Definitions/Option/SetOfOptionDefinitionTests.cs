// SPDX-License-Identifier: Apache-2.0
// Copyright (c) HatTrick Labs, LLC

using Xunit;

namespace HatTrick.CommandLine.Test
{
    public class SetOfOptionDefinitionTests
    {
        #region indexer this[string key]
        [Fact]
        public void IndexerOnKeyString_WhenKeyExists_ShouldReturn_IndexedOptionDefinition()
        {
            var set = new SetOfOptionDefinition();
            set.Add(new OptionDefinition<string>("abc", "help", (x) => x, ("-a", "--abc")));
            set.Add(new OptionDefinition<string>("abcd", "help", (x) => x, ("-b", "--abcd")));
            set.Add(new OptionDefinition<string>("abcde", "help", (x) => x, ("-c", "--abcde")));

            OptionDefinition opDef = set["abcd"];

            Assert.Equal("abcd", opDef.Key);
        }

        [Fact]
        public void IndexerOnKeyString_WhenKeyDoesNotExists_ShouldThrow_KeyNotFoundException()
        {
            var set = new SetOfOptionDefinition();
            set.Add(new OptionDefinition<string>("abc", "help", (x) => x, ("-a", "--abc")));
            set.Add(new OptionDefinition<string>("abcd", "help", (x) => x, ("-b", "--abcd")));
            set.Add(new OptionDefinition<string>("abcde", "help", (x) => x, ("-c", "--abcde")));

            OptionDefinition? opDef = null;
            Action action = () => opDef = set["abcdefg"];
            Assert.Throws<KeyNotFoundException>(action);
        }
        #endregion

        #region constains key
        [Fact]
        public void ContainsKey_WhenSetIsEmpty_ShouldReturn_False()
        {
            var set = new SetOfOptionDefinition();
            bool result = set.ContainsKey("abc");
            Assert.False(result);
        }

        [Fact]
        public void ContainsKey_WhenSetContainsName_ShouldReturn_True()
        {
            var set = new SetOfOptionDefinition();
            set.Add(new OptionDefinition<string>("key", "help", (x) => x, ("-a", "--abc")));
            bool result = set.ContainsKey("key");
            Assert.True(result);
        }

        [Fact]
        public void ContainsKey_WhenNullArgumentProvided_ShouldThrow_ArgumentNullException()
        {
            var set = new SetOfOptionDefinition();
            set.Add(new OptionDefinition<string>("key", "help", (x) => x, ("-a", "--abc")));
            Action action = () => set.ContainsKey(null);
            Assert.Throws<ArgumentNullException>(action);
        }
        #endregion

        #region add
        [Fact]
        public void Add_WhenNullArgumentProvided_ShouldThrow_ArgumentNullException()
        {
            var set = new SetOfOptionDefinition();
            Action action = () => set.Add(null);
            Assert.Throws<ArgumentNullException>(action);
            return;
        }

        [Fact]
        public void Add_WhenOptionWithExistingKeyProvided_ShouldThrow_CommandDefinitionException()
        {
            var set = new SetOfOptionDefinition();
            set.Add(new OptionDefinition<string>("key", "help", (x) => x, ("-a", "--abc")));
            Action action = () => set.Add(new OptionDefinition<string>("key", "help", (x) => x, ("-a", "--abc")));
            Assert.Throws<CommandDefinitionException>(action);
            return;
        }

        [Fact]
        public void Add_WhenOptionWithExistingFlagProvided_ShouldThrow_CommandDefinitionException()
        {
            var set = new SetOfOptionDefinition();
            set.Add(new OptionDefinition<string>("key1", "help", (x) => x, ("-a", "--abc")));
            Action action = () => set.Add(new OptionDefinition<string>("key2", "help", (x) => x, ("-a", "--abcxx")));
            Assert.Throws<CommandDefinitionException>(action);
            return;
        }
        #endregion
    }
}
