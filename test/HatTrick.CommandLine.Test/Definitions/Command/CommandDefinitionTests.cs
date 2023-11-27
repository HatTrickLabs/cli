using Xunit;

namespace HatTrick.CommandLine.Test
{
    public class CommandDefinitionTests
    {
        #region constructor
        [Fact]
        public void Constructor_WhenNameArgumentIsNull_ShouldThrow_ArgumentNullException()
        {
            Action action = () => new CommandDefinition(null);
            Assert.Throws<ArgumentNullException>(action);
        }
        #endregion

        #region hide
        [Fact]
        public void Hide_ShouldMarkDefinition_Hidden()
        {
            var cmdDef = new CommandDefinition("abc");
            cmdDef.Hide();
            Assert.True(cmdDef.Hidden);
        }
        #endregion

        #region is valid command name char
        [Fact]
        public void IsValidCommandNameChar_WhenValidCharProvided_ShouldReturnTrue()
        {
            //45 -
            //46 .
            //48 0
            //57 9
            //65 A
            //97 a
            //122 z
            for (int i = 45; i <= 122; i++)
            {
                char c = (char)i;
                Assert.True(CommandDefinition.IsValidCommandNameChar(c));

                if (i == 46)
                    i = 47;

                if (i == 57)
                    i = 64;

                if (i == 90)
                    i = 96;
            }
        }

        [Fact]
        public void IsValidCommandNameChar_WhenInValidCharProvided_ShouldReturnFalse()
        {

            char c = '$';
            Assert.False(CommandDefinition.IsValidCommandNameChar(c));
        }
        #endregion

        #region option indexer this[string key]
        [Fact]
        public void IndexerOnKeyString_WhenOptionExists_ShouldReturn_IndexedOption()
        {
            var cmdDef = new CommandDefinition("abc");
            cmdDef.AddOption<string>(key: "key", help: "help", flags: (terse: "-k", verbose: "--key"));
            OptionDefinition opDef = cmdDef["key"];
            Assert.Equal("key", opDef.Key);
        }

        [Fact]
        public void IndexerOnKeyString_WhenOptionDoesNotExists_ShouldThrow_KeyNotFoundException()
        {
            var cmdDef = new CommandDefinition("abc");
            cmdDef.AddOption<string>(key: "key", help: "help", flags: (terse: "-k", verbose: "--key"));
            OptionDefinition? opDef = null;
            Action action = () => opDef = cmdDef["not-key"];
            Assert.Throws<KeyNotFoundException>(action);
        }
        #endregion

        #region add option of T
        [Fact]
        public void AddOptionOfT_ForSignature_Key_Help_Flags()
        {
            var cmdDef = new CommandDefinition("abc");
            cmdDef.AddOption<string>(key: "key", help: "help", flags: (terse: "-k", verbose: "--key"));
            OptionDefinition opDef = cmdDef["key"];

            Assert.Equal(typeof(string), opDef.GenericType);
            Assert.Equal("key", opDef.Key);
            Assert.Equal("help", opDef.Help);
            Assert.Equal("-k", opDef.Flags.Terse);
            Assert.Equal("--key", opDef.Flags.Verbose);
            Assert.True(opDef.HasConstraints);
            Assert.False(opDef.HasDefault);
        }

        [Fact]
        public void AddOptionOfT_ForSignature_Key_Default_Help_Flags()
        {
            var cmdDef = new CommandDefinition("abc");
            cmdDef.AddOption<string>(key: "key", defaultArg: "default", help: "help", flags: (terse: "-k", verbose: "--key"));

            OptionDefinition opDef = cmdDef["key"];

            Assert.Equal(typeof(string), opDef.GenericType);
            Assert.Equal("key", opDef.Key);
            Assert.Equal("help", opDef.Help);
            Assert.Equal("-k", opDef.Flags.Terse);
            Assert.Equal("--key", opDef.Flags.Verbose);
            Assert.True(opDef.HasConstraints);
            Assert.False(opDef.MustAssign);
            Assert.True(opDef.HasDefault);
        }

        [Fact]
        public void AddOptionOfT_ForSignature_Key_Help_Converter_Flags()
        {
            var cmdDef = new CommandDefinition("abc");
            cmdDef.AddOption<string>(key: "key", help: "help", converter: (arg) => string.Empty, flags: (terse: "-k", verbose: "--key")) ;

            OptionDefinition opDef = cmdDef["key"];

            Assert.Equal(typeof(string), opDef.GenericType);
            Assert.Equal("key", opDef.Key);
            Assert.Equal("help", opDef.Help);
            Assert.Equal("-k", opDef.Flags.Terse);
            Assert.Equal("--key", opDef.Flags.Verbose);
            Assert.True(opDef.HasConstraints);
            Assert.True(opDef.MustAssign);
            Assert.False(opDef.HasDefault);
        }

