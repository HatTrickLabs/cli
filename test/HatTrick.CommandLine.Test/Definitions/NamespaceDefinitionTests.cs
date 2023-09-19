using Xunit;

namespace HatTrick.CommandLine.Test
{
    public class NamespaceDefinitionTests
    {
        //Test anything that throw ex and Validate();
        #region constructor tests
        [Fact]
        public void Constructor_WhenNameArgumentIsNull_ShouldThrow_ArgumentNullException()
        {
            Action action = () => new NamespaceDefinition(null, "help");
            Assert.Throws <ArgumentNullException>(action);
        }

        [Fact]
        public void Constructor_WhenHelpArgumentIsNull_ShouldThrow_ArgumentNullException()
        {
            Action action = () => new NamespaceDefinition("name", null);
            Assert.Throws<ArgumentNullException>(action);
        }
        #endregion

        #region validate tests
        [Fact]
        public void Validate_WhenNameIsEmpty_ShouldThrow_NamespaceDefinitionException()
        {
            var namespaceDef = new NamespaceDefinition(string.Empty, "help");
            Action action = () => namespaceDef.Validate();
            Assert.Throws<NamespaceDefinitionException>(action);
        }

        [Fact]
        public void Validate_WhenHelpIsEmpty_ShouldThrow_NamespaceDefinitionException()
        {
            var namespaceDef = new NamespaceDefinition("name", string.Empty);
            Action action = () => namespaceDef.Validate();
            Assert.Throws<NamespaceDefinitionException>(action);
        }

        [Fact]
        public void Validate_WhenNameBeginsWithDash_ShouldThrow_NamespaceDefinitionException()
        {
            var namespaceDef = new NamespaceDefinition("-name", string.Empty);
            Action action = () => namespaceDef.Validate();
            Assert.Throws<NamespaceDefinitionException>(action);
        }

        [Fact]
        public void Validate_WhenNameLength_GreaterThanMaxNameLength_ShouldThrow_NamespaceDefinitionException()
        {
            var namespaceDef = new NamespaceDefinition(new string('x', NamespaceDefinition.MaxNameLength + 1), string.Empty);
            Action action = () => namespaceDef.Validate();
            Assert.Throws<NamespaceDefinitionException>(action);
        }

        [Fact]
        public void Validate_WhenNameContainsInvalidChar_ShouldThrow_NamespaceDefinitionException()
        {
            var namespaceDef = new NamespaceDefinition("name+abc", string.Empty);
            Action action = () => namespaceDef.Validate();
            Assert.Throws<NamespaceDefinitionException>(action);
        }

        [Fact]
        public void Validate_WhenNameContainsOneDot_ShouldExposeDepthOfOne()
        {
            var namespaceDef = new NamespaceDefinition("abc.xyz", "help");
            namespaceDef.Validate();
            Assert.Equal(1, namespaceDef.Depth);
        }

        [Fact]
        public void Validate_WhenNameContainsTwoDots_ShouldExposeDepthOfTwo()
        {
            var namespaceDef = new NamespaceDefinition("abc.xyz.xxx", "help");
            namespaceDef.Validate();
            Assert.Equal(2, namespaceDef.Depth);
        }

        [Fact]
        public void Validate_WhenNameContainsThreeDots_ShouldExposeDepthOfThree()
        {
            var namespaceDef = new NamespaceDefinition("abc.xyz.xxx.aaa", "help");
            namespaceDef.Validate();
            Assert.Equal(3, namespaceDef.Depth);
        }
        #endregion

        #region hide tests
        [Fact]
        public void Hide_ShouldChangeState_ToHidden()
        {
            var namespaceDef = new NamespaceDefinition("name", "help");
            namespaceDef.Hide();
            Assert.True(namespaceDef.Hidden);
        }
        #endregion
    }
}
