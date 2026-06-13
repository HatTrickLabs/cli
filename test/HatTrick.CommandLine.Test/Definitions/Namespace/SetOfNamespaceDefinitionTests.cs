using Xunit;

namespace HatTrick.CommandLine.Test
{
    public class SetOfNamespaceDefinitionTests
    {
        #region indexer this[string key]
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
        public void Add_WhenNameHasSegmentGap_ShouldAutoVivify_SyntheticAncestor()
        {
            var set = new SetOfNamespaceDefinition();
            set.Add(new NamespaceDefinition("system", "help"));
            set.Add(new NamespaceDefinition("system.io", "help"));
            set.Add(new NamespaceDefinition("system.io.access", "help"));

            //"system.io.access.output" is missing — it should be auto-created as a synthetic placeholder
            set.Add(new NamespaceDefinition("system.io.access.output.encoding", "help"));

            Assert.True(set.ContainsName("system.io.access.output"));
            Assert.True(set["system.io.access.output"].Synthetic);
            Assert.Null(set["system.io.access.output"].Help);

            Assert.True(set.ContainsName("system.io.access.output.encoding"));
            Assert.False(set["system.io.access.output.encoding"].Synthetic);
        }

        [Fact]
        public void Add_WhenAncestorsMissing_ShouldCreate_SyntheticAncestors()
        {
            var set = new SetOfNamespaceDefinition();
            set.Add(new NamespaceDefinition("a.b.c", "help"));

            Assert.True(set["a"].Synthetic);
            Assert.Null(set["a"].Help);
            Assert.True(set["a.b"].Synthetic);
            Assert.Null(set["a.b"].Help);
            Assert.False(set["a.b.c"].Synthetic);
            Assert.Equal("help", set["a.b.c"].Help);
        }

        [Fact]
        public void Add_RealNamespace_WhenSyntheticPlaceholderExists_ShouldPromote()
        {
            var set = new SetOfNamespaceDefinition();
            set.Add(new NamespaceDefinition("a.b.c", "help"));    //auto-creates synthetic "a" and "a.b"

            Assert.True(set["a.b"].Synthetic);

            set.Add(new NamespaceDefinition("a.b", "real help")); //promotes the placeholder

            Assert.False(set["a.b"].Synthetic);
            Assert.Equal("real help", set["a.b"].Help);
            //promotion must not create a duplicate node
            Assert.Single(set.GetDescendants(set["a"]), ns => ns.Name == "a.b");
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

        [Fact]
        public void Add_WhenArgumentIsValid_ShouldResultIn_AddedNamespaceDefinition()
        {
            var set = new SetOfNamespaceDefinition();
            set.Add(new NamespaceDefinition("name1", "help"));
            set.Add(new NamespaceDefinition("name2", "help"));
            set.Add(new NamespaceDefinition("name3", "help"));
            set.Add(new NamespaceDefinition("name4", "help"));

            Assert.Collection<NamespaceDefinition>(set,
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

            Action action = () => set.GetDescendants(null);

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
            NamespaceDefinition[] childSet = set.GetDescendants(parent);

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
            NamespaceDefinition[] childSet = set.GetDescendants(parent);

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
            NamespaceDefinition[] childSet = set.GetDescendants(parent: parent, includeHidden: true);

            Assert.Equal(4, childSet.Length);
            Assert.Collection(childSet,
                (ns) => { Assert.True(ns.Depth > parent.Depth); Assert.Equal("abc.name3", ns.Name); },
                (ns) => { Assert.True(ns.Depth > parent.Depth); Assert.Equal("abc.name4", ns.Name); },
                (ns) => { Assert.True(ns.Depth > parent.Depth); Assert.Equal("abc.xyz", ns.Name); },
                (ns) => { Assert.True(ns.Depth > parent.Depth); Assert.Equal("abc.xyz.name5", ns.Name); }
            );
        }
        #endregion

        #region get ancestors
        [Fact]
        public void GetAncestors_ShouldReturn_OnlyPrefixAncestors_OfProvidedCommand()
        {
            var set = new SetOfNamespaceDefinition();
            set.Add(new NamespaceDefinition("name1", "help"));      //depth 0, not an ancestor
            set.Add(new NamespaceDefinition("ab", "help"));         //depth 0, sibling-prefix of "abc", not an ancestor
            set.Add(new NamespaceDefinition("abc", "help"));        //depth 0, ancestor
            set.Add(new NamespaceDefinition("abc.other", "help"));  //depth 1, shallower but not a prefix
            set.Add(new NamespaceDefinition("abc.xyz", "help"));    //depth 1, ancestor

            var cmd = new CommandDefinition("abc.xyz.connect") { Handler = (c) => { }, Help = "help" };
            cmd.Validate();//computes depth (2)

            NamespaceDefinition[] ancestors = set.GetAncestors(cmd);

            Assert.Equal(2, ancestors.Length);
            Assert.Collection(ancestors,
                (ns) => Assert.Equal("abc", ns.Name),
                (ns) => Assert.Equal("abc.xyz", ns.Name)
            );
        }
        #endregion

        #region segment-aware prefix matching
        [Fact]
        public void GetChildren_ShouldExclude_SiblingPrefixNamespaces()
        {
            var set = new SetOfNamespaceDefinition();
            set.Add(new NamespaceDefinition("abc", "help"));       //depth 0, parent
            set.Add(new NamespaceDefinition("abc.name3", "help")); //depth 1, real child
            set.Add(new NamespaceDefinition("abcd", "help"));      //depth 0, sibling-prefix
            set.Add(new NamespaceDefinition("abcd.xyz", "help"));  //depth 1, child of abcd (NOT abc)

            NamespaceDefinition[] children = set.GetChildren(set["abc"]);

            Assert.Single(children);
            Assert.Equal("abc.name3", children[0].Name);
        }

        [Fact]
        public void GetDescendants_ShouldExclude_SiblingPrefixNamespaces()
        {
            var set = new SetOfNamespaceDefinition();
            set.Add(new NamespaceDefinition("abc", "help"));       //depth 0, parent
            set.Add(new NamespaceDefinition("abc.name3", "help")); //depth 1, real descendant
            set.Add(new NamespaceDefinition("abcd", "help"));      //depth 0, sibling-prefix
            set.Add(new NamespaceDefinition("abcd.xyz", "help"));  //depth 1, descendant of abcd (NOT abc)

            NamespaceDefinition[] descendants = set.GetDescendants(set["abc"]);

            Assert.Single(descendants);
            Assert.Equal("abc.name3", descendants[0].Name);
        }
        #endregion
    }
}
