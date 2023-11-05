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

        [Fact]
        public void Tokenize_WhenAnyArg_IsSignleDash_ShouldThrow_CommandInputException()
        {
            string[] input = new string[] { "abc", "=", "-" };
            Action action = () => Tokenizer.Tokenize(input);
            Assert.Throws<CommandInputException>(action);
        }

        [Fact]
        public void Tokenize_WhenArgAtIndexZero_DoesNot_StartWithDash_ShouldTokenize_AsCommandToken()
        {
            string[] input = new string[] { "abc" };
            Token[] tokens = Tokenizer.Tokenize(input);
            Assert.IsAssignableFrom<CommandToken>(tokens[0]);
        }

        [Fact]
        public void Tokenize_WhenArgAtIndex_GreaterThan_Zero_DoesNot_StartWithDash_ShouldTokenize_AsArgument()
        {
            string[] input = new string[] { "abc", "-f", "xyz" };
            Token[] tokens = Tokenizer.Tokenize(input);
            Assert.IsAssignableFrom<ArgumentToken>(tokens[2]);
        }

        [Fact]
        public void Tokenize_WhenArg_IsLength2_AndOnlyContainsDashes_ShouldThrow_CommandInputException()
        {
            string[] input = new string[] { "abc", "--", "y" };
            Action action = () => Tokenizer.Tokenize(input);
            Assert.Throws<CommandInputException>(action);
        }

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
