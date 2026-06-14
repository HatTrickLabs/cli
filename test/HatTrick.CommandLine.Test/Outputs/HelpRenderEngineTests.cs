// SPDX-License-Identifier: Apache-2.0
// Copyright (c) HatTrick Labs, LLC

using System;
using System.IO;
using Xunit;

namespace HatTrick.CommandLine.Test
{
    [Collection("Sequential")]
    public class HelpRenderEngineTests
    {
        #region synthetic namespace rendering
        [Fact]
        public void RenderRootHelp_WhenSyntheticRootNamespacePresent_ShouldRender_WithoutThrowing()
        {
            DefinitionRegistry.Clear();
            var registry = DefinitionRegistry.GetInstance();
            registry.Add(new NamespaceDefinition("a.b", "real help")); //"a" is auto-created as a synthetic root

            Assert.True(registry.TryGetNamespaceDefinition("a", out NamespaceDefinition synthetic));
            Assert.True(synthetic.Synthetic);
            Assert.Null(synthetic.Help);

            string output = CaptureConsole(() => new HelpRenderEngine().RenderRootHelp());

            //the synthetic root renders (as a bare name, no description) and nothing throws
            Assert.Contains("a", output);
        }

        [Fact]
        public void RenderNamespaceHelp_WhenTargetIsSynthetic_ShouldRender_WithoutThrowing()
        {
            DefinitionRegistry.Clear();
            var registry = DefinitionRegistry.GetInstance();
            registry.Add(new NamespaceDefinition("a.b", "real help")); //"a" synthetic
            registry.Add(new CommandDefinition("a.connect") { Handler = (cmd) => { }, Help = "Connect." });

            Assert.True(registry.TryGetNamespaceDefinition("a", out NamespaceDefinition synthetic));
            Assert.True(synthetic.Synthetic);

            //target itself has null help — must not NRE; children must still render
            string output = CaptureConsole(() => new HelpRenderEngine().RenderNamespaceHelp(synthetic));

            Assert.Contains("a.b", output);       //child namespace listed
            Assert.Contains("a.connect", output); //child command listed
        }
        #endregion

        #region helpers
        private static string CaptureConsole(Action action)
        {
            TextWriter original = Console.Out;
            using var sw = new StringWriter();
            Console.SetOut(sw);
            try { action(); }
            finally { Console.SetOut(original); }
            return sw.ToString();
        }
        #endregion
    }
}