        [Fact]
        public void AddOptionOfT_ForSignature_Key_Default_Help_Converter_Flags()
        {
            var cmdDef = new CommandDefinition("abc");
            cmdDef.AddOption<string>(key: "key", defaultArg: "xyz", help: "help", converter: (arg) => arg, flags: (terse: "-k", verbose: "--key"));

            OptionDefinition opDef = cmdDef["key"];

            Assert.Equal(typeof(string), opDef.GenericType);
            Assert.Equal("key", opDef.Key);
            Assert.Equal("help", opDef.Help);
            Assert.Equal("-k", opDef.Flags.Terse);
            Assert.Equal("--key", opDef.Flags.Verbose);
            Assert.True(opDef.HasConstraints);
            Assert.False(opDef.MustAssign);
            Assert.True(opDef.HasDefault);
        }
        #endregion

        #region option exists
        [Fact]
        public void OptionExists_WhenOptionDoesExist_ShouldReturn_True()
        {
            var cmdDef = new CommandDefinition("abc");
            cmdDef.AddOption<string>(key: "key", help: "help", flags: (terse: "-k", verbose: "--key"));
            Assert.True(cmdDef.OptionExists("key"));   
        }

        [Fact]
        public void OptionExists_WhenOptionDoesNotExist_ShouldReturn_False()
        {
            var cmdDef = new CommandDefinition("abc");
            cmdDef.AddOption<string>(key: "key", help: "help", flags: (terse: "-k", verbose: "--key"));
            Assert.False(cmdDef.OptionExists("not-key"));
        }
        #endregion

        #region must assign one of
        [Fact]
        public void MustAssignOneOf_WhenNullParamsProvided_ShouldThrow_ArgumentNullException()
        {
            var cmdDef = new CommandDefinition("abc");
            Action action = () => cmdDef.MustAssignOneOf(null);
            Assert.Throws<ArgumentNullException>(action);
        }

        [Fact]
        public void MustAssignOneOf_WhenOnlyOneOptionKeysParamProvided_ShouldThrow_ArgumentException()
        {
            var cmdDef = new CommandDefinition("abc");
            cmdDef.AddOption<int>(key: "one", defaultArg: 1, help: "help-one", flags: (terse: "-o", verbose: "--one"));
            cmdDef.AddOption<int>(key: "two", defaultArg: 2, help: "help-two", flags: (terse: "-t", verbose: "--two"));

            Action action = () => cmdDef.MustAssignOneOf("one");
            Assert.Throws<ArgumentException>(action);
        }

        [Fact]
        public void MustAssignOneOf_WhenOptionKeyProvided_DoesNotExist_ShouldThrow_KeyNotFoundException()
        {
            var cmdDef = new CommandDefinition("abc");
            cmdDef.AddOption<int>(key: "one", defaultArg: 1, help: "help-one", flags: (terse: "-o", verbose: "--one"));
            cmdDef.AddOption<int>(key: "two", defaultArg: 2, help: "help-two", flags: (terse: "-t", verbose: "--two"));

            Action action = () => cmdDef.MustAssignOneOf("three", "one");
            Assert.Throws<KeyNotFoundException>(action);
        }

        [Fact]
        public void MustAssignOneOf_WhenOptionKeyProvided_MapsToOptionWithNoDefaultArg_ShouldThrow_CommandDefinitionException()
        {
            var cmdDef = new CommandDefinition("abc");
            cmdDef.AddOption<int>(key: "one", defaultArg: 1, help: "help-one", flags: (terse: "-o", verbose: "--one"));
            cmdDef.AddOption<int>(key: "two", defaultArg: 2, help: "help-two", flags: (terse: "-t", verbose: "--two"));
            cmdDef.AddOption<int>(key: "uno", help: "help-two", flags: (terse: "-u", verbose: "--uno"));

            //uno must already be assigned as it does not have a default argument; therefor, uno cannot be part
            //of a must assign one of constraint
            Action action = () => cmdDef.MustAssignOneOf("one", "two", "uno");
            Assert.Throws<CommandDefinitionException>(action);
        }

