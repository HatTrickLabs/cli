// SPDX-License-Identifier: Apache-2.0
// Copyright (c) HatTrick Labs, LLC

using Xunit;

namespace HatTrick.CommandLine.Test
{
    [Collection("Sequential")]
    public class DefinitionRegistryTests
    {
        #region get instance
        [Fact]
        public void GetInstance_SingletonInstance_ShouldContain_DefaultCommandDefinition()
        {
            DefinitionRegistry.Clear();
            var registry = DefinitionRegistry.GetInstance();
            CommandDefinition cmdDef = registry.GetCommandDefinition(CommandDefinition.DefaultCommandName);
            Assert.NotNull(cmdDef);
            Assert.Equal(CommandDefinition.DefaultCommandName, cmdDef.Name);
        }
        #endregion

        #region add
        [Fact]
        public void Add_NamespaceDefinition_WhenName_CollidesWith_ExistingCommandName_ShouldThrow_NamespaceDefinitionException()
        {
            DefinitionRegistry.Clear();
            var registry = DefinitionRegistry.GetInstance();
            registry.Add(new CommandDefinition("name") { Handler = (cmd) => { }, Help = "Help!" });
            Action action = () => registry.Add(new NamespaceDefinition("name", "help"));
            Assert.Throws<NamespaceDefinitionException>(action);
        }

        [Fact]
        public void Add_CommandDefinition_WhenName_CollidesWith_ExistingNamespaceName_ShouldThrow_CommandDefinitionException()
        {
            DefinitionRegistry.Clear();
            var registry = DefinitionRegistry.GetInstance();
            registry.Add(new NamespaceDefinition("name", "help"));
            Action action = () => registry.Add(new CommandDefinition("name") { Handler = (cmd) => { } });
            Assert.Throws<CommandDefinitionException>(action);
        }

        [Fact]
        public void Add_NamespaceDefinition_WhenAncestor_CollidesWith_ExistingCommandName_ShouldThrow_NamespaceDefinitionException()
        {
            DefinitionRegistry.Clear();
            var registry = DefinitionRegistry.GetInstance();
            registry.Add(new CommandDefinition("git") { Handler = (cmd) => { }, Help = "Help!" });
            //registering "git.push" would auto-vivify a synthetic namespace "git", colliding with the command
            Action action = () => registry.Add(new NamespaceDefinition("git.push", "help"));
            Assert.Throws<NamespaceDefinitionException>(action);
        }
        #endregion

        #region get command definitions
        [Fact]
        public void GetCommandDefinitions_WhenWherePredicate_IsNull_ShouldReturn_AllCommmandDefinitions()
        {
            DefinitionRegistry.Clear();
            var registry = DefinitionRegistry.GetInstance();
            registry.Add(new CommandDefinition("name1") { Handler = (cmd) => { }, Help = "Help!" });
            registry.Add(new CommandDefinition("name2") { Handler = (cmd) => { }, Help = "Help!" });
            registry.Add(new CommandDefinition("name3") { Handler = (cmd) => { }, Help = "Help!" });
            registry.Add(new CommandDefinition("name4") { Handler = (cmd) => { }, Help = "Help!" });

            var result = registry.GetCommandDefinitions(null);

            Assert.Equal(5, result.Length);

            Assert.Collection<CommandDefinition>(result,
                (x) => Assert.Equal(CommandDefinition.DefaultCommandName, x.Name),
                (x) => Assert.Equal("name1", x.Name),
                (x) => Assert.Equal("name2", x.Name),
                (x) => Assert.Equal("name3", x.Name),
                (x) => Assert.Equal("name4", x.Name)
            );
        }

        [Fact]
        public void GetCommandDefinitions_WhenWherePredicate_IsNotNull_ShouldReturn_CommandDefinitionsMatchingPredicate()
        {
            DefinitionRegistry.Clear();
            var registry = DefinitionRegistry.GetInstance();
            registry.Add(new CommandDefinition("abc1")  { Handler = (cmd) => { }, Help = "Help!" });
            registry.Add(new CommandDefinition("name2") { Handler = (cmd) => { }, Help = "Help!" });
            registry.Add(new CommandDefinition("name3") { Handler = (cmd) => { }, Help = "Help!" });
            registry.Add(new CommandDefinition("abc4")  { Handler = (cmd) => { }, Help = "Help!" });

            var result = registry.GetCommandDefinitions((cmd) => cmd.Name.StartsWith("abc"));

            Assert.Equal(2, result.Length);

            Assert.Collection<CommandDefinition>(result,
                (x) => Assert.Equal("abc1", x.Name),
                (x) => Assert.Equal("abc4", x.Name)
            );
        }
        #endregion

