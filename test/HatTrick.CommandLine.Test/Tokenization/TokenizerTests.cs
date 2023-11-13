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
        public void Tokenize_WhenVerboseOptionFlagProvided_ShouldNotUnroll()
        {
            string[] input = new string[] { "abc", "--silent" };
            Token[] result = Tokenizer.Tokenize(input);

            Assert.Collection(result,
                (x) => { Assert.Equal("abc", x.Value); Assert.IsAssignableFrom<CommandToken>(x); },
                (x) => { Assert.Equal("--silent", x.Value); Assert.IsAssignableFrom<VerboseFlagToken>(x); }
            );
        }
    }
}
