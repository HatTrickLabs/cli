using Xunit;

namespace HatTrick.CommandLine.Test
{
    public class NamespaceDefinitionTests
    {
        #region constructor
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

        #region is valid namespace char
        [Fact]
        public void IsValidNamespaceChar_WhenValidCharProvided_ShouldReturnTrue()
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
                Assert.True(NamespaceDefinition.IsValidNamespaceChar(c));

                if (i == 46)
                    i = 47;

                if (i == 57)
                    i = 64;

                if (i == 90)
                    i = 96;
            }
        }

        [Fact]
        public void IsValidNamespaceChar_WhenInValidCharProvided_ShouldReturnFalse()
        {

            char c = '$';
            Assert.False(NamespaceDefinition.IsValidNamespaceChar(c));
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

        #region validate
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
        public void Validate_WhenNameStartsWhithNonLetterChar_ShouldThrow_NamespaceDefinitionException()
        {
            var set = new SetOfNamespaceDefinition();
            Action action1 = () => set.Add(new NamespaceDefinition(".abc", "help"));
            Action action2 = () => set.Add(new NamespaceDefinition("-abc", "help"));
            Action action3 = () => set.Add(new NamespaceDefinition("1abc", "help"));
            Action action4 = () => set.Add(new NamespaceDefinition("+abc", "help"));
            Assert.Throws<NamespaceDefinitionException>(action1);
            Assert.Throws<NamespaceDefinitionException>(action2);
            Assert.Throws<NamespaceDefinitionException>(action3);
            Assert.Throws<NamespaceDefinitionException>(action4);
        }

        [Fact]
        public void Validate_WhenNameLength_GreaterThanMaxNameLength_ShouldThrow_NamespaceDefinitionException()
        {
            var namespaceDef = new NamespaceDefinition(new string('x', NamespaceDefinition.MaxNameLength + 1), string.Empty);
            Action action = () => namespaceDef.Validate();
            Assert.Throws<NamespaceDefinitionException>(action);
        }

        [Fact]
        public void Validate_WhenNameContainsOneOrMoreDots_ShouldExposeDepthAsDotCount()
        {
            var namespaceDef1 = new NamespaceDefinition("abc.xyz", "help");
            namespaceDef1.Validate();

            var namespaceDef2 = new NamespaceDefinition("abc.xyz.xxx", "help");
            namespaceDef2.Validate();

            var namespaceDef3 = new NamespaceDefinition("abc.xyz.xxx.aaa", "help");
            namespaceDef3.Validate();

            var namespaceDef4 = new NamespaceDefinition("abc.xyz.xxx.aaa.bbb", "help");
            namespaceDef4.Validate();

            var namespaceDef5 = new NamespaceDefinition("abc.xyz.xxx.aaa.bbb.ccc", "help");
            namespaceDef5.Validate();

            Assert.Equal(1, namespaceDef1.Depth);
            Assert.Equal(2, namespaceDef2.Depth);
            Assert.Equal(3, namespaceDef3.Depth);
            Assert.Equal(4, namespaceDef4.Depth);
            Assert.Equal(5, namespaceDef5.Depth);
        }

        [Fact]
        public void Validate_WhenNameEndsWithDot_ShouldThrow_NamespaceDefinitionException()
        {
            var namespaceDef = new NamespaceDefinition("abc.xyz.", "help");
            Action action = () => namespaceDef.Validate();
            Assert.Throws<NamespaceDefinitionException>(action);
        }
        #endregion
    }
}
