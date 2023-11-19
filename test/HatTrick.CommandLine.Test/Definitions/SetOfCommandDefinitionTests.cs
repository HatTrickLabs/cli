using Xunit;

namespace HatTrick.CommandLine.Test
{
    public class SetOfCommandDefinitionTests
    {
        #region indexer this[string key]
        [Fact]
        public void IndexerOnKeyString_WhenKeyExists_ShouldReturn_IndexedNamespace()
        {
            var set = new SetOfCommandDefinition();
            set.Add(new(name: "abc") { Handler = (cmd) => { } });
            set.Add(new(name: "abcd") { Handler = (cmd) => { } });
            set.Add(new(name: "abcde") { Handler = (cmd) => { } });
            set.Add(new(name: "abcdef") { Handler = (cmd) => { } });

            CommandDefinition cd = set["abcde"];

            Assert.Equal("abcde", cd.Name);
        }

        [Fact]
        public void IndexerOnKeyString_WhenKeyDoesNotExists_ShouldThrow_KeyNotFoundException()
        {
            var set = new SetOfCommandDefinition();
            set.Add(new(name: "abcd") { Handler = (cmd) => { } });
            set.Add(new(name: "abcde") { Handler = (cmd) => { } });
            set.Add(new(name: "abcdef") { Handler = (cmd) => { } });

            CommandDefinition? cd = null;
            Action action = () => cd = set["abcdefg"];
            Assert.Throws<KeyNotFoundException>(action);
        }
        #endregion

        #region constains name
        [Fact]
        public void ContainsName_WhenSetIsEmpty_ShouldReturn_False()
        {
            var set = new SetOfCommandDefinition();
            bool result = set.ContainsName("abc");
            Assert.False(result);
        }

        [Fact]
        public void ContainsName_WhenSetContainsName_ShouldReturn_True()
        {
            var set = new SetOfCommandDefinition();
            set.Add(new CommandDefinition("abc") { Handler = (cmd) => { } });
            bool result = set.ContainsName("abc");
            Assert.True(result);
        }

        [Fact]
        public void ContainsName_WhenNullArgumentProvided_ShouldThrow_ArgumentNullException()
        {
            var set = new SetOfCommandDefinition();
            set.Add(new CommandDefinition("abc") { Handler = (cmd) => { } });
            Action action = () => set.ContainsName(null);
            Assert.Throws<ArgumentNullException>(action);
        }
        #endregion

        #region add
        [Fact]
        public void Add_WhenNullArgumentProvided_ShouldThrow_ArgumentNullException()
        {
            var set = new SetOfCommandDefinition();
            Action action = () => set.Add(null);
            Assert.Throws<ArgumentNullException>(action);
        }

        [Fact]
        public void Add_WhenNameIsDuplicate_ShouldThrow_CommandDefinitionException()
        {
            var set = new SetOfCommandDefinition();
            set.Add(new CommandDefinition("name1") { Handler = (cmd) => { } });
            set.Add(new CommandDefinition("name2") { Handler = (cmd) => { } });
            set.Add(new CommandDefinition("name3") { Handler = (cmd) => { } });
            set.Add(new CommandDefinition("name4") { Handler = (cmd) => { } });
            Action action = () => set.Add(new CommandDefinition("name2") { Handler = (cmd) => { } });
            Assert.Throws<CommandDefinitionException>(action);
        }

        [Fact]
        public void Add_WhenArgumentIsValid_ShouldResultIn_AddedCommandDefinition()
        {
            var set = new SetOfCommandDefinition();
            set.Add(new CommandDefinition("name1") { Handler = (cmd) => { } });
            set.Add(new CommandDefinition("name2") { Handler = (cmd) => { } });
            set.Add(new CommandDefinition("name3") { Handler = (cmd) => { } });
            set.Add(new CommandDefinition("name4") { Handler = (cmd) => { } });

            Assert.Collection<CommandDefinition>(set,
                (x) => Assert.Equal("name1", x.Name),
                (x) => Assert.Equal("name2", x.Name),
                (x) => Assert.Equal("name3", x.Name),
                (x) => Assert.Equal("name4", x.Name)
            );
        }
        #endregion

