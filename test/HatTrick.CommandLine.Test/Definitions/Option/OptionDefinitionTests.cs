using NuGet.Frameworks;
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
            Action action = () => opDef = new OptionDefinition<string>(key: null, help: "help", converter: (arg) => arg, ("-x", "--xx"));
            Assert.Throws<ArgumentNullException>(action);
        }

        [Fact]
        public void Constructor_WhenHelpArgument_IsNull_ShouldThrow_ArgumentNullException()
        {
            OptionDefinition? opDef = null;
            Action action = () => opDef = new OptionDefinition<string>(key: "key", help: null, converter: (arg) => arg, ("-x", "--xx"));
            Assert.Throws<ArgumentNullException>(action);
        }

        [Fact]
        public void Constructor_WhenConverterArgument_IsNull_ShouldThrow_ArgumentNullException()
        {
            OptionDefinition? opDef = null;
            Action action = () => opDef = new OptionDefinition<string>(key: "key", help: "help", converter: null, ("-x", "--xx"));
            Assert.Throws<ArgumentNullException>(action);
        }

        [Fact]
        public void Constructor_WhenFlagsArgument_IsDefault_ShouldThrow_ArgumentNullException()
        {
            OptionDefinition? opDef = null;
            Action action = () => opDef = new OptionDefinition<string>(key: "key", help: "help", converter: (arg) => arg, default);
            Assert.Throws<ArgumentNullException>(action);
        }

        [Fact]
        public void Constructor_WhenGenericTypeIsBool_And_DefaultArg_IsNotProvided_ShouldHaveDefaultConstraint_EqualToFalse()
        {
            OptionDefinition opDef = new OptionDefinition<bool>(
                key: "key", 
                help: "help", 
                converter: (arg) => bool.Parse(arg), 
                ("-x", "--xxx")
            );

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
                ("-x", "--xxx")
            );

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
                ("-x", "--xxx")
            );

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
            var opDef = new OptionDefinition<int>(key: "key", help: "help", converter: (arg) => int.Parse(arg), ("-x", "--xx"));
            Assert.False(opDef.Hidden);
            opDef.Hide();
            Assert.True(opDef.Hidden);
        }
        #endregion

        #region accepted values
        [Fact]
        public void AcceptedValuesOfT_WhenT_IsCompatibleWith_TOfOption_ShouldHave_AcceptedValuesConstraint_ContainingProvidedAcceptedValues()
        {
            var opDef = new OptionDefinition<int>(key: "key", help: "help", converter: (arg) => int.Parse(arg), ("-x", "--xx"));
            (opDef as OptionDefinition).AcceptedValues(128, 256, 512);

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
            var opDef = new OptionDefinition<int>(key: "key", help: "help", converter: (arg) => int.Parse(arg), ("-x", "--xxx"));
            //attempt to add string values to op def of int
            Action action = () => (opDef as OptionDefinition).AcceptedValues("128", "256", "512");
            Assert.Contains("Cannot set accepted values", Assert.Throws<CommandDefinitionException>(action).Message);
        }

        [Fact]
        public void AcceptedValuesOfT_WhenNullProvided_ShouldThrow_ArgumentNullException()
        {
            var opDef = new OptionDefinition<int>(key: "key", help: "help", converter: (arg) => int.Parse(arg), ("-x", "--xxx"));
            Action action = () => opDef.AcceptedValues(EqualityComparer<int>.Default, null);
            Assert.Throws<ArgumentNullException>(action);
        }

        [Fact]
        public void AcceptedValuesOfT_WhenEmptyArrayProvided_ShouldThrow_ArgumentException()
        {
            var opDef = new OptionDefinition<int>(key: "key", help: "help", converter: (arg) => int.Parse(arg), ("-x", "--xxx"));
            Action action = () => opDef.AcceptedValues(new int[0]);
            Assert.Throws<ArgumentException>(action);
        }

        [Fact]
        public void AcceptedValuesOfT_WhenOptionDefintion_AlreadyContains_AcceptedValuesConstraint_ShouldThrow_CommandDefinitionException()
        {
            var opDef = new OptionDefinition<int>(key: "key", help: "help", converter: (arg) => int.Parse(arg), ("-x", "--xxx"));
            opDef.AcceptedValues(128, 256, 512);
            Action action = () => opDef.AcceptedValues(1024, 2048, 4096);
            Assert.Contains("already contains an", Assert.Throws<CommandDefinitionException>(action).Message);
        }

        [Fact]
        public void AcceptedValuesOfT_WhenOptionDefintion_ContainsDefaultConstraint_AndAcceptedValues_DoesNotContainDefault_ShouldThrow_CommandDefinitionException()
        {
            var opDef = new OptionDefinition<int>(key: "key", 127, help: "help", converter: (arg) => int.Parse(arg), ("-x", "--xxx"));
            Action action = () => opDef.AcceptedValues(128, 256, 512);
            Assert.Contains("which is not defined in the accepted values set", Assert.Throws<CommandDefinitionException>(action).Message);
        }
        #endregion

        #region apply constraint
        [Fact]
        public void ApplyConstraint_WhenConstraintArgument_IsNull_ShouldThrow_ArgumentNullException()
        {
            var opDef = new OptionDefinition<int>(key: "key", help: "help", (arg) => int.Parse(arg), ("-x", "--xx"));
            //get the abstract implementation
            Action action = () => opDef.ApplyConstraint<int>(null, "name", "description");
            Assert.Throws<ArgumentNullException>(action);
        }

        [Fact]
        public void ApplyConstraint_WhenNameArgument_IsNull_ShouldThrow_ArgumentNullException()
        {
            var opDef = new OptionDefinition<int>(key: "key", help: "help", (arg) => int.Parse(arg), ("-x", "--xx"));
            //get the abstract implementation
            Action action = () => opDef.ApplyConstraint<int>((x) => true, null, "description");
            Assert.Throws<ArgumentNullException>(action);
        }

        [Fact]
        public void ApplyConstraint_WhenDescriptionArgument_IsNull_ShouldThrow_ArgumentNullException()
        {
            var opDef = new OptionDefinition<int>(key: "key", help: "help", (arg) => int.Parse(arg), ("-x", "--xx"));
            //get the abstract implementation
            Action action = () => opDef.ApplyConstraint<int>((x) => true, "name", null);
            Assert.Throws<ArgumentNullException>(action);
        }

        [Fact]
        public void ApplyConstraint_WhenT_IsNotCompatibleWith_TofOption_ShouldThrow_CommandDefinitionException()
        {
            var opDef = new OptionDefinition<int>(key: "key", help: "help", (arg) => int.Parse(arg), ("-x", "--xx"));
            //get the abstract implementation
            Action action = () => opDef.ApplyConstraint<DateTime>((x) => true, "name", "description");
            Assert.Throws<CommandDefinitionException>(action);
        }

        [Fact]
        public void ApplyConstraint_WhenArgumentsValid_ShouldResultInValidFunctioningConstraint()
        {
            var opDef = new OptionDefinition<int>(key: "key", help: "help", (arg) => int.Parse(arg), ("-x", "--xx"));
            opDef.ApplyConstraint((x) => x > 20, "greater than 20", "description");
            Assert.True(opDef.Constraints.Exists(c => c is ArgumentConstraint && c.Name == "greater than 20"));
        }
        #endregion

        #region empty instance
        [Fact]
        public void EmptyInstance_MapsCorrectly()
        {
            var opDef = new OptionDefinition<int>(key: "key", help: "help", (arg) => int.Parse(arg), ("-k", "--key"));
            EmptyOption empty = opDef.EmptyInstance();
            Assert.Equal("key", empty.Key);
            Assert.Equal("--key", empty.Flag);//VERBOSE FLAG
        }
        #endregion

        #region validate
        [Fact]
        public void Validate_WhenKeyIsEmpty_ShouldThrow_CommandDefinitionException()
        {
            var opDef = new OptionDefinition<int>(key: string.Empty, help: "help", converter: (arg) => int.Parse(arg), ("-x", "--x"));
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
                ("-x", "--x")
            );

            Action action = () => opDef.Validate();
            Assert.Contains("max accepted char length is", Assert.Throws<CommandDefinitionException>(action).Message);
        }

        [Fact]
        public void Validate_WhenHelpIsEmpty_ShouldThrow_CommandDefinitionException()
        {
            var opDef = new OptionDefinition<int>(key: "key", help: string.Empty, converter: (arg) => int.Parse(arg), ("-x", "--x"));
            Action action = () => opDef.Validate();
            Assert.Throws<CommandDefinitionException>(action);
        }

        [Fact]
        public void Validate_WhenVerboseFlagsLength_LessThan4_ShouldThrow_CommandDefinitionException()
        {
            var opDef = new OptionDefinition<int>(key: "key", help: "help", converter: (arg) => int.Parse(arg), (terse: "-x", verbose: "--x")) ;
            Action action = () => opDef.Validate();
            Assert.Contains(
                "verbose flags must be at least 4 chars in length",
                Assert.Throws<CommandDefinitionException>(action).Message
            );
        }

        [Fact]
        public void Validate_WhenVerboseFlags_DoesNotStartWith_2Dashes_ShouldThrow_CommandDefinitionException()
        {
            var opDef = new OptionDefinition<int>(key: "key", help: "help", converter: (arg) => int.Parse(arg), (terse: "-x", verbose: "-xxxx"));
            Action action = () => opDef.Validate();
            Assert.Contains(
                "verbose flags must start with 2 dashes.",
                Assert.Throws<CommandDefinitionException>(action).Message
            );
        }

        [Fact]
        public void Validate_WhenVerboseFlags_StartsWith_3Dashses_ShouldThrow_CommandDefinitionException()
        {
            var opDef = new OptionDefinition<int>(key: "key", help: "help", converter: (arg) => int.Parse(arg), (terse: "-x", verbose: "---xx"));
            Action action = () => opDef.Validate();
            Assert.Contains(
                "verbose flags cannot start with 3 dashes",
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
                ("-x", "--" + new string('x', OptionDefinition.MaxFlagLength))
            );
            Action action = () => opDef.Validate();
            Assert.Contains(
                "is greater than max allowed",
                Assert.Throws<CommandDefinitionException>(action).Message
            );
        }

        [Fact]
        public void Validate_WhenVerboseFlag_ContainsAChar_NotLetterOrDigit_ShouldThrow_CommandDefinitionException()
        {
            var opDef = new OptionDefinition<int>(
                key: "key",
                help: "help",
                converter: (arg) => int.Parse(arg),
                ("-x}", "--xxx.x")
            );
            Action action = () => opDef.Validate();
            Assert.Contains(
                "verbose flags can contain letters, digits and dashes.",
                Assert.Throws<CommandDefinitionException>(action).Message
            );
        }

        [Fact]
        public void Validate_WhenTerseFlagLength_LessThan2_ShouldThrow_CommandDefinitionException()
        {
            var opDef = new OptionDefinition<int>(
                key: "key",
                help: "help",
                converter: (arg) => int.Parse(arg),
                ("-", "--xxx")
            );
            Action action = () => opDef.Validate();
            Assert.Contains(
                "terse flags start with a single dash followed by a single char", 
                Assert.Throws<CommandDefinitionException>(action).Message
            );
        }

        [Fact]
        public void Validate_WhenTerseFlagLength_GreaterThan2_ShouldThrow_CommandDefinitionException()
        {
            var opDef = new OptionDefinition<int>(
                key: "key",
                help: "help",
                converter: (arg) => int.Parse(arg),
                ("-xx", "--xxx")
            );
            Action action = () => opDef.Validate();
            Assert.Contains(
                "terse flags start with a single dash followed by a single char", 
                Assert.Throws<CommandDefinitionException>(action).Message
            );
        }

        [Fact]
        public void Validate_WhenTerseFlagDoesNotStartWithDash_ShouldThrow_CommandDefinitionException()
        {
            var opDef = new OptionDefinition<int>(
                key: "key", 
                help: "help", 
                converter: (arg) => int.Parse(arg), 
                ("+x", "--xxx")
            );
            Action action = () => opDef.Validate();
            Assert.Contains(
                "terse flags must start with a single dash", 
                Assert.Throws<CommandDefinitionException>(action).Message
            );
        }

        [Fact]
        public void Validate_WhenTerseFlagDoesNotEndWith_LetterOrDigit_ShouldThrow_CommandDefinitionException()
        {
            var opDef = new OptionDefinition<int>(
                key: "key",
                help: "help",
                converter: (arg) => int.Parse(arg),
                ("-}", "--xxx")
            );
            Action action = () => opDef.Validate();
            Assert.Contains(
                "terse flags start with a dash followed by a single letter or digit", 
                Assert.Throws<CommandDefinitionException>(action).Message
            );
        }
        #endregion

        #region get generic type
        [Fact]
        public void GenericType_Accessor_ResolvesCorrectGenericType_ForCLRTypes()
        {
            var opDefInt32 = new OptionDefinition<int>(key: "key", help: "help", (arg) => int.Parse(arg), ("-k", "--key"));
            var opDefInt64 = new OptionDefinition<long>(key: "key", help: "help", (arg) => long.Parse(arg), ("-k", "--key"));
            var opDefChar = new OptionDefinition<char>(key: "key", help: "help", (arg) => char.Parse(arg), ("-k", "--key"));
            var opDefString = new OptionDefinition<string>(key: "key", help: "help", (arg) => arg, ("-k", "--key"));
            var opDefDateTime = new OptionDefinition<DateTime>(key: "key", help: "help", (arg) => DateTime.Parse(arg), ("-k", "--key"));
            var opDefGuid = new OptionDefinition<Guid>(key: "key", help: "help", (arg) => Guid.Parse(arg), ("-k", "--key"));
            var opDefByte = new OptionDefinition<byte>(key: "key", help: "help", (arg) => byte.Parse(arg), ("-k", "--key"));
            var opDefBoolean = new OptionDefinition<bool>(key: "key", help: "help", (arg) => bool.Parse(arg), ("-k", "--key"));
            //...that's enouch to prove the point...

            Assert.Equal(typeof(int), opDefInt32.GenericType);
            Assert.Equal(typeof(long), opDefInt64.GenericType);
            Assert.Equal(typeof(char), opDefChar.GenericType);
            Assert.Equal(typeof(string), opDefString.GenericType);
            Assert.Equal(typeof(DateTime), opDefDateTime.GenericType);
            Assert.Equal(typeof(Guid), opDefGuid.GenericType);
            Assert.Equal(typeof(byte), opDefByte.GenericType);
            Assert.Equal(typeof(bool), opDefBoolean.GenericType);
        }

        [Fact]
        public void GenericType_Accessor_ResolvesCorrectGenericType_ForNullableCLRTypes()
        {
            var opDefInt32 = new OptionDefinition<int?>(key: "key", help: "help", (arg) => arg is not null ? int.Parse(arg) : null, ("-k", "--key"));
            var opDefInt64 = new OptionDefinition<long?>(key: "key", help: "help", (arg) => arg is not null ? long.Parse(arg) : null, ("-k", "--key"));
            var opDefChar = new OptionDefinition<char?>(key: "key", help: "help", (arg) => arg is not null ? char.Parse(arg) : null, ("-k", "--key"));
            var opDefDateTime = new OptionDefinition<DateTime?>(key: "key", help: "help", (arg) => arg is not null ? DateTime.Parse(arg) : null, ("-k", "--key"));
            var opDefGuid = new OptionDefinition<Guid?>(key: "key", help: "help", (arg) => arg is not null ? Guid.Parse(arg) : null, ("-k", "--key"));
            var opDefByte = new OptionDefinition<byte?>(key: "key", help: "help", (arg) => arg is not null ? byte.Parse(arg) : null, ("-k", "--key"));
            var opDefBoolean = new OptionDefinition<bool?>(key: "key", help: "help", (arg) => arg is not null ? bool.Parse(arg) : null, ("-k", "--key"));
            //...that's enouch to prove the point...

            Assert.Equal(typeof(int?), opDefInt32.GenericType);
            Assert.Equal(typeof(long?), opDefInt64.GenericType);
            Assert.Equal(typeof(char?), opDefChar.GenericType);
            Assert.Equal(typeof(DateTime?), opDefDateTime.GenericType);
            Assert.Equal(typeof(Guid?), opDefGuid.GenericType);
            Assert.Equal(typeof(byte?), opDefByte.GenericType);
            Assert.Equal(typeof(bool?), opDefBoolean.GenericType);
        }

        [Fact]
        public void GenericType_Accessor_ResolvesCorrectGenericType_ForNonCLRReferenceTypes()
        {
            var opDefFileInfo = new OptionDefinition<FileInfo>(key: "file", help: "help", (arg) => new FileInfo(arg), ("-f", "--file-name"));
            var opDefUri = new OptionDefinition<Uri>(key: "file", help: "help", (arg) => new Uri(arg), ("-u", "--uri"));
            //...that's enouch to prove the point...

            Assert.Equal(typeof(FileInfo), opDefFileInfo.GenericType);
            Assert.Equal(typeof(Uri), opDefUri.GenericType);
        }

        [Fact]
        public void GenericType_Accessor_ResolvesCorrectGenericType_ForEnumerationTypes()
        {
            var opDefFileMode = new OptionDefinition<FileMode>(key: "file", help: "help", (arg) => Enum.Parse<FileMode>(arg, true), ("-f", "--file-name"));
            var opDefFileAccess = new OptionDefinition<FileAccess>(key: "file", help: "help", (arg) => Enum.Parse<FileAccess>(arg, true), ("-u", "--uri"));
            //...that's enouch to prove the point...

            Assert.Equal(typeof(FileMode), opDefFileMode.GenericType);
            Assert.Equal(typeof(FileAccess), opDefFileAccess.GenericType);
        }

        [Fact]
        public void GenericType_Accessor_ResolvesCorrectGenericType_ForNullableEnumerationTypes()
        {
            var opDefFileMode = new OptionDefinition<FileMode?>(key: "file", help: "help", (arg) => arg is not null ? Enum.Parse<FileMode>(arg, true) : null, ("-f", "--file-name"));
            var opDefFileAccess = new OptionDefinition<FileAccess?>(key: "file", help: "help", (arg) => arg is not null ? Enum.Parse<FileAccess>(arg, true) : null, ("-u", "--uri"));
            //...that's enouch to prove the point...

            Assert.Equal(typeof(FileMode?), opDefFileMode.GenericType);
            Assert.Equal(typeof(FileAccess?), opDefFileAccess.GenericType);
        }
        #endregion
    }
}
