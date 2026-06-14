// SPDX-License-Identifier: Apache-2.0
// Copyright (c) HatTrick Labs, LLC

using System.Threading.Tasks;
using Xunit;

namespace HatTrick.CommandLine.Test
{
    [Collection("Sequential")]
    public class CommandExecutorTests
    {
        #region on pre ensure
        [Fact]
        public async Task ExecuteAsync_ShouldInvoke_OnPreEnsure_BeforeHandler()
        {
            DefinitionRegistry.Clear();
            bool hookFired = false;

            var cmdDef = new CommandDefinition("go") { Help = "help!" };
            cmdDef.AddOption<string>(key: "val", help: "v help", ("-v", "--val"));
            cmdDef.OnPreEnsure = (cmd) =>
            {
                hookFired = true;
                cmd.GetOption(o => o.Flag == "--val").OverrideArgument("rewritten");
            };
            cmdDef.AsyncHandler = async (cmd) =>
            {
                //the hook must have rewritten the raw argument before constraint/hydration
                Assert.Equal("rewritten", cmd["val"].GetValue<string>());
                await Task.CompletedTask;
            };
            DefinitionRegistry.GetInstance().Add(cmdDef);

            Command cmd = CommandBuilder.Build("go --val original");
            CommandExecutor exe = DefinitionRegistry.GetInstance().GetCommandExecutor(cmd);
            await exe.ExecuteAsync();

            Assert.True(hookFired);
        }

        [Fact]
        public void Execute_ShouldInvoke_OnPreEnsure_BeforeHandler()
        {
            DefinitionRegistry.Clear();
            bool hookFired = false;

            var cmdDef = new CommandDefinition("go") { Help = "help!" };
            cmdDef.AddOption<string>(key: "val", help: "v help", ("-v", "--val"));
            cmdDef.OnPreEnsure = (cmd) =>
            {
                hookFired = true;
                cmd.GetOption(o => o.Flag == "--val").OverrideArgument("rewritten");
            };
            cmdDef.Handler = (cmd) =>
            {
                Assert.Equal("rewritten", cmd["val"].GetValue<string>());
            };
            DefinitionRegistry.GetInstance().Add(cmdDef);

            Command cmd = CommandBuilder.Build("go --val original");
            CommandExecutor exe = DefinitionRegistry.GetInstance().GetCommandExecutor(cmd);
            exe.Execute();

            Assert.True(hookFired);
        }
        #endregion
    }
}
