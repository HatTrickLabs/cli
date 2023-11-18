using System;
using HatTrick.CommandLine;
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
        public void Parse_WhenInput_IsEmpty_ShouldThrow_ArgumentNullException()
        {
            Command cmd = Parser.Parse(Array.Empty<Token>());
            Assert.Equal(DefaultCommandDefinition.DefaultCommandName, cmd.Name);
        }

        [Fact]
        public void Parse_WhenInput_SingleCommandToken_ShouldReturn_OptionlessCommand()
        {
            Command cmd = Parser.Parse(new Token[] { new CommandToken("go") });
            Assert.Equal("go", cmd.Name);
            Assert.True(cmd.Options.Length == 0);
        }

        [Fact]
        public void Parse_WhenExplicitAssign_AtIndex0_ShouldThrow_ArgumentNullException()
        {
            Action action = () => Parser.Parse(new Token[] { new ExplicitAssignToken("=") });
            Assert.Throws<CommandParseException>(action);
        }

        [Fact]
        public void Parse_WhenExplicitAssign_Not_FollowedBy_ArgumentToken_ShouldThrow_ArgumentNullException()
        {
            Action action = () => Parser.Parse(new Token[] { 
                new ExplicitAssignToken("="),
                new TerseFlagToken("-f")
            });
            Assert.Throws<CommandParseException>(action);
        }

        [Fact]
        public void Parse_WhenVerboseFlagToken_FollowedByArgument_ShouldResultIn_OptionWithArg()
        {
            Command cmd = Parser.Parse(new Token[] { 
                new CommandToken("go"),
                new VerboseFlagToken("--path"),
                new ArgumentToken("c:/tmp")
            });
            Assert.Equal("go", cmd.Name);
            Assert.True(cmd.Options.Length == 1);
            Assert.Equal("--path", cmd.Options[0].Flag);
            Assert.Equal("c:/tmp", cmd.Options[0].Argument);
        }

        [Fact]
        public void Parse_WhenTerseFlagToken_FollowedByArgument_ShouldResultIn_OptionWithArg()
        {
            Command cmd = Parser.Parse(new Token[] {
                new CommandToken("go"),
                new VerboseFlagToken("-p"),
                new ArgumentToken("c:/tmp")
            });
            Assert.Equal("go", cmd.Name);
            Assert.True(cmd.Options.Length == 1);
            Assert.Equal("-p", cmd.Options[0].Flag);
            Assert.Equal("c:/tmp", cmd.Options[0].Argument);
        }

        [Fact]
        public void Parse_WhenCompoundTerseFlagToken_ShouldUnrollInto_TerseFlagTokenSet()
        {
            Command cmd = Parser.Parse(new Token[] {
                new CommandToken("go"),
                new CompoundTerseFlagToken("-abc")
            });
            Assert.Equal("go", cmd.Name);
            Assert.True(cmd.Options.Length == 3);
            Assert.Equal("-a", cmd.Options[0].Flag);
            Assert.Equal("-b", cmd.Options[1].Flag);
            Assert.Equal("-c", cmd.Options[2].Flag);
        }

        [Fact]
        public void Parse_WhenCompoundTerseFlagToken_FollowedByArgument_ShouldUnrollInto_TerseFlagTokenSet_LastHavingArg()
        {
            Command cmd = Parser.Parse(new Token[] {
                new CommandToken("go"),
                new CompoundTerseFlagToken("-abc"),
                new ArgumentToken("xyz")
            });
            Assert.Equal("go", cmd.Name);
            Assert.True(cmd.Options.Length == 3);
            Assert.Equal("-a", cmd.Options[0].Flag);
            Assert.Equal("-b", cmd.Options[1].Flag);
            Assert.Equal("-c", cmd.Options[2].Flag);
            Assert.Equal("xyz", cmd.Options[2].Argument);
        }

        [Fact]
        public void Parse_WhenCompoundTerseFlagToken_FollowedByExplicitAssign_ThenArgument_ShouldUnrollInto_TerseFlagTokenSet_LastHavingArg()
        {
            Command cmd = Parser.Parse(new Token[] {
                new CommandToken("go"),
                new CompoundTerseFlagToken("-abc"),
                new ExplicitAssignToken(":"),
                new ArgumentToken("xyz")
            });
            Assert.Equal("go", cmd.Name);
            Assert.True(cmd.Options.Length == 3);
            Assert.Equal("-a", cmd.Options[0].Flag);
            Assert.Equal("-b", cmd.Options[1].Flag);
            Assert.Equal("-c", cmd.Options[2].Flag);
            Assert.Equal("xyz", cmd.Options[2].Argument);
        }

        [Fact]
        public void Parse_WhenArgumentToken_And_OptionsLengthIs_Zero_ShouldThrow_CommandParseException()
        {
            Action action = () => Parser.Parse(new Token[] {
                new CommandToken("my-command"),
                new ArgumentToken("arg")
            });
            Assert.Throws<CommandParseException>(action);
        }

        [Fact]
        public void Parse_WhenArgumentToken_And_PreviousParsedOption_AlreadyHasArgument_ShouldThrow_CommandParseException()
        {
            Action action = () => Parser.Parse(new Token[] {
                new CommandToken("my-command"),
                new TerseFlagToken("-f"),
                new ArgumentToken("arg1"),
                new ArgumentToken("arg2")
            });
            Assert.Throws<CommandParseException>(action);
        }

        [Fact]
        public void Parse_WhenExplicitAssignToken_And_PreviousParsedOption_AlreadyHasArgument_ShouldThrow_CommandParseException()
        {
            Action action = () => Parser.Parse(new Token[] {
                new CommandToken("my-command"),
                new TerseFlagToken("-f"),
                new ArgumentToken("arg1"),
                new ExplicitAssignToken(":"),
                new ArgumentToken("arg2")
            }); ;
            Assert.Throws<CommandParseException>(action);
        }
    }
}
