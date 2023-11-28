using Xunit;

namespace HatTrick.CommandLine.Test
{
    [Collection("Sequential")]
    public class MutuallyExclusiveConstraintTests
    {
        #region constructor
        [Fact]
        public void Constructor_WhenopDefKeysArgument_IsNull_ShouldThrow_ArgumentNullException()
        {
            Action action = () => new MutuallyExclusiveSetConstraint(default);
            Assert.Contains("opDefKeys", Assert.Throws<ArgumentNullException>(action).Message);
        }

        [Fact]
        public void Constructor_WhenopDefKeysArgument_IsPopulated_ShouldGenerate_PipeDelimited_Description()
        {
            var constraint = new MutuallyExclusiveSetConstraint(
                ("key1", "--flag1"),
                ("key2", "--flag2"),
                ("key3", "--flag3"),
                ("key4", "--flag4")
            );

            Assert.Equal("--flag1|--flag2|--flag3|--flag4", constraint.Description);
        }
        #endregion

        #region ensure
        [Fact]
        public void Ensure_WhenAllOptions_AreOfTypes_Default_Or_Empty_ShouldPass()
        {
            Option op1 = new EmptyOption("key1", "-f1");
            Option op2 = new DefaultOption("key2", "-f2");
            op2.SetValue("2");
            Option op3 = new EmptyOption("key3", "-f3");
            Option op4 = new EmptyOption("key4", "-f4");
            Option op5 = new EmptyOption("key5", "-f5");

            Command cmd = new Command("go", op1, op2, op3, op4, op5);

            var constraint = new MutuallyExclusiveSetConstraint(("key2", "-f2"), ("key4", "op4"));

            constraint.Ensure(cmd);
        }

        [Fact]
        public void Ensure_WhenAllOptions_AreOfTypes_Default_Or_Empty_Or_NotTargeted_ShouldPass()
        {
            Option op1 = new EmptyOption("key1", "-f1");
            Option op2 = new DefaultOption("key2", "-f2");
            op2.SetValue("2");
            Option op3 = new EmptyOption("key3", "-f3");
            Option op4 = new EmptyOption("key4", "-f4");
            Option op5 = new EmptyOption("key5", "-f5");
            Option op6 = new Option("key6", "-f6", "6");

            Command cmd = new Command("go", op1, op2, op3, op4, op5, op6);

            var constraint = new MutuallyExclusiveSetConstraint(("key2", "-f2"), ("key4", "op4"));

            constraint.Ensure(cmd);
        }

        [Fact]
        public void Ensure_WhenAllOptions_AreOfTypes_Default_Or_Empty_Except_OneTargeted_ShouldPass()
        {
            Option op1 = new EmptyOption("key1", "-f1");
            Option op2 = new Option("key2", "-f2", "2");
            op2.SetValue("2");
            Option op3 = new EmptyOption("key3", "-f3");
            Option op4 = new EmptyOption("key4", "-f4");
            Option op5 = new EmptyOption("key5", "-f5");

            Command cmd = new Command("go", op1, op2, op3, op4, op5);

            var constraint = new MutuallyExclusiveSetConstraint(("key2", "-f2"), ("key4", "op4"));

            constraint.Ensure(cmd);
        }

        [Fact]
        public void Ensure_When2TargetOptions_AreNot_OfType_Default_Or_Empty_ShouldThrow_CommandInputException()
        {
            Option op1 = new EmptyOption("key1", "-f1");
            Option op2 = new Option("key2", "-f2", "2");
            Option op3 = new EmptyOption("key3", "-f3");
            Option op4 = new Option("key4", "-f4", "4");
            Option op5 = new EmptyOption("key5", "-f5");

            Command cmd = new Command("go", op1, op2, op3, op4, op5);

            var constraint = new MutuallyExclusiveSetConstraint(("key2", "-f2"), ("key4", "op4"));

            Action action = () => constraint.Ensure(cmd);

            Assert.Throws<CommandInputException>(action);
        }
        #endregion

