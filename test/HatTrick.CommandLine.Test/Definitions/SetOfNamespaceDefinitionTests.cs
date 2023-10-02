using Xunit;

namespace HatTrick.CommandLine.Test
{
    public class SetOfNamespaceDefinitionTests
    {
        #region namespace indexer this[string key]
        [Fact]
        public void IndexerOnKeyString_WhenKeyExists_ShouldReturn_IndexedNamespace()
        {
            var set = new SetOfNamespaceDefinition();
            set.Add(new(name: "abc", help: "help"));
            set.Add(new(name: "abcd", help: "help"));
            set.Add(new(name: "abcde", help: "help"));
            set.Add(new(name: "abcdef", help: "help"));

            NamespaceDefinition ns = set["abcde"];

            Assert.Equal("abcde", ns.Name);
        }

        [Fact]
        public void IndexerOnKeyString_WhenKeyDoesNotExists_ShouldThrow_KeyNotFoundException()
        {
            var set = new SetOfNamespaceDefinition();
            set.Add(new(name: "abc", help: "help"));
            set.Add(new(name: "abcd", help: "help"));
            set.Add(new(name: "abcde", help: "help"));
            set.Add(new(name: "abcdef", help: "help"));

            NamespaceDefinition? ns = null;
            Action action = () => ns = set["abcdefg"];
            Assert.Throws<KeyNotFoundException>(action);
        }
        #endregion

        #region constains name
        [Fact]
        public void ContainsName_WhenSetIsEmpty_ShouldReturn_False()
        {
            var set = new SetOfNamespaceDefinition();
            bool result = set.ContainsName("abc");
            Assert.False(result);
        }

        [Fact]
        public void ContainsName_WhenSetContainsName_ShouldReturn_True()
        {
            var set = new SetOfNamespaceDefinition();
            set.Add(new NamespaceDefinition("abc", "xyz"));
            bool result = set.ContainsName("abc");
            Assert.True(result);
        }

        [Fact]
        public void ContainsName_WhenNullArgumentProvided_ShouldThrow_ArgumentNullException()
        {
            var set = new SetOfNamespaceDefinition();
            set.Add(new NamespaceDefinition("abc", "xyz"));
            Action action = () => set.ContainsName(null);
            Assert.Throws<ArgumentNullException>(action);
        }
        #endregion

        #region add
        [Fact]
        public void Add_WhenNullArgumentProvided_ShouldThrow_ArgumentNullException()
        {
            var set = new SetOfNamespaceDefinition();
            Action action = () => set.Add(null);
            Assert.Throws<ArgumentNullException>(action);
            return;
        }

        [Fact]
        public void Add_WhenNameHasSegmentGap_ShouldThrow_NamespaceDefinition()
        {
            var set = new SetOfNamespaceDefinition();
            set.Add(new NamespaceDefinition("system", "help"));
            set.Add(new NamespaceDefinition("system.io", "help"));
            set.Add(new NamespaceDefinition("system.io.access", "help"));


            Action action = () => set.Add(new NamespaceDefinition("system.io.access.output.encoding", "help"));
            Assert.Throws<NamespaceDefinitionException>(action);
            return;
        }

        [Fact]
        public void Add_WhenNameIsDuplicate_ShouldThrow_NamespaceDefinitionException()
        {
            var set = new SetOfNamespaceDefinition();
            set.Add(new NamespaceDefinition("system", "help"));
            set.Add(new NamespaceDefinition("system.io", "help"));

            Action action = () => set.Add(new NamespaceDefinition("system.io", "help"));
            Assert.Throws<NamespaceDefinitionException>(action);
            return;
        }
        #endregion

        #region try get
        [Fact]
        public void TryGet_WhenProvidedNameExists_ShouldReturn_True_And_OutputFoundNamespace()
        {
            var set = new SetOfNamespaceDefinition();
            set.Add(new NamespaceDefinition("name1", "help1"));
            set.Add(new NamespaceDefinition("name2", "help2"));
            set.Add(new NamespaceDefinition("name3", "help3"));
            set.Add(new NamespaceDefinition("name4", "help4"));
            set.Add(new NamespaceDefinition("name5", "help5"));

            Assert.True(set.TryGet("name4", out NamespaceDefinition ns));
            Assert.Equal("name4", ns.Name);
        }

