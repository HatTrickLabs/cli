using Xunit;

namespace HatTrick.CommandLine.Test
{
    [Collection("Sequential")]
    public class CustomCommandConstraintTests
    {
        #region constructors
        [Fact]
        public void Construcotor_WhenConstraint_IsNull_ShouldThrow_ArgumentNullException()
        {
            Action action = () => new CustomCommandConstraint(null, "name", "description");
            Assert.Contains("constraint", Assert.Throws<ArgumentNullException>(action).Message);
        }

        [Fact]
        public void Construcotor_WhenName_IsNull_ShouldThrow_ArgumentNullException()
        {
            Action action = () => new CustomCommandConstraint((cnd) => true, null, "description");
            Assert.Contains("name", Assert.Throws<ArgumentNullException>(action).Message);
        }

        [Fact]
        public void Construcotor_WhenDescription_IsNull_ShouldThrow_ArgumentNullException()
        {
            Action action = () => new CustomCommandConstraint((cmd) => true, "name", null);
            Assert.Contains("description", Assert.Throws<ArgumentNullException>(action).Message);
        }

        [Fact]
        public void Construcotor_WhenName_IsEmpty_ShouldThrow_ArgumentException()
        {
            Action action = () => new CustomCommandConstraint((cnd) => true, string.Empty, "description");
            Assert.Contains("name", Assert.Throws<ArgumentException>(action).Message);
        }

        [Fact]
        public void Construcotor_WhenDescription_IsEmpty_ShouldThrow_ArgumentException()
        {
            Action action = () => new CustomCommandConstraint((cmd) => true, "name", string.Empty);
            Assert.Contains("description", Assert.Throws<ArgumentException>(action).Message);
        }
        #endregion

        #region ensure
        [Fact]
        public void Ensure_WhenConstraintFails_ShouldThrow_CommandInputException()
        {
            var op1 = new Option("key1", "-1", "1");
            var op2 = new Option("key2", "-2", "2");
            var op3 = new Option("key3", "-3", "3");
            var op4 = new Option("key4", "-4", "4");
            var command = new Command("go", op1, op2, op3, op4);
            var constraint = new CustomCommandConstraint((cmd) => cmd["key2"].Argument == "3", "name", "description");
            Action action = () => constraint.Ensure(command);
            Assert.Throws <CommandInputException>(action);
        }

        [Fact]
        public void Ensure_WhenConstraintPasses_ShouldPass()
        {
            var op1 = new Option("key1", "-1", "1");
            var op2 = new Option("key2", "-2", "2");
            var op3 = new Option("key3", "-3", "3");
            var op4 = new Option("key4", "-4", "4");
            var command = new Command("go", op1, op2, op3, op4);
            var constraint = new CustomCommandConstraint((cmd) => cmd["key2"].Argument == "2", "name", "description");
            constraint.Ensure(command);
        }
        #endregion

        #region usage
        [Fact]
        public void Usage_WhenConstraintConditionPasses_ShouldPass()
        {
            DefinitionRegistry.Clear();
            var cmdDef = new CommandDefinition("go");
            cmdDef.Help = "help!";
            cmdDef.AddOption<string>(key: "fn", help: "fn help", ("-f", "--fn"));
            cmdDef.AddOption<string>(key: "ln", help: "ln help", ("-l", "--ln"));
            cmdDef.AddOption<int>(key: "age", help: "age help", ("-a", "--age"));

            cmdDef.ApplyConstraint((cmd) => cmd["age"].GetValue<int>() > 7, "age restriction", "age > 7");

            cmdDef.Handler = (cmd) =>
            {
                Assert.Equal("Charlie", cmd["fn"].GetValue<string>());
                Assert.Equal("Brown", cmd["ln"].GetValue<string>());
                Assert.Equal(8, cmd["age"].GetValue<int>());
            };

            DefinitionRegistry.GetInstance().Add(cmdDef);

            string input = "go -f Charlie -l Brown -a 8";
            Command cmd = CommandBuilder.Build(input);
            CommandExecutor exe = DefinitionRegistry.GetInstance().GetCommandExecutor(cmd);
            exe.Execute();
        }

        [Fact]
        public void Usage_WhenConstraintConditionFails_ShouldThrow_CommandInputException()
        {
            DefinitionRegistry.Clear();
            var cmdDef = new CommandDefinition("go");
            cmdDef.Help = "help!";
            cmdDef.AddOption<string>(key: "fn", help: "fn help", ("-f", "--fn"));
            cmdDef.AddOption<string>(key: "ln", help: "ln help", ("-l", "--ln"));
            cmdDef.AddOption<int>(key: "age", help: "age help", ("-a", "--age"));

            cmdDef.ApplyConstraint((cmd) => cmd["age"].GetValue<int>() > 8, "age restriction", "age > 8");

            cmdDef.Handler = (cmd) =>
            {
                Assert.Equal("Charlie", cmd["fn"].GetValue<string>());
                Assert.Equal("Brown", cmd["ln"].GetValue<string>());
                Assert.Equal(8, cmd["age"].GetValue<int>());
            };

            DefinitionRegistry.GetInstance().Add(cmdDef);
            string input = "go -f Charlie -l Brown -a 8";
            Command cmd = CommandBuilder.Build(input);
            CommandExecutor exe = DefinitionRegistry.GetInstance().GetCommandExecutor(cmd);
            Action action = () => exe.Execute();

            Assert.Throws<CommandInputException>(action);
        }
        #endregion
    }
}