        [Fact]
        public void MustAssignOneOf_WhenAlloptionKeyProvided_MapsToOptionWithDefaultArgs_ShouldCreateValidConstraint()
        {
            var cmdDef = new CommandDefinition("abc");
            cmdDef.AddOption<int>(key: "one", defaultArg: 1, help: "help-one", flags: (terse: "-o", verbose: "--one"));
            cmdDef.AddOption<int>(key: "two", defaultArg: 2, help: "help-two", flags: (terse: "-t", verbose: "--two"));
            cmdDef.AddOption<int>(key: "uno", defaultArg: 1, help: "help-two", flags: (terse: "-u", verbose: "--uno"));

            cmdDef.MustAssignOneOf("one", "two", "uno");
            Assert.True(cmdDef.Constraints.Exists(c => c is MustAssignOneOfConstraint));
        }
        #endregion

        #region mutually exclusive set
        [Fact]
        public void MutuallyExclusiveSet_WhenNullParamsProvided_ShouldThrow_ArgumentNullException()
        {
            var cmdDef = new CommandDefinition("abc");
            Action action = () => cmdDef.MutuallyExclusiveSet(null);
            Assert.Throws<ArgumentNullException>(action);
        }

        [Fact]
        public void MutuallyExclusiveSet_WhenOnlyOneOptionKeysParamProvided_ShouldThrow_CommandDefinitionException()
        {
            var cmdDef = new CommandDefinition("abc");
            cmdDef.AddOption<int>(key: "one", defaultArg: 1, help: "help-one", flags: (terse: "-o", verbose: "--one"));
            cmdDef.AddOption<int>(key: "two", defaultArg: 2, help: "help-two", flags: (terse: "-t", verbose: "--two"));

            Action action = () => cmdDef.MutuallyExclusiveSet("one");
            Assert.Throws<ArgumentException>(action);
        }

        [Fact]
        public void MutuallyExclusiveSet_WhenOptionKeyProvided_DoesNotExist_ShouldThrow_KeyNotFoundException()
        {
            var cmdDef = new CommandDefinition("abc");
            cmdDef.AddOption<int>(key: "one", defaultArg: 1, help: "help-one", flags: (terse: "-o", verbose: "--one"));
            cmdDef.AddOption<int>(key: "two", defaultArg: 2, help: "help-two", flags: (terse: "-t", verbose: "--two"));

            Action action = () => cmdDef.MutuallyExclusiveSet("three", "one");
            Assert.Throws<KeyNotFoundException>(action);
        }

        [Fact]
        public void MutuallyExclusiveSet_WhenOptionKeyProvided_MapsToOptionWithNoDefaultArg_ShouldThrow_CommandDefinitionException()
        {
            var cmdDef = new CommandDefinition("abc");
            cmdDef.AddOption<int>(key: "one", defaultArg: 1, help: "help-one", flags: (terse: "-o", verbose: "--one"));
            cmdDef.AddOption<int>(key: "two", defaultArg: 2, help: "help-two", flags: (terse: "-t", verbose: "--two"));
            cmdDef.AddOption<int>(key: "uno", help: "help-two", flags: (terse: "-u", verbose: "--uno"));

            //uno must already be assigned as it does not have a default argument; therefor, uno cannot be part
            //of a must assign one of constraint
            Action action = () => cmdDef.MutuallyExclusiveSet("one", "two", "uno");
            Assert.Throws<CommandDefinitionException>(action);
        }

        [Fact]
        public void MutuallyExclusiveSet_WhenAlloptionKeyProvided_MapsToOptionWithDefaultArgs_ShouldCreateValidConstraint()
        {
            var cmdDef = new CommandDefinition("abc");
            cmdDef.AddOption<int>(key: "one", defaultArg: 1, help: "help-one", flags: (terse: "-o", verbose: "--one"));
            cmdDef.AddOption<int>(key: "two", defaultArg: 2, help: "help-two", flags: (terse: "-t", verbose: "--two"));
            cmdDef.AddOption<int>(key: "uno", defaultArg: 1, help: "help-two", flags: (terse: "-u", verbose: "--uno"));

            cmdDef.MutuallyExclusiveSet("one", "two", "uno");
            Assert.True(cmdDef.Constraints.Exists(c => c is MutuallyExclusiveSetConstraint));
        }
        #endregion

        #region apply constraint
        [Fact]
        public void ApplyConstraint_WhenConstraintArgumentIsNull_ShouldThrow_ArgumentNullException()
        {
            var cmdDef = new CommandDefinition("abc");
            
            Action action = () => cmdDef.ApplyConstraint(null, "name", "description");
            Assert.Throws<ArgumentNullException>(action);
        }

        [Fact]
        public void ApplyConstraint_WhenNameArgumentIsNull_ShouldThrow_ArgumentNullException()
        {
            var cmdDef = new CommandDefinition("abc");

            Action action = () => cmdDef.ApplyConstraint((cmd) => true, null, "description");
            Assert.Throws<ArgumentNullException>(action);
        }