        [Fact]
        public void TryGet_WhenProvidedNameExists_ShouldReturn_False_And_OutputNull()
        {
            var set = new SetOfNamespaceDefinition();
            set.Add(new NamespaceDefinition("name1", "help1"));
            set.Add(new NamespaceDefinition("name2", "help2"));
            set.Add(new NamespaceDefinition("name3", "help3"));
            set.Add(new NamespaceDefinition("name4", "help4"));
            set.Add(new NamespaceDefinition("name5", "help5"));

            Assert.False(set.TryGet("name6", out NamespaceDefinition ns));
            Assert.Null(ns);
        }
        #endregion

        #region get roots
        [Fact]
        public void GetRoots_ShouldReturn_AllItems_DepthEqualTo_Zero()
        {
            var set = new SetOfNamespaceDefinition();
            set.Add(new NamespaceDefinition("name1", "help"));
            set.Add(new NamespaceDefinition("name2", "help"));
            set.Add(new NamespaceDefinition("abc", "help"));
            set.Add(new NamespaceDefinition("abc.name3", "help"));
            set.Add(new NamespaceDefinition("abc.name4", "help"));
            set.Add(new NamespaceDefinition("abc.xyz", "help"));
            set.Add(new NamespaceDefinition("abc.xyz.name5", "help"));

            NamespaceDefinition[] rootSet = set.GetRoots();

            Assert.Equal(3, rootSet.Length);
            Assert.Collection(rootSet,
                (ns) => { Assert.Equal(0, ns.Depth); Assert.Equal("name1", ns.Name); },
                (ns) => { Assert.Equal(0, ns.Depth); Assert.Equal("name2", ns.Name); },
                (ns) => { Assert.Equal(0, ns.Depth); Assert.Equal("abc", ns.Name); }
            );
        }

        [Fact]
        public void GetRoots_ShouldReturn_AllItems_NotHidden_DepthEqualTo_Zero()
        {
            var set = new SetOfNamespaceDefinition();
            set.Add(new NamespaceDefinition("name1", "help"));
            set.Add(new NamespaceDefinition("name2", "help"));
            set.Add(new NamespaceDefinition("abc", "help"));
            set.Add(new NamespaceDefinition("abc.name3", "help"));
            set.Add(new NamespaceDefinition("abc.name4", "help"));
            set.Add(new NamespaceDefinition("abc.xyz", "help"));
            set.Add(new NamespaceDefinition("abc.xyz.name5", "help"));

            //hide name2
            set["name2"].Hide();

            NamespaceDefinition[] rootSet = set.GetRoots();

            Assert.Equal(2, rootSet.Length);
            Assert.Collection(rootSet,
                (ns) => { Assert.Equal(0, ns.Depth); Assert.Equal("name1", ns.Name); },
                (ns) => { Assert.Equal(0, ns.Depth); Assert.Equal("abc", ns.Name); }
            );
        }

        [Fact]
        public void GetRoots_WhenIncludeHiddenFlagEnabled_ShouldReturn_AllItems_IncludingHidden_DepthEqualTo_Zero()
        {
            var set = new SetOfNamespaceDefinition();
            set.Add(new NamespaceDefinition("name1", "help"));
            set.Add(new NamespaceDefinition("name2", "help"));
            set.Add(new NamespaceDefinition("abc", "help"));
            set.Add(new NamespaceDefinition("abc.name3", "help"));
            set.Add(new NamespaceDefinition("abc.name4", "help"));
            set.Add(new NamespaceDefinition("abc.xyz", "help"));
            set.Add(new NamespaceDefinition("abc.xyz.name5", "help"));

            //hide name2
            set["name2"].Hide();

            NamespaceDefinition[] rootSet = set.GetRoots(includeHidden: true);

            Assert.Equal(3, rootSet.Length);
            Assert.Collection(rootSet,
                (ns) => { Assert.Equal(0, ns.Depth); Assert.Equal("name1", ns.Name); },
                (ns) => { Assert.Equal(0, ns.Depth); Assert.Equal("name2", ns.Name); },
                (ns) => { Assert.Equal(0, ns.Depth); Assert.Equal("abc", ns.Name); }
            );
        }
        #endregion

        #region get children
        [Fact]
        public void GetChildren_WhenParentArgument_IsNull_ShouldThrow_ArgumentNullException()
        {
            var set = new SetOfNamespaceDefinition();
            set.Add(new NamespaceDefinition("abc", "help"));
            set.Add(new NamespaceDefinition("abc.name3", "help"));
            set.Add(new NamespaceDefinition("abc.name4", "help"));

            Action action = () => set.GetChildren(null);

            Assert.Throws<ArgumentNullException>(action);

        }

