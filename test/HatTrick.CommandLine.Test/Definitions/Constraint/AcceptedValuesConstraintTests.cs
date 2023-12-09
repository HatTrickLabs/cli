using Xunit;

namespace HatTrick.CommandLine.Test
{
    [Collection("Sequential")]
    public class AcceptedValuesConstraintTests
    {
        #region constructor
        [Fact]
        public void Constructor_WhenValues_IsNull_ShouldTrhow_ArgumentNullException()
        {
            Action action = () => new AcceptedValuesConstraint<string>(null);
            Assert.Throws<ArgumentNullException>(action);
        }
        #endregion

        #region ensure
        [Fact]
        public void Ensure_WhenConstraintFails_ShouldThrow_OptionArgumentException()
        {
            var constraint = new AcceptedValuesConstraint<int>(new int[] { 1, 2, 3, 4, 5, 6, 7, 9, 10 });
            var option = new Option("key", "flag", "8");
            option.SetValue(int.Parse(option.Argument));
            Action action = () => constraint.Ensure(ref option);
        }

        [Fact]
        public void Ensure_WhenConstraintPasses_shouldPass()
        {
            var constraint = new AcceptedValuesConstraint<int>(new int[] { 1, 2, 3, 4, 5, 6, 7, 9, 10 });
            var option = new Option("key", "flag", "7");
            option.SetValue(int.Parse(option.Argument));
            Action action = () => constraint.Ensure(ref option);
        }
        #endregion

        #region usage
        [Fact]
        public void Usage_WhenConstraintInvalid_DueToUnIncludedDefault_ShouldThrow_CommandDefinitionException()
        {
            DefinitionRegistry.Clear();
            var cmdDef = new CommandDefinition("guid");
            cmdDef.AddOption<int>(key: "count", 1, "Number of guids to generate.", ("-c", "--count"));
            cmdDef.AddOption<string>(key: "format", "Z", "Guid output format string.", ("-f", "--format"));
            Action action = () => cmdDef["format"].AcceptedValues("N", "D", "B", "P", "X");//does not include Z

            Assert.Contains("is defined with a default value of 'Z' which is not defined in the accepted values set", 
                Assert.Throws<CommandDefinitionException>(action).Message);
        }

        [Fact]
        public void Usage_WhenConstraintInvalid_DueToPreexisting_AcceptedValuesConstraint_ShouldThrow_CommandDefinitionException()
        {
            DefinitionRegistry.Clear();
            var cmdDef = new CommandDefinition("guid");
            cmdDef.AddOption<int>(key: "count", 1, "Number of guids to generate.", ("-c", "--count"));
            cmdDef.AddOption<string>(key: "format", "X", "Guid output format string.", ("-f", "--format"));
            Action action = () =>
            {
                cmdDef["format"].AcceptedValues("N", "D", "B", "P", "X");
                cmdDef["format"].AcceptedValues("N", "D", "B", "P", "X");
            };


            Assert.Contains($"already contains an {nameof(OptionDefinition.AcceptedValues)} constraint.",
                Assert.Throws<CommandDefinitionException>(action).Message);
        }

        [Fact]
        public void Usage_WhenConstraintFails_ShouldThrow_OptionArgumentException()
        {
            DefinitionRegistry.Clear();
            var cmdDef = new CommandDefinition("guid");
            cmdDef.AddOption<int>(key: "count", 1, "Number of guids to generate.", ("-c", "--count"));
            cmdDef.AddOption<string>(key: "format", "N", "Guid output format string.", ("-f", "--format"));
            cmdDef["format"].AcceptedValues("N", "D", "B", "P", "X");
            cmdDef.Handler += (cmd) => { };
            DefinitionRegistry.GetInstance().Add(cmdDef);

            string input = "guid -c 5 -f Z";
            Command command = CommandBuilder.Build(input);
            CommandExecutor exe = DefinitionRegistry.GetInstance().GetCommandExecutor(command);
            Action action = () => exe.Execute();

            Assert.Throws<OptionArgumentException>(action);
        }

        [Fact]
        public void Usage_WhenConstraintPasses_ByProvidedArg_ShouldPass()
        {
            DefinitionRegistry.Clear();
            var cmdDef = new CommandDefinition("guid");
            cmdDef.AddOption<int>(key: "count", 1, "Number of guids to generate.", ("-c", "--count"));
            cmdDef.AddOption<string>(key: "format", "N", "Guid output format string.", ("-f", "--format"));
            cmdDef["format"].AcceptedValues("N", "D", "B", "P", "X");
            cmdDef.Handler += (cmd) => { };
            DefinitionRegistry.GetInstance().Add(cmdDef);

            string input = "guid -c 5 -f X";
            Command command = CommandBuilder.Build(input);
            CommandExecutor exe = DefinitionRegistry.GetInstance().GetCommandExecutor(command);
            exe.Execute();
        }

        [Fact]
        public void Usage_WhenConstraintPasses_ByDefaultArg_ShouldPass()
        {
            DefinitionRegistry.Clear();
            var cmdDef = new CommandDefinition("guid");
            cmdDef.AddOption<int>(key: "count", 1, "Number of guids to generate.", ("-c", "--count"));
            cmdDef.AddOption<string>(key: "format", "N", "Guid output format string.", ("-f", "--format"));
            cmdDef["format"].AcceptedValues("N", "D", "B", "P", "X");
            cmdDef.Handler += (cmd) => { };
            DefinitionRegistry.GetInstance().Add(cmdDef);

            string input = "guid -c 5";
            Command command = CommandBuilder.Build(input);
            CommandExecutor exe = DefinitionRegistry.GetInstance().GetCommandExecutor(command);
            exe.Execute();
        }
        #endregion
    }
}
