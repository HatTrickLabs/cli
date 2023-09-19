using Xunit;

namespace HatTrick.CommandLine.Test
{
    public class ParserTests
    {
        [Fact]
        public void Parse_WhenInput_IsNull_ShouldThrow_ArgumentNullException()
        {
            Action action = () => Parser.Parse(null);
            Assert.Throws<ArgumentNullException>(action);
        }

        [Fact]
        public void Parser_WhenInput_IsEmpty_ShouldReturn_DefaultCommand()
        {
            string[] input = Array.Empty<string>();
            Command result = Parser.Parse(input);
            Assert.Equal(CommandDefinition.DefaultCommandName, result.Key);
        }

        [Fact]
        public void Parser_WhenInput_IsEmpty_ShouldReturn_Command_WithoutOptions()
        {
            string[] input = Array.Empty<string>();
            Command result = Parser.Parse(input);
            Assert.Equal(Array.Empty<CommandOption>(), result.GetOptions());
        }

        [Fact]
        public void Parser_WhenInput_IsCommandlessWithOptions_ShouldReturn_DefaultCommand_WithOptions()
        {
            string[] input = new string[] { "-a", "-b" };
            Command result = Parser.Parse(input);
            Assert.Equal(CommandDefinition.DefaultCommandName, result.Key);
            Assert.Collection<CommandOption>(result.GetOptions(),
                (x) => Assert.Equal("-a", x.Flag),
                (x) => Assert.Equal("-b", x.Flag)
            );
        }

        [Fact]
        public void Parser_WhenInput_HasCommandWithOptions_ShouldReturn_Command_WithOptions()
        {
            string[] input = new string[] { "test", "-a", "-b" };
            Command result = Parser.Parse(input);
            Assert.Equal("test", result.Key);
            Assert.Collection<CommandOption>(result.GetOptions(),
                (x) => Assert.Equal("-a", x.Flag),
                (x) => Assert.Equal("-b", x.Flag)
            );
        }

        [Fact]
        public void Parser_WhenInput_ContainsArgumentTokenStartingWithDash_WithNoExplicitAssign_Should_TreatTokenAsFlag()
        {
            string[] input = new string[] { "add", "-x", "-3", "-y", "3" };
            Command result = Parser.Parse(input);
            Assert.Equal("add", result.Key);
            Assert.Collection<CommandOption>(result.GetOptions(),
                (x) => Assert.Equal("-x", x.Flag),
                (x) => Assert.Equal("-3", x.Flag),
                (x) => Assert.Equal("-y", x.Flag)
            );
        }

        [Fact]
        public void Parser_WhenInput_ContainsArgumentTokenStartingWithDash_WithExplicitEqualAssign_Should_TreatTokenAsArgument()
        {
            string[] input = new string[] { "add", "-x", "=", "-3", "-y", "3" };
            Command result = Parser.Parse(input);
            Assert.Equal("add", result.Key);
            Assert.Collection<CommandOption>(result.GetOptions(),
                (x) => { Assert.Equal("-x", x.Flag); Assert.Equal("-3", x.Argument); },
                (x) => Assert.Equal("-y", x.Flag)
            );
        }

        [Fact]
        public void Parser_WhenInput_ContainsArgumentTokenStartingWithDash_WithExplicitColonAssign_Should_TreatTokenAsArgument()
        {
            string[] input = new string[] { "add", "-x", ":", "-3", "-y", "3" };
            Command result = Parser.Parse(input);
            Assert.Equal("add", result.Key);
            Assert.Collection<CommandOption>(result.GetOptions(),
                (x) => { Assert.Equal("-x", x.Flag); Assert.Equal("-3", x.Argument); },
                (x) => { Assert.Equal("-y", x.Flag); Assert.Equal("3", x.Argument); }
            );
        }

        [Fact]
        public void Parser_WhenFirstOptionToken_IsExplicitAssignEqualToken_ShouldThrow_CommandInputException()
        {
            string[] input = new string[] { "abc", "=", "-x" };
            Action action = () => Parser.Parse(input);
            Assert.Throws<CommandInputException>(action);
        }

        [Fact]
        public void Parser_WhenAnyToken_IsSingleDashOnly_ShouldThrow_CommandInputException()
        {
            string[] input = new string[] { "abc", "=", "-" };
            Action action = () => Parser.Parse(input);
            Assert.Throws<CommandInputException>(action);
        }