        [Fact]
        public void GetChildren_ShouldReturn_AllChildItems_OfProvidedParent()
        {
            var set = new SetOfNamespaceDefinition();
            set.Add(new NamespaceDefinition("name1", "help"));
            set.Add(new NamespaceDefinition("name2", "help"));
            set.Add(new NamespaceDefinition("abc", "help"));
            set.Add(new NamespaceDefinition("abc.name3", "help"));
            set.Add(new NamespaceDefinition("abc.name4", "help"));
            set.Add(new NamespaceDefinition("abc.xyz", "help"));
            set.Add(new NamespaceDefinition("abc.xyz.name5", "help"));

            NamespaceDefinition parent = set["abc"];//depth 0
            NamespaceDefinition[] childSet = set.GetChildren(parent);

            Assert.Equal(3, childSet.Length);
            Assert.Collection(childSet,
                (ns) => { Assert.Equal(1, ns.Depth); Assert.Equal("abc.name3", ns.Name); },
                (ns) => { Assert.Equal(1, ns.Depth); Assert.Equal("abc.name4", ns.Name); },
                (ns) => { Assert.Equal(1, ns.Depth); Assert.Equal("abc.xyz", ns.Name); }
            );
        }

        [Fact]
        public void GetChildren_ShouldReturn_All_NotHidden_ChildItems_OfProvidedParent()
        {
            var set = new SetOfNamespaceDefinition();
            set.Add(new NamespaceDefinition("name1", "help"));
            set.Add(new NamespaceDefinition("name2", "help"));
            set.Add(new NamespaceDefinition("abc", "help"));
            set.Add(new NamespaceDefinition("abc.name3", "help"));
            set.Add(new NamespaceDefinition("abc.name4", "help"));
            set.Add(new NamespaceDefinition("abc.xyz", "help"));
            set.Add(new NamespaceDefinition("abc.xyz.name5", "help"));

            //hide abc.name4
            set["abc.name4"].Hide();

            NamespaceDefinition parent = set["abc"];//depth 0
            NamespaceDefinition[] childSet = set.GetChildren(parent);

            Assert.Equal(2, childSet.Length);
            Assert.Collection(childSet,
                (ns) => { Assert.Equal(1, ns.Depth); Assert.Equal("abc.name3", ns.Name); },
                (ns) => { Assert.Equal(1, ns.Depth); Assert.Equal("abc.xyz", ns.Name); }
            );
        }

        [Fact]
        public void GetChildren_WhenIncludeHiddenFlagEnabled_ShouldReturn_All_ChildItems_IncludingHidden_OfProvidedParent()
        {
            var set = new SetOfNamespaceDefinition();
            set.Add(new NamespaceDefinition("name1", "help"));
            set.Add(new NamespaceDefinition("name2", "help"));
            set.Add(new NamespaceDefinition("abc", "help"));
            set.Add(new NamespaceDefinition("abc.name3", "help"));
            set.Add(new NamespaceDefinition("abc.name4", "help"));
            set.Add(new NamespaceDefinition("abc.xyz", "help"));
            set.Add(new NamespaceDefinition("abc.xyz.name5", "help"));

            //hide abc.name4
            set["abc.name4"].Hide();

            NamespaceDefinition parent = set["abc"];//depth 0
            NamespaceDefinition[] childSet = set.GetChildren(parent: parent, includeHidden: true);

            Assert.Equal(3, childSet.Length);
            Assert.Collection(childSet,
                (ns) => { Assert.Equal(1, ns.Depth); Assert.Equal("abc.name3", ns.Name); },
                (ns) => { Assert.Equal(1, ns.Depth); Assert.Equal("abc.name4", ns.Name); },
                (ns) => { Assert.Equal(1, ns.Depth); Assert.Equal("abc.xyz", ns.Name); }
            );
        }
        #endregion

        #region get descendents
        [Fact]
        public void GetDescendents_WhenParentArgument_IsNull_ShouldThrow_ArgumentNullException()
        {
            var set = new SetOfNamespaceDefinition();
            set.Add(new NamespaceDefinition("abc", "help"));
            set.Add(new NamespaceDefinition("abc.name3", "help"));
            set.Add(new NamespaceDefinition("abc.name4", "help"));

            Action action = () => set.GetDescendents(null);

            Assert.Throws<ArgumentNullException>(action);

        }