        [Fact]
        public void ApplyConstraint_WhenDescriptionArgumentIsNull_ShouldThrow_ArgumentNullException()
        {
            var cmdDef = new CommandDefinition("abc");

            Action action = () => cmdDef.ApplyConstraint((cmd) => true, "name", null);
            Assert.Throws<ArgumentNullException>(action);
        }

        [Fact]
        public void ApplyConstraint_WhenNameArgumentIsEmpty_ShouldThrow_ArgumentException()
        {
            var cmdDef = new CommandDefinition("abc");

            Action action = () => cmdDef.ApplyConstraint((cmd) => true, string.Empty, "description");
            Assert.Throws<ArgumentException>(action);
        }

        [Fact]
        public void ApplyConstraint_WhenDescriptionArgumentIsEmpty_ShouldThrow_ArgumentException()
        {
            var cmdDef = new CommandDefinition("abc");

            Action action = () => cmdDef.ApplyConstraint((cmd) => true, "name", string.Empty);
            Assert.Throws<ArgumentException>(action);
        }

        [Fact]
        public void ApplyConstraint_WhenValidArgumentsProvided_ShouldCreateValidConstraint()
        {
            var cmdDef = new CommandDefinition("abc");
            cmdDef.ApplyConstraint((cmd) => true, "name", "description");
            Assert.True(cmdDef.HasConstraints);
            Assert.True(cmdDef.Constraints.Exists(c => c.Name == "name"));
        }
        #endregion

        #region validate
        [Fact]
        public void Validate_WhenNameIsEmpty_ShouldThrow_CommandDefinitionException()
        {
            var cmdDef = new CommandDefinition(string.Empty);
            Action action = () => cmdDef.Validate();
            Assert.Throws<CommandDefinitionException>(action);
        }

        [Fact]
        public void Validate_WhenNameStartsWithDash_ShouldThrow_CommandDefinitionException()
        {
            var cmdDef = new CommandDefinition("-abc");
            Action action = () => cmdDef.Validate();
            Assert.Throws<CommandDefinitionException>(action);
        }

        [Fact]
        public void Validate_WhenNamelongerThanMaxNameLength_ShouldThrow_CommandDefinitionException()
        {
            var cmdDef = new CommandDefinition(new string('x', CommandDefinition.MaxNameLength + 1));
            Action action = () => cmdDef.Validate();
            Assert.Throws<CommandDefinitionException>(action);
        }

        [Fact]
        public void Validate_WhenAllExecutionHandlersAreNull_ShouldThrow_CommandDefinitionException()
        {
            var cmdDef = new CommandDefinition("abc");
            Action action = () => cmdDef.Validate();
            Assert.Equal($"{nameof(CommandDefinition)} must be provided a value for one of: {nameof(CommandDefinition.Handler)}, {nameof(CommandDefinition.AsyncHandler)}.", 
            Assert.Throws<CommandDefinitionException>(action).Message);
        }

        [Fact]
        public void Validate_WhenNameContainsOneOrMoreDots_ShouldExposeDepthAsDotCount()
        {
            var cmdDef1 = new CommandDefinition("abc.xyz");
            cmdDef1.Handler = (c) => { };
            cmdDef1.Validate();

            var cmdDef2 = new CommandDefinition("abc.xyz.xxx");
            cmdDef2.Handler = (c) => { };
            cmdDef2.Validate();

            var cmdDef3 = new CommandDefinition("abc.xyz.xxx.aaa");
            cmdDef3.Handler = (c) => { };
            cmdDef3.Validate();

            var cmdDef4 = new CommandDefinition("abc.xyz.xxx.aaa.bbb");
            cmdDef4.Handler = (c) => { };
            cmdDef4.Validate();

            var cmdDef5 = new CommandDefinition("abc.xyz.xxx.aaa.bbb.ccc");
            cmdDef5.Handler = (c) => { };
            cmdDef5.Validate();

            Assert.Equal(1, cmdDef1.Depth);
            Assert.Equal(2, cmdDef2.Depth);
            Assert.Equal(3, cmdDef3.Depth);
            Assert.Equal(4, cmdDef4.Depth);
            Assert.Equal(5, cmdDef5.Depth);
        }

        [Fact]
        public void Validate_WhenNameEndsWithDot_ShouldThrow_NamespaceDefinitionException()
        {
            var cmdDef = new CommandDefinition("abc.xyz.");
            Action action = () => cmdDef.Validate();
            Assert.Throws<CommandDefinitionException>(action);
        }
        #endregion
    }
}