        #region get namespace definitions
        [Fact]
        public void GetNamespaceDefinitions_WhenWherePredicate_IsNull_ShouldReturn_AllNamespaceDefinitions()
        {
            DefinitionRegistry.Clear();
            var registry = DefinitionRegistry.GetInstance();
            registry.Add(new NamespaceDefinition("name1", "help"));
            registry.Add(new NamespaceDefinition("name2", "help"));
            registry.Add(new NamespaceDefinition("name3", "help"));
            registry.Add(new NamespaceDefinition("name4", "help"));

            var result = registry.GetNamespaceDefinitions(null);

            Assert.Equal(4, result.Length);

            Assert.Collection(result,
                (x) => Assert.Equal("name1", x.Name),
                (x) => Assert.Equal("name2", x.Name),
                (x) => Assert.Equal("name3", x.Name),
                (x) => Assert.Equal("name4", x.Name)
            );
        }

        [Fact]
        public void GetNamespaceDefinitions_WhenWherePredicate_IsNotNull_ShouldReturn_NamespaceDefinitionsMatchingPredicate()
        {
            DefinitionRegistry.Clear();
            var registry = DefinitionRegistry.GetInstance();
            registry.Add(new NamespaceDefinition("abc1", "help"));
            registry.Add(new NamespaceDefinition("name2", "help"));
            registry.Add(new NamespaceDefinition("name3", "help"));
            registry.Add(new NamespaceDefinition("abc4", "help"));

            var result = registry.GetNamespaceDefinitions((ns) => ns.Name.StartsWith("abc"));

            Assert.Equal(2, result.Length);

            Assert.Collection(result,
                (x) => Assert.Equal("abc1", x.Name),
                (x) => Assert.Equal("abc4", x.Name)
            );
        }
        #endregion

        #region try get namespace definition
        [Fact]
        public void TryGetNamespaceDefinition_ShouldReturn_False_WhenNotFound_ShouldReturn_True_WhenFound()
        {
            DefinitionRegistry.Clear();
            var registry = DefinitionRegistry.GetInstance();
            registry.Add(new NamespaceDefinition("name1", "help"));
            registry.Add(new NamespaceDefinition("name2", "help"));
            registry.Add(new NamespaceDefinition("name3", "help"));
            registry.Add(new NamespaceDefinition("name4", "help"));

            bool result1 = registry.TryGetNamespaceDefinition("name3", out NamespaceDefinition ns1);
            bool result2 = registry.TryGetNamespaceDefinition("name5", out NamespaceDefinition ns2);

            Assert.True(result1);
            Assert.False(result2);

            Assert.Equal("name3", ns1.Name);
            Assert.Null(ns2);
        }
        #endregion

        #region try get command definition
        [Fact]
        public void TryGetCommandDefinition_ShouldReturn_False_WhenNotFound_ShouldReturn_True_WhenFound()
        {
            DefinitionRegistry.Clear();
            var registry = DefinitionRegistry.GetInstance();
            registry.Add(new CommandDefinition("name1") { Handler = (cmd) => { }, Help = "Help!" });
            registry.Add(new CommandDefinition("name2") { Handler = (cmd) => { }, Help = "Help!" });
            registry.Add(new CommandDefinition("name3") { Handler = (cmd) => { }, Help = "Help!" });
            registry.Add(new CommandDefinition("name4") { Handler = (cmd) => { }, Help = "Help!" });

            bool result1 = registry.TryGetCommandDefinition("name3", out CommandDefinition cmd1);
            bool result2 = registry.TryGetCommandDefinition("name5", out CommandDefinition cmd2);

            Assert.True(result1);
            Assert.False(result2);

            Assert.Equal("name3", cmd1.Name);
            Assert.Null(cmd2);
        }
        #endregion

        #region get child command definitions
        [Fact]
        public void GetChildCommandDefinitions_WhenNullArgumentProvided_ShouldThrow_ArgumentNullException()
        {
            DefinitionRegistry.Clear();
            var registry = DefinitionRegistry.GetInstance();
            registry.Add(new NamespaceDefinition("aaa", "help"));
            registry.Add(new NamespaceDefinition("bbb", "help"));
            registry.Add(new NamespaceDefinition("ccc", "help"));
            registry.Add(new NamespaceDefinition("aaa.xyz", "help"));

            registry.Add(new CommandDefinition("aaa.cmd1") { Handler = (cmd) => { }, Help = "Help!" });
            registry.Add(new CommandDefinition("aaa.cmd2") { Handler = (cmd) => { }, Help = "Help!" });
            registry.Add(new CommandDefinition("bbb.cmd1") { Handler = (cmd) => { }, Help = "Help!" });
            registry.Add(new CommandDefinition("bbb.cmd2") { Handler = (cmd) => { }, Help = "Help!" });

            Action action = () => registry.GetChildCommandDefinitions(null);
            Assert.Throws<ArgumentNullException>(action);
        }

