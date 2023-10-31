using Xunit;

namespace HatTrick.CommandLine.Test
{
    public class TokenizerTests
    {
        [Fact]
        public void Tokenize_WhenInput_IsNull_ShouldThrow_ArgumentNullException()
        {
            Action action = () => Tokenizer.Tokenize(null);
            Assert.Throws<ArgumentNullException>(action);
        }

        [Fact]
        public void Tokenize_WhenInput_IsEmpty_ShouldReturn_EmptyTokenArray()
        {
            string[] input = Array.Empty<string>();
            Token[] result = Tokenizer.Tokenize(input);
            Assert.Empty(result);
        }

        //[Fact]
        //public void Tokenize_WhenInput_IsEmpty_ShouldReturn_Command_WithoutOptions()
        //{
        //    string[] input = Array.Empty<string>();
        //    Command result = Tokenizer.Tokenize(input);
        //    Assert.Equal(Array.Empty<Option>(), result.GetOptions());
        //}

        //[Fact]
        //public void Tokenize_WhenInput_IsCommandlessWithOptions_ShouldReturn_DefaultCommand_WithOptions()
        //{
        //    string[] input = new string[] { "-a", "-b" };
        //    Command result = Tokenizer.Tokenize(input);
        //    Assert.Equal(CommandDefinition.DefaultCommandName, result.Name);
        //    Assert.Collection<Option>(result.GetOptions(),
        //        (x) => Assert.Equal("-a", x.Flag),
        //        (x) => Assert.Equal("-b", x.Flag)
        //    );
        //}

        //[Fact]
        //public void Tokenize_WhenInput_HasCommandWithOptions_ShouldReturn_Command_WithOptions()
        //{
        //    string[] input = new string[] { "test", "-a", "-b" };
        //    Command result = Tokenizer.Tokenize(input);
        //    Assert.Equal("test", result.Name);
        //    Assert.Collection<Option>(result.GetOptions(),
        //        (x) => Assert.Equal("-a", x.Flag),
        //        (x) => Assert.Equal("-b", x.Flag)
        //    );
        //}

        //[Fact]
        //public void Tokenize_WhenInput_ContainsArgumentTokenStartingWithDash_WithNoExplicitAssign_Should_TreatTokenAsFlag()
        //{
        //    string[] input = new string[] { "add", "-x", "-3", "-y", "3" };
        //    Command result = Tokenizer.Tokenize(input);
        //    Assert.Equal("add", result.Name);
        //    Assert.Collection<Option>(result.GetOptions(),
        //        (x) => Assert.Equal("-x", x.Flag),
        //        (x) => Assert.Equal("-3", x.Flag),
        //        (x) => Assert.Equal("-y", x.Flag)
        //    );
        //}

        //[Fact]
        //public void Tokenize_WhenInput_ContainsArgumentTokenStartingWithDash_WithExplicitEqualAssign_Should_TreatTokenAsArgument()
        //{
        //    string[] input = new string[] { "add", "-x", "=", "-3", "-y", "3" };
        //    Command result = Tokenizer.Tokenize(input);
        //    Assert.Equal("add", result.Name);
        //    Assert.Collection<Option>(result.GetOptions(),
        //        (x) => { Assert.Equal("-x", x.Flag); Assert.Equal("-3", x.Argument); },
        //        (x) => Assert.Equal("-y", x.Flag)
        //    );
        //}

        //[Fact]
        //public void Tokenize_WhenInput_ContainsArgumentTokenStartingWithDash_WithExplicitColonAssign_Should_TreatTokenAsArgument()
        //{
        //    string[] input = new string[] { "add", "-x", ":", "-3", "-y", "3" };
        //    Command result = Tokenizer.Tokenize(input);
        //    Assert.Equal("add", result.Name);
        //    Assert.Collection<Option>(result.GetOptions(),
        //        (x) => { Assert.Equal("-x", x.Flag); Assert.Equal("-3", x.Argument); },
        //        (x) => { Assert.Equal("-y", x.Flag); Assert.Equal("3", x.Argument); }
        //    );
        //}

