using Xunit;

namespace HatTrick.CommandLine.Test
{
    public class OptionDefinitionTests
    {
        #region constructor
        [Fact]
        public void Constructor_WhenKeyArgument_IsNull_ShouldThrow_ArgumentNullException()
        {
            OptionDefinition? opDef = null;
            Action action = () => opDef = new OptionDefinition<string>(key: null, help: "help", converter: (arg) => arg, "-x");
            Assert.Throws<ArgumentNullException>(action);
        }

        [Fact]
        public void Constructor_WhenHelpArgument_IsNull_ShouldThrow_ArgumentNullException()
        {
            OptionDefinition? opDef = null;
            Action action = () => opDef = new OptionDefinition<string>(key: "key", help: null, converter: (arg) => arg, "-x");
            Assert.Throws<ArgumentNullException>(action);
        }

        [Fact]
        public void Constructor_WhenConverterArgument_IsNull_ShouldThrow_ArgumentNullException()
        {
            OptionDefinition? opDef = null;
            Action action = () => opDef = new OptionDefinition<string>(key: "key", help: "help", converter: null, "-x");
            Assert.Throws<ArgumentNullException>(action);
        }

        [Fact]
        public void Constructor_WhenFlagsArgument_IsNull_ShouldThrow_ArgumentNullException()
        {
            OptionDefinition? opDef = null;
            Action action = () => opDef = new OptionDefinition<string>(key: "key", help: "help", converter: (arg) => arg, null);
            Assert.Throws<ArgumentNullException>(action);
        }

        [Fact]
        public void Constructor_WhenGenericTypeIsBool_And_DefaultArg_IsNotProvided_ShouldHaveDefaultConstraint_EqualToFalse()
        {
            OptionDefinition opDef = new OptionDefinition<bool>(
                key: "key", 
                help: "help", 
                converter: (arg) => bool.Parse(arg), 
                "-x", "--xxx");

            Assert.True(opDef.HasConstraints);
            Assert.False(opDef.MustAssign);
            Assert.True(opDef.HasDefault);
            Assert.IsType<DefaultConstraint<bool>>(opDef.Constraints[0]);
            Assert.False((opDef.Constraints[0] as DefaultConstraint<bool>)!.DefaultValue);
        }

        [Fact]
        public void Constructor_WhenGenericTypeIsAnythingButBool_And_DefaultArg_IsNotProvided_ShouldHaveMustAssignConstraint()
        {
            OptionDefinition opDef = new OptionDefinition<int>(
                key: "key",
                help: "help",
                converter: (arg) => int.Parse(arg),
                "-x", "--xxx");

            Assert.True(opDef.HasConstraints);
            Assert.False(opDef.HasDefault);
            Assert.True(opDef.MustAssign);
            Assert.IsType<MustAssignConstraint<int>>(opDef.Constraints[0]);
        }

        [Fact]
        public void Constructor_WhenGenericTypeIsAnythingButBool_And_DefaultArg_IsProvided_ShouldHaveDefaultConstraint_EqualToDefaultArg()
        {
            OptionDefinition opDef = new OptionDefinition<int>(
                key: "key",
                defaultArg: 128,
                help: "help",
                converter: (arg) => int.Parse(arg),
                "-x", "--xxx");

            Assert.True(opDef.HasConstraints);
            Assert.True(opDef.HasDefault);
            Assert.False(opDef.MustAssign);
            Assert.IsType<DefaultConstraint<int>>(opDef.Constraints[0]);
            Assert.Equal(128, (opDef.Constraints[0] as DefaultConstraint<int>)!.DefaultValue);
        }
        #endregion

        #region hide
        [Fact]
        public void Hide_ShouldMarkDefinition_Hidden()
        {
            var opDef = new OptionDefinition<int>(key: "key", help: "help", converter: (arg) => int.Parse(arg), "-x");
            Assert.False(opDef.Hidden);
            opDef.Hide();
            Assert.True(opDef.Hidden);
        }
        #endregion

