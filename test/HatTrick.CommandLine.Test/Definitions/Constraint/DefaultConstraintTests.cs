using Xunit;

namespace HatTrick.CommandLine.Test
{
    [Collection("Sequential")]
    public class DefaultConstraintTests
    {
        #region ensure
        [Fact]
        public void Ensure_When_EmptyOption_ShouldSwap_ForDefaultOption()
        {
            //ensure for the default constraint is a bit different than other constraints...
            //it fixes the op by swapping the empty option for a default option
            var constraint = new DefaultConstraint<int>("key", "--key", 99);
            Option option = new EmptyOption("key", "--key");

            constraint.Ensure(ref option);

            Assert.IsNotType<EmptyOption>(option);//option is no longer type EmptyOption
            Assert.IsType<DefaultOption>(option);//option is now type DefaultOption
            Assert.Equal(constraint.DefaultValue, option.GetValue<int>());//option holds the default value (99)
        }

        [Fact]
        public void Ensure_WhenNot_EmptyOption_ShouldNotSwap_ForDefaultOption()
        {
            //ensure for the default constraint is a bit different than other constraints...
            //it fixes the op by swapping the empty option for a default option
            var constraint = new DefaultConstraint<int>("key", "--key", 99);
            Option option = new Option("key", "--key", "88");

            constraint.Ensure(ref option);

            Assert.IsNotType<DefaultOption>(option);//option is no longer type EmptyOption
            Assert.IsType<Option>(option);//option is still just an option
            Assert.Equal("88", option.Argument);//option holds the provided arg (88)
        }
        #endregion

        #region usage
        [Fact]
        public void Usage_When_ArgumentNotProvided_ShouldSwap_ForDefault()
        {
            DefinitionRegistry.Clear();
            var cmdDef = new CommandDefinition("guid");
            cmdDef.Help = "help!";
            cmdDef.AddOption<int>(key: "count", 1, "Number of guids to generate.", ("-c", "--count"));
            cmdDef.AddOption<string>(key: "format", "N", "Guid output format string.", ("-f", "--format"));
            Action action = () => cmdDef["format"].AcceptedValues("N", "D", "B", "P", "X");
            cmdDef.Handler += (cmd) => {
                Assert.IsType<DefaultOption>(cmd["count"]);
                Assert.Equal(1, cmd["count"].GetValue<int>());
            };
            DefinitionRegistry.GetInstance().Add(cmdDef);

            string input = "guid --format X";//no count provided, should result in default of definition (1)s
            Command command = CommandBuilder.Build(input);
            CommandExecutor exe = DefinitionRegistry.GetInstance().GetCommandExecutor(command);
            exe.Execute();
        }

        [Fact]
        public void Usage_When_ArgumentProvided_ShouldNotSwap_ForDefault()
        {
            DefinitionRegistry.Clear();
            var cmdDef = new CommandDefinition("guid");
            cmdDef.Help = "help!";
            cmdDef.AddOption<int>(key: "count", 1, "Number of guids to generate.", ("-c", "--count"));
            cmdDef.AddOption<string>(key: "format", "N", "Guid output format string.", ("-f", "--format"));
            Action action = () => cmdDef["format"].AcceptedValues("N", "D", "B", "P", "X");
            cmdDef.Handler += (cmd) => {
                Assert.IsNotType<DefaultOption>(cmd["count"]);
                Assert.Equal(5, cmd["count"].GetValue<int>());
            };
            DefinitionRegistry.GetInstance().Add(cmdDef);

            string input = "guid --count 5 --format X";//no count provided, should result in default of definition (1)s
            Command command = CommandBuilder.Build(input);
            CommandExecutor exe = DefinitionRegistry.GetInstance().GetCommandExecutor(command);
            exe.Execute();
        }
        #endregion
    }
}
