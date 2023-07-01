using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using HatTrick.CommandLine.Namespace;
using HatTrick.Text.Templating;

namespace HatTrick.CommandLine
{
    internal class RenderEngine
    {
        #region internals
        private TemplateAccessor _templateAccessor;
        #endregion

        #region interface
        protected TemplateAccessor TemplateAccessor
        { get => _templateAccessor is null ? _templateAccessor = new TemplateAccessor() : _templateAccessor; }
        #endregion

        #region constructors
        internal RenderEngine()
        {
        }
        #endregion

        #region get executable name
        private string GetExecutableName()
        {
            string path = Environment.ProcessPath;
            return path is null ? "N/A" : Path.GetFileNameWithoutExtension(path);
        }
        #endregion

        #region get child namespaces
        private NamespaceDefinition[] GetChildNamespaces(NamespaceDefinition parent)
        {
            if (parent is null)
                throw new ArgumentNullException(nameof(parent));

            var registry = CommandDefinitionRegistry.GetInstance();

            int atDepth = parent.Depth + 1;
            var children = registry.GetNamespaceDefinitions((ns) => ns.Depth == atDepth && ns.Name.StartsWith(parent.Name));

            return children;
        }
        #endregion

        #region get descendent namespaces
        private NamespaceDefinition[] GetDescendentNamespaces(NamespaceDefinition parent)
        {
            if (parent is null)
                throw new ArgumentNullException(nameof(parent));

            var registry = CommandDefinitionRegistry.GetInstance();

            var descendents = registry.GetNamespaceDefinitions((ns) => ns.Depth > parent.Depth && ns.Name.StartsWith(parent.Name));

            return descendents;
        }
        #endregion

        #region get child command definitions
        private CommandDefinition[] GetChildCommandDefinitions(NamespaceDefinition parent)
        {
            if (parent is null)
                throw new ArgumentNullException(nameof(parent));

            var registry = CommandDefinitionRegistry.GetInstance();

            NamespaceDefinition[] descNamespaces = this.GetDescendentNamespaces(parent);

            var children = registry.GetCommandDefinitions(
                (cmd) => cmd.Depth > parent.Depth 
                      && cmd.Name.StartsWith(parent.Name) 
                      && !Array.Exists(descNamespaces, (ns) => cmd.Name.StartsWith(ns.Name))
            );

            return children;
        }
        #endregion

        #region get descendent command definitions
        private CommandDefinition[] GetDescendentCommandDefinitions(NamespaceDefinition parent)
        {
            if (parent is null)
                throw new ArgumentNullException(nameof(parent));

            var registry = CommandDefinitionRegistry.GetInstance();

            var descendents = registry.GetCommandDefinitions((cmd) => cmd.Depth > parent.Depth && cmd.Name.StartsWith(parent.Name));

            return descendents;
        }
        #endregion

        #region render usage help to
        internal void RenderUsageHelp()
        {
            var cmdDef = CommandDefinitionRegistry.GetInstance().GetDefinition(CommandDefinition.DefaultCommandName);
            string template = TemplateAccessor.GetTemplate("usage-help");

            var ngin = new TemplateEngine(template);
            ngin.TrimWhitespace = true;
            ngin.LambdaRepo.Register(nameof(string.Join), (Func<char, object[], string>)string.Join);
            ngin.LambdaRepo.Register(nameof(this.GetExecutableName), this.GetExecutableName);

            string output = ngin.Merge(cmdDef);

            Console.Write(output);
        }
        #endregion

        #region render root help
        internal void RenderRootHelp()
        {
            string template = TemplateAccessor.GetTemplate("root-help");
            var registry = CommandDefinitionRegistry.GetInstance();

            var bindTo = new Dictionary<string, object>()
            {
                { "Namespaces", registry.GetNamespaceDefinitions((nsd) => !nsd.Hidden && nsd.Depth == 0) },
                { "Commands",   registry.GetCommandDefinitions(  (cmd) => !cmd.Hidden && cmd.Depth == 0) }
            };

            TemplateEngine ngin = new TemplateEngine(template);
            ngin.LambdaRepo.Register(nameof(this.GetExecutableName), this.GetExecutableName);

            string output = ngin.Merge(bindTo);

            Console.Write(output);
        }
        #endregion

        #region render help
        internal void RenderHelp(NamespaceDefinition namespaceDef, bool wildcard)
        {
            string template = TemplateAccessor.GetTemplate("namespace-help");
            var registry = CommandDefinitionRegistry.GetInstance();

            NamespaceDefinition[] namespaces = (wildcard)
                ? new NamespaceDefinition[0]// Array.FindAll(this.GetDescendentNamespaces(namespaceDef), (nsd) => !nsd.Hidden)
                : Array.FindAll(this.GetChildNamespaces(namespaceDef), (nsd) => !nsd.Hidden);

            CommandDefinition[] commands = wildcard
                ? Array.FindAll(this.GetDescendentCommandDefinitions(namespaceDef), (cmd) => !cmd.Hidden)
                : Array.FindAll(this.GetChildCommandDefinitions(namespaceDef), (cmd) => !cmd.Hidden);

            var bindTo = new Dictionary<string, object>()
            {
                { "IsWildcard", wildcard        },
                { "Target",     namespaceDef    },
                { "Namespaces", namespaces      },
                { "Commands",   commands        }
            };

            TemplateEngine ngin = new TemplateEngine(template);
            ngin.TrimWhitespace = true;
            ngin.LambdaRepo.Register(nameof(this.GetExecutableName), this.GetExecutableName);

            string output = ngin.Merge(bindTo);

            Console.Write(output);
        }

        internal void RenderHelp(CommandDefinition commandDef, bool wildcard)
        {
            string template = TemplateAccessor.GetTemplate("command-help");


            var bindTo = new Dictionary<string, object>()
            {
                { "Target",     commandDef },
            };

            TemplateEngine ngin = new TemplateEngine(template);
            ngin.TrimWhitespace = true;
            ngin.LambdaRepo.Register(nameof(this.GetExecutableName), this.GetExecutableName);
            ngin.LambdaRepo.Register(nameof(string.Join), (Func<char, object[], string>)string.Join);

            string output = ngin.Merge(bindTo);

            Console.Write(output);
        }
        #endregion
    }
}