        [Fact]
        public void GetChildCommandDefinitions_ShouldReturn_DirectChildDescedents_OfProvidedNamespace()
        {
            DefinitionRegistry.Clear();
            var registry = DefinitionRegistry.GetInstance();
            registry.Add(new NamespaceDefinition("aaa", "help"));
            registry.Add(new NamespaceDefinition("bbb", "help"));
            registry.Add(new NamespaceDefinition("ccc", "help"));
            var ns = new NamespaceDefinition("aaa.xyz", "help");
            registry.Add(ns);

            registry.Add(new CommandDefinition("aaa.cmd1") { Handler = (cmd) => { }, Help = "Help!" });
            registry.Add(new CommandDefinition("aaa.cmd2") { Handler = (cmd) => { }, Help = "Help!" });
            registry.Add(new CommandDefinition("bbb.cmd1") { Handler = (cmd) => { }, Help = "Help!" });
            registry.Add(new CommandDefinition("bbb.cmd2") { Handler = (cmd) => { }, Help = "Help!" });
            registry.Add(new CommandDefinition("ccc.cmd1") { Handler = (cmd) => { }, Help = "Help!" });
            registry.Add(new CommandDefinition("ccc.cmd2") { Handler = (cmd) => { }, Help = "Help!" });
            registry.Add(new CommandDefinition("aaa.xyz.cmd1") { Handler = (cmd) => { }, Help = "Help!" });
            registry.Add(new CommandDefinition("aaa.xyz.cmd2") { Handler = (cmd) => { }, Help = "Help!" });

            CommandDefinition[] result = registry.GetChildCommandDefinitions(ns);

            Assert.Equal(2, result.Length);
            Assert.Collection<CommandDefinition>(result,
                (x) => Assert.Equal("aaa.xyz.cmd1", x.Name),
                (x) => Assert.Equal("aaa.xyz.cmd2", x.Name)
            );
        }
        #endregion

        #region get descendent command definitions
        [Fact]
        public void GetDescendentCommandDefinitions_WhenNullArgumentProvided_ShouldThrow_ArgumentNullException()
        {
            DefinitionRegistry.Clear();
            var registry = DefinitionRegistry.GetInstance();
            registry.Add(new NamespaceDefinition("aaa", "help"));
            registry.Add(new NamespaceDefinition("bbb", "help"));
            registry.Add(new NamespaceDefinition("ccc", "help"));
            registry.Add(new NamespaceDefinition("aaa.xyz", "help"));

            registry.Add(new CommandDefinition("aaa.cmd1") { Handler = (cmd) => { }, Help = "Help!" });
            registry.Add(new CommandDefinition("aaa.cmd2") { Handler = (cmd) => { }, Help = "Help!" });
            registry.Add(new CommandDefinition("bbb.cmd1") { Handler = (cmd) => { }, Help = "Help!" });
            registry.Add(new CommandDefinition("bbb.cmd2") { Handler = (cmd) => { }, Help = "Help!" });

            Action action = () => registry.GetDescendentCommandDefinitions(null);
            Assert.Throws<ArgumentNullException>(action);
        }

        [Fact]
        public void GetDescendentCommandDefinitions_ShouldReturn_AllDescedents_OfProvidedNamespace()
        {
            DefinitionRegistry.Clear();
            var registry = DefinitionRegistry.GetInstance();
            var ns = new NamespaceDefinition("aaa", "help");
            registry.Add(ns);
            registry.Add(new NamespaceDefinition("bbb", "help"));
            registry.Add(new NamespaceDefinition("ccc", "help"));
            registry.Add(new NamespaceDefinition("aaa.xyz", "help"));

            registry.Add(new CommandDefinition("aaa.cmd1") { Handler = (cmd) => { }, Help = "Help!" });
            registry.Add(new CommandDefinition("aaa.cmd2") { Handler = (cmd) => { }, Help = "Help!" });
            registry.Add(new CommandDefinition("bbb.cmd1") { Handler = (cmd) => { }, Help = "Help!" });
            registry.Add(new CommandDefinition("bbb.cmd2") { Handler = (cmd) => { }, Help = "Help!" });
            registry.Add(new CommandDefinition("ccc.cmd1") { Handler = (cmd) => { }, Help = "Help!" });
            registry.Add(new CommandDefinition("ccc.cmd2") { Handler = (cmd) => { }, Help = "Help!" });
            registry.Add(new CommandDefinition("aaa.xyz.cmd1") { Handler = (cmd) => { }, Help = "Help!" });
            registry.Add(new CommandDefinition("aaa.xyz.cmd2") { Handler = (cmd) => { }, Help = "Help!" });

            CommandDefinition[] result = registry.GetDescendentCommandDefinitions(ns);

            Assert.Equal(4, result.Length);
            Assert.Collection<CommandDefinition>(result,
                (x) => Assert.Equal("aaa.cmd1", x.Name),
                (x) => Assert.Equal("aaa.cmd2", x.Name),
                (x) => Assert.Equal("aaa.xyz.cmd1", x.Name),
                (x) => Assert.Equal("aaa.xyz.cmd2", x.Name)
            );
        }
        #endregion
    }
}