        #region accepted values
        [Fact]
        public void AcceptedValuesOfT_WhenT_IsCompatibleWith_TOfOption_ShouldHave_AcceptedValuesConstraint_ContainingProvidedAcceptedValues()
        {
            var opDef = new OptionDefinition<int>(key: "key", help: "help", converter: (arg) => int.Parse(arg), "--xxx");
            opDef.AcceptedValues(128, 256, 512);

            Assert.True(opDef.HasConstraints);
            var accepted = opDef.Constraints.Find(c => c is AcceptedValuesConstraint<int>) as AcceptedValuesConstraint<int>;
            Assert.NotNull(accepted);
            Assert.True(accepted.IsInAcceptedSet(128));
            Assert.True(accepted.IsInAcceptedSet(256));
            Assert.True(accepted.IsInAcceptedSet(512));
        }

        [Fact]
        public void AcceptedValuesOfT_WhenT_IsNotCompatibleWith_TOfOption_ShouldThrow_CommandDefinitionException()
        {
            var opDef = new OptionDefinition<int>(key: "key", help: "help", converter: (arg) => int.Parse(arg), "--xxx");
            //attempt to add string values to op def of int
            Action action = () => opDef.AcceptedValues("128", "256", "512");
            Assert.Throws<CommandDefinitionException>(action);
        }
        #endregion

        #region apply constraint
        [Fact]
        public void ApplyConstraint_WhenConstraintArgument_IsNull_ShouldThrow_ArgumentNullException()
        {
            var opDef = new OptionDefinition<int>(key: "key", help: "help", (arg) => int.Parse(arg), "-x");
            //get the abstract implementation
            OptionDefinition op = opDef as OptionDefinition;
            Action action = () => op.ApplyConstraint<int>(null, "name", "description");
            Assert.Throws<ArgumentNullException>(action);
        }

        [Fact]
        public void ApplyConstraint_WhenNameArgument_IsNull_ShouldThrow_ArgumentNullException()
        {
            var opDef = new OptionDefinition<int>(key: "key", help: "help", (arg) => int.Parse(arg), "-x");
            //get the abstract implementation
            OptionDefinition op = opDef as OptionDefinition;
            Action action = () => op.ApplyConstraint<int>((x) => true, null, "description");
            Assert.Throws<ArgumentNullException>(action);
        }

        [Fact]
        public void ApplyConstraint_WhenDescriptionArgument_IsNull_ShouldThrow_ArgumentNullException()
        {
            var opDef = new OptionDefinition<int>(key: "key", help: "help", (arg) => int.Parse(arg), "-x");
            //get the abstract implementation
            OptionDefinition op = opDef as OptionDefinition;
            Action action = () => op.ApplyConstraint<int>((x) => true, "name", null);
            Assert.Throws<ArgumentNullException>(action);
        }

        [Fact]
        public void ApplyConstraint_WhenT_IsNotCompatibleWith_TofOption_ShouldThrow_CommandDefinitionException()
        {
            var opDef = new OptionDefinition<int>(key: "key", help: "help", (arg) => int.Parse(arg), "-x");
            //get the abstract implementation
            OptionDefinition op = opDef as OptionDefinition;
            Action action = () => op.ApplyConstraint<DateTime>((x) => true, "name", "description");
            Assert.Throws<CommandDefinitionException>(action);
        }
        #endregion

        #region empty instance
        [Fact]
        public void EmptyInstance_()
        {
            var opDef = new OptionDefinition<int>(key: "key", help: "help", (arg) => int.Parse(arg), "-k", "--key");
            EmptyOption empty = opDef.EmptyInstance();
            Assert.Equal("key", empty.Key);
            Assert.Equal("--key", empty.Flag);
        }
        #endregion

        #region validate
        [Fact]
        public void Validate_WhenKeyIsEmpty_ShouldThrow_CommandDefinitionException()
        {
            var opDef = new OptionDefinition<int>(key: string.Empty, help: "help", converter: (arg) => int.Parse(arg), "-x", "--x");
            Action action = () => opDef.Validate();
            Assert.Throws<CommandDefinitionException>(action);
        }