        [Fact]
        public void Parser_WhenFirstToken_ContainsNoDash_ShouldThrow_CommandInputException()
        {
            string[] input = new string[] { "abc", "xyz" };
            Action action = () => Parser.Parse(input);
            Assert.Throws<CommandInputException>(action);
        }

        [Fact]
        public void Parser_WhenAnyOption_HasMultipleArguments_ShouldThrow_CommandInputException()
        {
            string[] input = new string[] { "-a", "a", "-b", "b", "-c", "c", "d" };
            Action action = () => Parser.Parse(input);
            Assert.Throws<CommandInputException>(action);
        }

        [Fact]
        public void Parser_WhenAnyOption_HasMultipleArguments_WithExplicitAssigns_ShouldThrow_CommandInputException()
        {
            string[] input = new string[] { "-a", "a", "-b", "b", "-c", "=", "c", "=", "d" };
            Action action = () => Parser.Parse(input);
            Assert.Throws<CommandInputException>(action);
        }

        [Fact]
        public void Parser_WhenCompoundFlag_ShouldUnrollCompoundFlag_Into_IndividualOptions()
        {
            string[] input = new string[] { "abc", "-xyz" };
            Command result = Parser.Parse(input);
            Assert.Equal("abc", result.Key);
            Assert.Collection<CommandOption>(result.GetOptions(),
                (x) => Assert.Equal("-x", x.Flag),
                (x) => Assert.Equal("-y", x.Flag),
                (x) => Assert.Equal("-z", x.Flag)
            );
        }

        [Fact]
        public void Parser_WhenCompoundFlag_And_ArgumentProvided_ShouldUnrollCompoundFlag_Into_IndividualOptions_AndAssign_LastOption_TheArgument()
        {
            string[] input = new string[] { "abc", "-xyz", "1" };
            Command result = Parser.Parse(input);
            Assert.Equal("abc", result.Key);
            Assert.Collection<CommandOption>(result.GetOptions(),
                (x) => { Assert.Equal("-x", x.Flag); Assert.False(x.HasArgument); },
                (x) => { Assert.Equal("-y", x.Flag); Assert.False(x.HasArgument); },
                (x) => { Assert.Equal("-z", x.Flag); Assert.True(x.HasArgument); Assert.Equal("1", x.Argument); }
            );
        }

        [Fact]
        public void Parser_WhenVerboseOptionFlagProvided_ShouldNotUnroll()
        {
            string[] input = new string[] { "abc", "--silent" };
            Command result = Parser.Parse(input);
            Assert.Equal("abc", result.Key);
            Assert.Single(result.GetOptions());
            Assert.Equal("--silent", result.GetOptions()[0].Flag);
        }

        [Fact]
        public void Parser_WhenFlag_ContainsExplicitEqualAssign_ShouldTreatEqual_AsPartOfFlag()
        {
            string[] input = new string[] { "abc", "--silent=1" };
            Command result = Parser.Parse(input);
            Assert.Equal("abc", result.Key);
            Assert.Equal("--silent=1", result.GetOptions()[0].Flag);
        }

        [Fact]
        public void Parser_WhenFlag_ContainsExplicitColonAssign_ShouldTreatColon_AsPartOfFlag()
        {
            string[] input = new string[] { "abc", "--silent:1" };
            Command result = Parser.Parse(input);
            Assert.Equal("abc", result.Key);
            Assert.Equal("--silent:1", result.GetOptions()[0].Flag);
        }

        [Fact]
        public void Parser_WhenCommandKey_ContainsExplicitEqualAssign_ShouldTreatEqual_AsPartOfCommandKey()
        {
            string[] input = new string[] { "abc=xyz", "--silent" };
            Command result = Parser.Parse(input);
            Assert.Equal("abc=xyz", result.Key);
            Assert.Equal("--silent", result.GetOptions()[0].Flag);
        }

        [Fact]
        public void Parser_WhenCommandKey_ContainsExplicitColonAssign_ShouldTreatColon_AsPartOfCommandKey()
        {
            string[] input = new string[] { "abc:xyz", "--silent" };
            Command result = Parser.Parse(input);
            Assert.Equal("abc:xyz", result.Key);
            Assert.Equal("--silent", result.GetOptions()[0].Flag);
        }
    }
}
