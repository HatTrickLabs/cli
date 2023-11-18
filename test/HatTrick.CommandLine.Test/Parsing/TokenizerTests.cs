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
        public void Tokenize_WhenArg_IsExplicitAssignEqual_ShouldTokenize_AsExplicitAssign()
        {
            string[] input = new string[] { "=" };
            Token[] tokens = Tokenizer.Tokenize(input);
            Assert.Collection<Token>(tokens,
                (x) => Assert.IsAssignableFrom<ExplicitAssignToken>(x)
            );
        }

        [Fact]
        public void Tokenize_WhenArg_IsExplicitAssignColon_ShouldTokenize_AsExplicitAssign()
        {
            string[] input = new string[] { ":" };
            Token[] tokens = Tokenizer.Tokenize(input);
            Assert.Collection<Token>(tokens,
                (x) => Assert.IsAssignableFrom<ExplicitAssignToken>(x)
            );
        }

        [Fact]
        public void Tokenize_WhenArgAtIndex_Zero_DoesNot_StartWithDash_ShouldTokenize_AsCommandToken()
        {
            string[] input = new string[] { "abc" };
            Token[] tokens = Tokenizer.Tokenize(input);
            Assert.IsAssignableFrom<CommandToken>(tokens[0]);
        }

        [Fact]
        public void Tokenize_WhenArgAtIndex_GreaterThanZero_DoesNot_StartWithDash_ShouldTokenize_AsArgument()
        {
            string[] input = new string[] { "abc", "-f", "xyz" };
            Token[] tokens = Tokenizer.Tokenize(input);
            Assert.IsAssignableFrom<ArgumentToken>(tokens[2]);
        }

        [Fact]
        public void Tokenize_WhenArg_StartsWith_SingleDash_ShouldTokenize_AsTerseFlag()
        {
            string[] input = new string[] { "-a", "test" };
            Token[] tokens = Tokenizer.Tokenize(input);
            Assert.IsAssignableFrom<TerseFlagToken>(tokens[0]);
        }

        [Fact]
        public void Tokenize_WhenArg_StartsWith_DoubleDash_ShouldTokenize_AsVerboseFlag()
        {
            string[] input = new string[] { "--abc", "test" };
            Token[] tokens = Tokenizer.Tokenize(input);
            Assert.IsAssignableFrom<VerboseFlagToken>(tokens[0]);
        }

        [Fact]
        public void Tokenize_WhenArg_StartsWith_SingleDash_And_Contains_MultipleChars_ShouldTokenize_AsCompoundTerseFlag()
        {
            string[] input = new string[] { "-abc", "test" };
            Token[] tokens = Tokenizer.Tokenize(input);
            Assert.IsAssignableFrom<CompoundTerseFlagToken>(tokens[0]);
        }
    }
}