        [Fact]
        public void Validate_WhenKeyLengthIsGreaterThanMax_ShouldThrow_CommandDefinitionException()
        {
            var opDef = new OptionDefinition<int>(
                key: new string('x', OptionDefinition.MaxKeyLength + 1), 
                help: "help", 
                converter: (arg) => int.Parse(arg), 
                "-x", "--x");

            Action action = () => opDef.Validate();
            Assert.Contains("max accepted char length is", Assert.Throws<CommandDefinitionException>(action).Message);
        }

        [Fact]
        public void Validate_WhenHelpIsEmpty_ShouldThrow_CommandDefinitionException()
        {
            var opDef = new OptionDefinition<int>(key: "key", help: string.Empty, converter: (arg) => int.Parse(arg), "-x", "--x");
            Action action = () => opDef.Validate();
            Assert.Throws<CommandDefinitionException>(action);
        }

        [Fact]
        public void Validate_WhenFlagsIsEmpty_ShouldThrow_CommandDefinitionException()
        {
            var opDef = new OptionDefinition<int>(key: "key", help: "help", converter: (arg) => int.Parse(arg), new string[] { }) ;
            Action action = () => opDef.Validate();
            Assert.Throws<CommandDefinitionException>(action);
        }

        [Fact]
        public void Validate_WhenFlagsIsNullOrWhitespace_ShouldThrow_CommandDefinitionException()
        {
            var opDef = new OptionDefinition<int>(
                key: "key", 
                help: "help", 
                converter: (arg) => int.Parse(arg), 
                "-x", "--xxx", string.Empty);
            Action action = () => opDef.Validate();
            Assert.Contains("contains a flag that is null or empty", Assert.Throws<CommandDefinitionException>(action).Message);
        }

        [Fact]
        public void Validate_WhenFlagsDoesNotStartWithDash_ShouldThrow_CommandDefinitionException()
        {
            var opDef = new OptionDefinition<int>(
                key: "key", 
                help: "help", 
                converter: (arg) => int.Parse(arg), 
                "-x", "--xxx", "xyz");
            Action action = () => opDef.Validate();
            Assert.Contains("Option flags must begin with a '-'", Assert.Throws<CommandDefinitionException>(action).Message);
        }

        [Fact]
        public void Validate_WhenVerboseFlagLengthLessThan4_ShouldThrow_CommandDefinitionException()
        {
            var opDef = new OptionDefinition<int>(
                key: "key",
                help: "help",
                converter: (arg) => int.Parse(arg),
                "-x", "--xxx", "--x");
            Action action = () => opDef.Validate();
            Assert.Contains(
                "Verbose option flags begin with '--' and must be longer than 1 additional char", 
                Assert.Throws<CommandDefinitionException>(action).Message
            );
        }

        [Fact]
        public void Validate_WhenVerboseFlagLengthGreaterThanMax_ShouldThrow_CommandDefinitionException()
        {
            var opDef = new OptionDefinition<int>(
                key: "key",
                help: "help",
                converter: (arg) => int.Parse(arg),
                "-x", "--xxx", ("--" + new string('x', OptionDefinition.MaxFlagLength)));
            Action action = () => opDef.Validate();
            Assert.Contains(
                "Verbose option flags cannot be >",
                Assert.Throws<CommandDefinitionException>(action).Message
            );
        }

        [Fact]
        public void Validate_WhenTerseFlagLengthGreaterThan2_ShouldThrow_CommandDefinitionException()
        {
            var opDef = new OptionDefinition<int>(
                key: "key",
                help: "help",
                converter: (arg) => int.Parse(arg),
                "-xy", "--xxx");
            Action action = () => opDef.Validate();
            Assert.Contains(
                "Terse option flags begin with '-' and must be exactly 1 other char",
                Assert.Throws<CommandDefinitionException>(action).Message
            );
        }
        #endregion
    }
}