        //[Fact]
        //public void Tokenize_WhenFirstOptionToken_IsExplicitAssignEqualToken_ShouldThrow_CommandInputException()
        //{
        //    string[] input = new string[] { "abc", "=", "-x" };
        //    Action action = () => Tokenizer.Tokenize(input);
        //    Assert.Throws<CommandInputException>(action);
        //}

        [Fact]
        public void Tokenize_WhenAnyToken_IsSingleDashOnly_ShouldThrow_CommandInputException()
        {
            string[] input = new string[] { "abc", "=", "-" };
            Action action = () => Tokenizer.Tokenize(input);
            Assert.Throws<CommandInputException>(action);
        }

        //[Fact]
        //public void Tokenize_WhenFirstToken_ContainsNoDash_ShouldThrow_CommandInputException()
        //{
        //    string[] input = new string[] { "abc", "xyz" };
        //    Action action = () => Tokenizer.Tokenize(input);
        //    Assert.Throws<CommandInputException>(action);
        //}

        //[Fact]
        //public void Tokenize_WhenAnyOption_HasMultipleArguments_ShouldThrow_CommandInputException()
        //{
        //    string[] input = new string[] { "-a", "a", "-b", "b", "-c", "c", "d" };
        //    Action action = () => Tokenizer.Tokenize(input);
        //    Assert.Throws<CommandInputException>(action);
        //}

        //[Fact]
        //public void Tokenize_WhenAnyOption_HasMultipleArguments_WithExplicitAssigns_ShouldThrow_CommandInputException()
        //{
        //    string[] input = new string[] { "-a", "a", "-b", "b", "-c", "=", "c", "=", "d" };
        //    Action action = () => Tokenizer.Tokenize(input);
        //    Assert.Throws<CommandInputException>(action);
        //}

        [Fact]
        public void Tokenize_WhenCompoundFlag_ShouldUnrollCompoundFlag_Into_IndividualTokens()
        {
            string[] input = new string[] { "abc", "-xyz" };
            Token[] result = Tokenizer.Tokenize(input);
            Assert.Collection<Token>(result,
                (x) => Assert.Equal("abc", x.Value),
                (x) => Assert.Equal("-x", x.Value),
                (x) => Assert.Equal("-y", x.Value),
                (x) => Assert.Equal("-z", x.Value)
            );
        }

        [Fact]
        public void Tokenize_WhenCompoundFlag_And_ArgumentProvided_ShouldUnrollCompoundFlag_Into_IndividualFlagTokens_PlusArgument()
        {
            string[] input = new string[] { "abc", "-xyz", "1" };
            Token[] result = Tokenizer.Tokenize(input);
            Assert.Collection<Token>(result,
                (x) => { Assert.Equal("abc", x.Value); Assert.IsAssignableFrom<CommandToken>(x); },
                (x) => { Assert.Equal("-x", x.Value); Assert.IsAssignableFrom<TerseFlagToken>(x); },
                (x) => { Assert.Equal("-y", x.Value); Assert.IsAssignableFrom<TerseFlagToken>(x); },
                (x) => { Assert.Equal("-z", x.Value); Assert.IsAssignableFrom<TerseFlagToken>(x); },
                (x) => { Assert.Equal("1", x.Value); Assert.IsAssignableFrom<ArgumentToken>(x); }
            );
        }

