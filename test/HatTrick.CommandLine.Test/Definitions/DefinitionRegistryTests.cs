using Xunit;

namespace HatTrick.CommandLine.Test
{
    public class DefinitionRegistryTests
    {
        #region get instance
        [Fact]
        public void GetInstance_SingletonInstance_ShouldContain_DefaultCommandDefinition()
        {
            DefinitionRegistry registry = DefinitionRegistry.GetInstance();
            CommandDefinition cmdDef = registry.GetCommandDefinition(CommandDefinition.DefaultCommandName);
            Assert.NotNull(cmdDef);
            Assert.Equal(DefaultCommandDefinition.DefaultCommandName, cmdDef.Name);
        }
        #endregion
    }
}