        #region try get
        [Fact]
        public void TryGet_WhenProvidedNameExists_ShouldReturn_True_And_OutputFoundNamespace()
        {
            var set = new SetOfCommandDefinition();
            set.Add(new CommandDefinition("name1") { Handler = (cmd) => { } });
            set.Add(new CommandDefinition("name2") { Handler = (cmd) => { } });
            set.Add(new CommandDefinition("name3") { Handler = (cmd) => { } });
            set.Add(new CommandDefinition("name4") { Handler = (cmd) => { } });
            set.Add(new CommandDefinition("name5") { Handler = (cmd) => { } });

            Assert.True(set.TryGet("name4", out CommandDefinition cd));
            Assert.Equal("name4", cd.Name);
        }

        [Fact]
        public void TryGet_WhenProvidedNameExists_ShouldReturn_False_And_OutputNull()
        {
            var set = new SetOfCommandDefinition();
            set.Add(new CommandDefinition("name1") { Handler = (cmd) => { } });
            set.Add(new CommandDefinition("name2") { Handler = (cmd) => { } });
            set.Add(new CommandDefinition("name3") { Handler = (cmd) => { } });
            set.Add(new CommandDefinition("name4") { Handler = (cmd) => { } });
            set.Add(new CommandDefinition("name5") { Handler = (cmd) => { } });

            Assert.False(set.TryGet("name6", out CommandDefinition cd));
            Assert.Null(cd);
        }
        #endregion

        #region get descendents
        [Fact]
        public void GetDescendents_WhenOfNamespace_IsNull_ShouldThrow_ArgumentNullException()
        {
            var set = new SetOfCommandDefinition();
            set.Add(new CommandDefinition("htl.name1") { Handler = (cmd) => { } });
            set.Add(new CommandDefinition("htl.name2") { Handler = (cmd) => { } });
            set.Add(new CommandDefinition("htl.name3") { Handler = (cmd) => { } });

            Action action = () => set.GetDescendents(null);
            Assert.Throws<ArgumentNullException>(action);
        }

        [Fact]
        public void GetDescendents_WhenOfNamespace_IsEmpty_ShouldReturn_EmptyArrayOfCommandDefinition()
        {
            var set = new SetOfCommandDefinition();
            set.Add(new CommandDefinition("htl.name1") { Handler = (cmd) => { } });
            set.Add(new CommandDefinition("htl.name2") { Handler = (cmd) => { } });
            set.Add(new CommandDefinition("htl.name3") { Handler = (cmd) => { } });

            Action action = () => set.GetDescendents(null);
            Assert.Throws<ArgumentNullException>(action);
        }

        [Fact]
        public void GetDescendents_ShouldReturn_AllDescendentsOf_ProvidedNamespace()
        {
            var set = new SetOfCommandDefinition();
            set.Add(new CommandDefinition("htl.abc.name1") { Handler = (cmd) => { } });
            set.Add(new CommandDefinition("htl.abc.name2") { Handler = (cmd) => { } });
            set.Add(new CommandDefinition("htl.aaa.name3") { Handler = (cmd) => { } });
            set.Add(new CommandDefinition("htl.xyz.name4") { Handler = (cmd) => { } });
            set.Add(new CommandDefinition("htl.bbb.name5") { Handler = (cmd) => { } });
            set.Add(new CommandDefinition("htl.bbb.name6") { Handler = (cmd) => { } });
            set.Add(new CommandDefinition("htl.aaa.name7") { Handler = (cmd) => { } });
            set.Add(new CommandDefinition("htl.bbb.ccc.name8") { Handler = (cmd) => { } });
            set.Add(new CommandDefinition("htl.bbb.ccc.xxx.name9") { Handler = (cmd) => { } });

            CommandDefinition[] result = set.GetDescendents("htl.bbb");

            Assert.Collection<CommandDefinition>(result,
                (x) => Assert.Equal("htl.bbb.name5", x.Name),
                (x) => Assert.Equal("htl.bbb.name6", x.Name),
                (x) => Assert.Equal("htl.bbb.ccc.name8", x.Name),
                (x) => Assert.Equal("htl.bbb.ccc.xxx.name9", x.Name)
            );
        }