        [Fact]
        public void GetDescendents_ShouldReturn_AllDescendentItems_OfProvidedParent()
        {
            var set = new SetOfNamespaceDefinition();
            set.Add(new NamespaceDefinition("name1", "help"));
            set.Add(new NamespaceDefinition("name2", "help"));
            set.Add(new NamespaceDefinition("abc", "help"));
            set.Add(new NamespaceDefinition("abc.name3", "help"));
            set.Add(new NamespaceDefinition("abc.name4", "help"));
            set.Add(new NamespaceDefinition("abc.xyz", "help"));
            set.Add(new NamespaceDefinition("abc.xyz.name5", "help"));

            NamespaceDefinition parent = set["abc"];//depth 0
            NamespaceDefinition[] childSet = set.GetDescendents(parent);

            Assert.Equal(4, childSet.Length);
            Assert.Collection(childSet,
                (ns) => { Assert.True(ns.Depth > parent.Depth); Assert.Equal("abc.name3", ns.Name); },
                (ns) => { Assert.True(ns.Depth > parent.Depth); Assert.Equal("abc.name4", ns.Name); },
                (ns) => { Assert.True(ns.Depth > parent.Depth); Assert.Equal("abc.xyz", ns.Name); },
                (ns) => { Assert.True(ns.Depth > parent.Depth); Assert.Equal("abc.xyz.name5", ns.Name); }
            );
        }

        [Fact]
        public void GetDescendents_ShouldReturn_All_NotHidden_DescendentItems_OfProvidedParent()
        {
            var set = new SetOfNamespaceDefinition();
            set.Add(new NamespaceDefinition("name1", "help"));
            set.Add(new NamespaceDefinition("name2", "help"));
            set.Add(new NamespaceDefinition("abc", "help"));
            set.Add(new NamespaceDefinition("abc.name3", "help"));
            set.Add(new NamespaceDefinition("abc.name4", "help"));
            set.Add(new NamespaceDefinition("abc.xyz", "help"));
            set.Add(new NamespaceDefinition("abc.xyz.name5", "help"));

            //hide abc.name4
            set["abc.name4"].Hide();

            NamespaceDefinition parent = set["abc"];//depth 0
            NamespaceDefinition[] childSet = set.GetDescendents(parent);

            Assert.Equal(3, childSet.Length);
            Assert.Collection(childSet,
                (ns) => { Assert.True(ns.Depth > parent.Depth); Assert.Equal("abc.name3", ns.Name); },
                (ns) => { Assert.True(ns.Depth > parent.Depth); Assert.Equal("abc.xyz", ns.Name); },
                (ns) => { Assert.True(ns.Depth > parent.Depth); Assert.Equal("abc.xyz.name5", ns.Name); }
            );
        }

        [Fact]
        public void GetDescendents_WhenIncludeHiddenFlagEnabled_ShouldReturn_All_DescendentItems_IncludingHidden_OfProvidedParent()
        {
            var set = new SetOfNamespaceDefinition();
            set.Add(new NamespaceDefinition("name1", "help"));
            set.Add(new NamespaceDefinition("name2", "help"));
            set.Add(new NamespaceDefinition("abc", "help"));
            set.Add(new NamespaceDefinition("abc.name3", "help"));
            set.Add(new NamespaceDefinition("abc.name4", "help"));
            set.Add(new NamespaceDefinition("abc.xyz", "help"));
            set.Add(new NamespaceDefinition("abc.xyz.name5", "help"));

            //hide abc.name4
            set["abc.name4"].Hide();

            NamespaceDefinition parent = set["abc"];//depth 0
            NamespaceDefinition[] childSet = set.GetDescendents(parent: parent, includeHidden: true);

            Assert.Equal(4, childSet.Length);
            Assert.Collection(childSet,
                (ns) => { Assert.True(ns.Depth > parent.Depth); Assert.Equal("abc.name3", ns.Name); },
                (ns) => { Assert.True(ns.Depth > parent.Depth); Assert.Equal("abc.name4", ns.Name); },
                (ns) => { Assert.True(ns.Depth > parent.Depth); Assert.Equal("abc.xyz", ns.Name); },
                (ns) => { Assert.True(ns.Depth > parent.Depth); Assert.Equal("abc.xyz.name5", ns.Name); }
            );
        }
        #endregion
    }
}