        [Fact]
        public void Tokenize_WhenCompoundFlag_And_ArgumentProvided_WithExplicitAssignViaColon_ShouldUnrollCompoundFlag_Into_IndividualFlagTokens_PlusTheFinalArgumentToken()
        {
            string[] input = new string[] { "abc", "-xyz:1" };
            Token[] result = Tokenizer.Tokenize(input);
            Assert.Collection<Token>(result,
                (x) => { Assert.Equal("abc", x.Value); Assert.IsAssignableFrom<CommandToken>(x); },
                (x) => { Assert.Equal("-x", x.Value); Assert.IsAssignableFrom<TerseFlagToken>(x); },
                (x) => { Assert.Equal("-y", x.Value); Assert.IsAssignableFrom<TerseFlagToken>(x); },
                (x) => { Assert.Equal("-z", x.Value); Assert.IsAssignableFrom<TerseFlagToken>(x); },
                (x) => { Assert.Equal(":", x.Value); Assert.IsAssignableFrom<ExplicitAssignToken>(x); },
                (x) => { Assert.Equal("1", x.Value); Assert.IsAssignableFrom<ArgumentToken>(x); }
            );
        }

        [Fact]
        public void Tokenize_WhenCompoundFlag_And_ArgumentProvided_WithExplicitAssignViaEqual_ShouldUnrollCompoundFlag_Into_IndividualFlagTokens_PlusTheFinalArgumentToken()
        {
            string[] input = new string[] { "abc", "-xyz=1" };
            Token[] result = Tokenizer.Tokenize(input);
            Assert.Collection<Token>(result,
                (x) => { Assert.Equal("abc", x.Value); Assert.IsAssignableFrom<CommandToken>(x); },
                (x) => { Assert.Equal("-x", x.Value); Assert.IsAssignableFrom<TerseFlagToken>(x); },
                (x) => { Assert.Equal("-y", x.Value); Assert.IsAssignableFrom<TerseFlagToken>(x); },
                (x) => { Assert.Equal("-z", x.Value); Assert.IsAssignableFrom<TerseFlagToken>(x); },
                (x) => { Assert.Equal("=", x.Value); Assert.IsAssignableFrom<ExplicitAssignToken>(x); },
                (x) => { Assert.Equal("1", x.Value); Assert.IsAssignableFrom<ArgumentToken>(x); }
            );
        }

        [Fact]
        public void Tokenize_WhenVerboseOptionFlagProvided_ShouldNotUnroll()
        {
            string[] input = new string[] { "abc", "--silent" };
            Token[] result = Tokenizer.Tokenize(input);

            Assert.Collection(result,
                (x) => { Assert.Equal("abc", x.Value); Assert.IsAssignableFrom<CommandToken>(x); },
                (x) => { Assert.Equal("--silent", x.Value); Assert.IsAssignableFrom<VerboseFlagToken>(x); }
            );
        }

        [Fact]
        public void Tokenize_WhenFlag_ContainsExplicitEqualAssign_ShouldsplitAsFlagAndArg()
        {
            string[] input = new string[] { "abc", "--silent=1" };
            Token[] result = Tokenizer.Tokenize(input);
            Assert.Collection<Token>(result,
                (x) => { Assert.Equal("abc", x.Value); Assert.IsAssignableFrom<CommandToken>(x); },
                (x) => { Assert.Equal("--silent", x.Value); Assert.IsAssignableFrom<VerboseFlagToken>(x); },
                (x) => { Assert.Equal("=", x.Value); Assert.IsAssignableFrom<ExplicitAssignToken>(x); },
                (x) => { Assert.Equal("1", x.Value); Assert.IsAssignableFrom<ArgumentToken>(x); }
            );
        }

        [Fact]
        public void Tokenize_WhenFlag_ContainsExplicitColonAssign_ShouldSplitAsFlagAndArg()
        {
            string[] input = new string[] { "abc", "--silent:1" };
            Token[] result = Tokenizer.Tokenize(input);
            Assert.Collection<Token>(result,
                (x) => { Assert.Equal("abc", x.Value); Assert.IsAssignableFrom<CommandToken>(x); },
                (x) => { Assert.Equal("--silent", x.Value); Assert.IsAssignableFrom<VerboseFlagToken>(x); },
                (x) => { Assert.Equal(":", x.Value); Assert.IsAssignableFrom<ExplicitAssignToken>(x); },
                (x) => { Assert.Equal("1", x.Value); Assert.IsAssignableFrom<ArgumentToken>(x); }
            );
        }
    }
}