        #region usage
        [Fact]
        public void Usage_WhenNothingAssigned_ShouldPass()
        {
            DefinitionRegistry.Clear();
            var cmdDef = new CommandDefinition("go");
            cmdDef.Handler = (cmd) => { };
            cmdDef.Help = "help";
            cmdDef.AddOption<string>(key: "op1", "a1", help: "help1", ("-1", "--op1"));
            cmdDef.AddOption<string>(key: "op2", "a2", help: "help2", ("-2", "--op2"));
            cmdDef.AddOption<string>(key: "op3", "a3", help: "help3", ("-3", "--op3"));
            cmdDef.AddOption<string>(key: "op4", "a4", help: "help4", ("-4", "--op4"));
            cmdDef.MutuallyExclusiveSet("op3", "op4");
            var reg = DefinitionRegistry.GetInstance();
            reg.Add(cmdDef);

            string input = "go";
            Command cmd = CommandBuilder.Build(input);
            CommandExecutor exe = reg.GetCommandExecutor(cmd);
            exe.Execute();
        }

        [Fact]
        public void Usage_WhenNoTargetAssigned_ShouldPass()
        {
            DefinitionRegistry.Clear();
            var cmdDef = new CommandDefinition("go");
            cmdDef.Handler = (cmd) => { };
            cmdDef.Help = "help";
            cmdDef.AddOption<string>(key: "op1", "a1", help: "help1", ("-1", "--op1"));
            cmdDef.AddOption<string>(key: "op2", "a2", help: "help2", ("-2", "--op2"));
            cmdDef.AddOption<string>(key: "op3", "a3", help: "help3", ("-3", "--op3"));
            cmdDef.AddOption<string>(key: "op4", "a4", help: "help4", ("-4", "--op4"));
            cmdDef.MutuallyExclusiveSet("op3", "op4");
            var reg = DefinitionRegistry.GetInstance();
            reg.Add(cmdDef);

            string input = "go --op1:y --op2:y";
            Command cmd = CommandBuilder.Build(input);
            CommandExecutor exe = reg.GetCommandExecutor(cmd);
            exe.Execute();
        }

        [Fact]
        public void Usage_WhenOneTargetAssigned_ShouldPass()
        {
            DefinitionRegistry.Clear();
            var cmdDef = new CommandDefinition("go");
            cmdDef.Handler = (cmd) => { };
            cmdDef.Help = "help";
            cmdDef.AddOption<string>(key: "op1", "a1", help: "help1", ("-1", "--op1"));
            cmdDef.AddOption<string>(key: "op2", "a2", help: "help2", ("-2", "--op2"));
            cmdDef.AddOption<string>(key: "op3", "a3", help: "help3", ("-3", "--op3"));
            cmdDef.AddOption<string>(key: "op4", "a4", help: "help4", ("-4", "--op4"));
            cmdDef.MutuallyExclusiveSet("op3", "op4");
            var reg = DefinitionRegistry.GetInstance();
            reg.Add(cmdDef);

            string input = "go --op1:y --op2:y --op4:y";
            Command cmd = CommandBuilder.Build(input);
            CommandExecutor exe = reg.GetCommandExecutor(cmd);
            exe.Execute();

        }

        [Fact]
        public void Usage_WhenTwoOrMoreTargetsAssigned_ShouldThrow_CommandInputException()
        {
            DefinitionRegistry.Clear();
            var cmdDef = new CommandDefinition("go");
            cmdDef.Handler = (cmd) => { };
            cmdDef.Help = "help";
            cmdDef.AddOption<string>(key: "op1", "a1", help: "help1", flags: ("-1", "--op1"));
            cmdDef.AddOption<string>(key: "op2", "a2", help: "help2", flags: ("-2", "--op2"));
            cmdDef.AddOption<string>(key: "op3", "a3", help: "help3", flags: ("-3", "--op3"));
            cmdDef.AddOption<string>(key: "op4", "a4", help: "help4", flags: ("-4", "--op4"));
            cmdDef.MutuallyExclusiveSet("op3", "op4");
            var reg = DefinitionRegistry.GetInstance();
            reg.Add(cmdDef);

            string input = "go --op1:y --op2:y --op4:y --op3:y";
            Command cmd = CommandBuilder.Build(input);
            CommandExecutor exe = reg.GetCommandExecutor(cmd);
            Action action = () => exe.Execute();
            Assert.Throws<CommandInputException>(action);
        }
        #endregion
    }
}
