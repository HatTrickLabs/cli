// SPDX-License-Identifier: Apache-2.0
// Copyright (c) HatTrick Labs, LLC

using Xunit;

namespace HatTrick.CommandLine.Test
{
    [Collection("Sequential")]
    public class MustAssignConstraintTests
    {
        #region ensure
        [Fact]
        public void Ensure_WhenConstraintFails_ShouldThrow_OptionArgumentException()
        {
            var constraint = new MustAssignConstraint<decimal>(("-f","--flag"));
            Option option = new EmptyOption("key", "-f");
            Action action = () => constraint.Ensure(ref option);
            Assert.Contains($"option has a '{MustAssignConstraint<decimal>.ConstraintName}' constraint.",
                Assert.Throws<OptionArgumentException>(action).Message
            );
        }

        [Fact]
        public void Ensure_WhenConstraintPasses_ShouldPass()
        {
            var constraint = new MustAssignConstraint<decimal>(("-f", "--flag"));
            Option option = new Option("key", "-f", "8.0");
            option.SetValue(decimal.Parse(option.Argument));
            constraint.Ensure(ref option);
        }
        #endregion

        #region usage
        [Fact]
        public void Usage_WhenConstraintFails_ShouldThrow_OptionArgumentException()
        {
            DefinitionRegistry.Clear();
            var cmdDef = new CommandDefinition("copy");
            cmdDef.Help = "Help!";
            //the following WILL have a must assign constraint applied internall because no default arg is provided here.
            cmdDef.AddOption<string>(key: "path", "Path of file or directory to copy.", ("-p", "--path"));
            cmdDef.Handler += (cmd) => { };
            DefinitionRegistry.GetInstance().Add(cmdDef);

            string input = "copy";
            Command cmd = CommandBuilder.Build(input);
            CommandExecutor exe = DefinitionRegistry.GetInstance().GetCommandExecutor(cmd);
            Action action = () => exe.Execute();
            Assert.Contains($"option has a '{MustAssignConstraint<string>.ConstraintName}' constraint.",
                Assert.Throws<OptionArgumentException>(action).Message
            );
        }

        [Fact]
        public void Usage_WhenConstraintPasses_ShouldPass()
        {
            DefinitionRegistry.Clear();
            var cmdDef = new CommandDefinition("copy");
            cmdDef.Help = "Help!";
            //the following WILL have a must assign constraint applied internall because no default arg is provided here.
            cmdDef.AddOption<string>(key: "path", "Path of file or directory to copy.", ("-p", "--path"));
            cmdDef.Handler += (cmd) => { };
            DefinitionRegistry.GetInstance().Add(cmdDef);

            string input = "copy -p \"c:/tmp_files\"";
            Command cmd = CommandBuilder.Build(input);
            CommandExecutor exe = DefinitionRegistry.GetInstance().GetCommandExecutor(cmd);
            exe.Execute();
        }
        #endregion
    }
}