        [Fact]
        public void GetDescendents_ShouldReturn_AllDescendentsOf_ProvidedNamespace_WhenNotHidden()
        {
            var set = new SetOfCommandDefinition();
            set.Add(new CommandDefinition("htl.abc.name1") { Handler = (cmd) => { } });
            set.Add(new CommandDefinition("htl.abc.name2") { Handler = (cmd) => { } });
            set.Add(new CommandDefinition("htl.aaa.name3") { Handler = (cmd) => { } });
            set.Add(new CommandDefinition("htl.xyz.name4") { Handler = (cmd) => { } });
            set.Add(new CommandDefinition("htl.bbb.name5") { Handler = (cmd) => { } });
            set.Add(new CommandDefinition("htl.bbb.name6") { Handler = (cmd) => { } });
            set.Add(new CommandDefinition("htl.aaa.name7") { Handler = (cmd) => { } });
            set.Add(new CommandDefinition("htl.bbb.ccc.name8") { Handler = (cmd) => { } });
            set.Add(new CommandDefinition("htl.bbb.ccc.xxx.name9") { Handler = (cmd) => { } });

            set["htl.bbb.ccc.name8"].Hide();

            CommandDefinition[] result = set.GetDescendents("htl.bbb");

            Assert.Collection<CommandDefinition>(result,
                (x) => Assert.Equal("htl.bbb.name5", x.Name),
                (x) => Assert.Equal("htl.bbb.name6", x.Name),
                //(x) => Assert.Equal("htl.bbb.ccc.name8", x.Name), //hidden
                (x) => Assert.Equal("htl.bbb.ccc.xxx.name9", x.Name)
            );
        }

        [Fact]
        public void GetDescendents_WithIncludeHiddenFlag_ShouldReturn_AllDescendentsOf_ProvidedNamespace_EvenWhenHidden()
        {
            var set = new SetOfCommandDefinition();
            set.Add(new CommandDefinition("htl.abc.name1") { Handler = (cmd) => { } });
            set.Add(new CommandDefinition("htl.abc.name2") { Handler = (cmd) => { } });
            set.Add(new CommandDefinition("htl.aaa.name3") { Handler = (cmd) => { } });
            set.Add(new CommandDefinition("htl.xyz.name4") { Handler = (cmd) => { } });
            set.Add(new CommandDefinition("htl.bbb.name5") { Handler = (cmd) => { } });
            set.Add(new CommandDefinition("htl.bbb.name6") { Handler = (cmd) => { } });
            set.Add(new CommandDefinition("htl.aaa.name7") { Handler = (cmd) => { } });
            set.Add(new CommandDefinition("htl.bbb.ccc.name8") { Handler = (cmd) => { } });
            set.Add(new CommandDefinition("htl.bbb.ccc.xxx.name9") { Handler = (cmd) => { } });

            set["htl.bbb.ccc.name8"].Hide();

            CommandDefinition[] result = set.GetDescendents("htl.bbb", true);

            Assert.Collection<CommandDefinition>(result,
                (x) => Assert.Equal("htl.bbb.name5", x.Name),
                (x) => Assert.Equal("htl.bbb.name6", x.Name),
                (x) => Assert.Equal("htl.bbb.ccc.name8", x.Name), //hidden, still returned
                (x) => Assert.Equal("htl.bbb.ccc.xxx.name9", x.Name)
            );
        }
        #endregion
    }
}
