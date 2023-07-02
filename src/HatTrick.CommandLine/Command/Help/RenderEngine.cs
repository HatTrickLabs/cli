using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
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
        private int _bufferWidth;
        #endregion

        #region interface
        protected TemplateAccessor TemplateAccessor
        { get => _templateAccessor is null ? _templateAccessor = new TemplateAccessor() : _templateAccessor; }
        #endregion

        #region constructors
        internal RenderEngine()
        {
            _bufferWidth = Console.BufferWidth;
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

        #region render usage help
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

            var namespaces = registry.GetNamespaceDefinitions((nsd) => !nsd.Hidden && nsd.Depth == 0);
            var commands = registry.GetCommandDefinitions((cmd) => !cmd.Hidden && cmd.Depth == 0);

            var bindTo = new Dictionary<string, object>()
            {
                { "Namespaces", namespaces  },
                { "Commands",   commands    }
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

            int maxNs = namespaces.Any() ? namespaces.Max(ns => ns.Name.Length) : 0;
            int maxCmd = commands.Any() ? commands.Max(cmd => cmd.Name.Length) : 0;

            int helpStart = Math.Max(maxNs, maxCmd);

            helpStart += 2; //add the 2 char indent

            helpStart += 5;//add the buffer we want between the end of the ns/cmd and the help (at least 5 chars width)

            if (helpStart < 20)
                helpStart = 20;

            //min start for help should be 20
            //max start for help should be ~40

            Func<int, int, int> Add = (int a, int b) => a + b;

            Func<int, string> Pad = (int postion) => new string(' ', (helpStart - postion));

            Func<string, string> RollAndIndent = (string help) =>
            {
                int max = Console.BufferWidth - 1;
                StringBuilder output = new StringBuilder((int)(help.Length * 1.1));//add 10%
                string[] words = help.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                int linePostion = helpStart;

                output.Append(words[0]);
                linePostion += words[0].Length;
                for (int i = 1; i < words.Length; i++)
                {
                    string word = words[i];
                    int wordLen = word.Length;
                    if ((linePostion + wordLen) > max)
                    {
                        output.Append(Environment.NewLine);
                        output.Append(new string(' ', helpStart));
                        linePostion = helpStart;
                        output.Append(word);
                        linePostion += word.Length;
                    }
                    else
                    {
                        output.Append(' ').Append(word);
                        linePostion += (word.Length + 1);
                    }
                }
                return output.ToString();
            };

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
            ngin.LambdaRepo.Register(nameof(Add), Add);
            ngin.LambdaRepo.Register(nameof(Pad), Pad);
            ngin.LambdaRepo.Register(nameof(RollAndIndent), RollAndIndent);

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
