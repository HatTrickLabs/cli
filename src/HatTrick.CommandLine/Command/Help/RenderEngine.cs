using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using HatTrick.CommandLine.Namespace;
using HatTrick.Text.Templating;
using Microsoft.VisualBasic;

namespace HatTrick.CommandLine
{
    internal class RenderEngine
    {
        #region const
        private const int Indent = 2;
        private const int HelpBlockLeftPad = 5;
        private const int MinHelpBlockStartPosition = 20;
        #endregion

        #region internals
        private TemplateAccessor _templateAccessor;
        private int _bufferLen;
        #endregion

        #region interface
        protected TemplateAccessor TemplateAccessor
        { get => _templateAccessor is null ? _templateAccessor = new TemplateAccessor() : _templateAccessor; }
        #endregion

        #region constructors
        internal RenderEngine()
        {
            _bufferLen = Console.BufferWidth - 1;

        }
        #endregion

        #region get executable name
        private string GetExecutableName()
        {
            string path = Environment.ProcessPath;
            return path is null ? "N/A" : Path.GetFileNameWithoutExtension(path);
        }
        #endregion

        #region resolve help block start position
        private int ResolveHelpBlockStartPosition(INamedDefinition[] definitions)
        {
            if (definitions is null)
                throw new ArgumentNullException(nameof(definitions));

            if (definitions.Length == 0)
                throw new ArgumentException("Argument must contain at least 1 element.", nameof(definitions));

            int maxNameLen = definitions.Max(d => d.Name.Length);

            return this.ResolveHelpBlockStartPosition(maxNameLen);
        }

        private int ResolveHelpBlockStartPosition(int maxLeftContentLength)
        {
            //add the indent and the desired padding
            int blockStart = maxLeftContentLength + RenderEngine.Indent + RenderEngine.HelpBlockLeftPad;

            return Math.Max(blockStart, RenderEngine.MinHelpBlockStartPosition);
        }
        #endregion

        #region get blocked content
        private string GetBlockedContent(string content, int blockAt, int startingAt)
        {
            if (content is null || content == string.Empty)
                return content;

            int maxLen = _bufferLen;
            StringBuilder output = new StringBuilder((int)(content.Length * 1.1));//add 10%

            char[] delims = new char[] { ' ', '\n', '\r', '\t' };

            string[] words = content.Split(delims, StringSplitOptions.RemoveEmptyEntries);

            int linePostion = startingAt;

            string pad = new string('.', (blockAt - startingAt));

            output.Append(pad);
            linePostion += pad.Length;
            string word = words[0];
            output.Append(word);
            linePostion += word.Length;

            pad = new string(' ', blockAt);

            for (int i = 1; i < words.Length; i++)
            {
                word = words[i];
                if ((linePostion + word.Length) > maxLen)
                {
                    output.Append(Environment.NewLine);//roll to next line
                    output.Append(pad);//pad to block start
                    linePostion = blockAt;
                    output.Append(word);
                    linePostion += word.Length;
                }
                else
                {
                    output.Append(' ');//word spacing
                    linePostion += 1;
                    output.Append(word);
                    linePostion += word.Length;
                }
            }
            return output.ToString();
        }
        #endregion

        #region render usage help
        internal void RenderUsageHelp()
        {
            var cmdDef = CommandDefinitionRegistry.GetInstance().GetCommandDefinition(CommandDefinition.DefaultCommandName);
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

        #region render namespace help
        internal void RenderNamespaceHelp(NamespaceDefinition target)
        {
            string template = TemplateAccessor.GetTemplate("namespace-help");
            var registry = CommandDefinitionRegistry.GetInstance();

            NamespaceDefinition[] namespaces = registry.GetChildNamespaceDefinitions(target, false);
            CommandDefinition[] commands = registry.GetChildCommandDefinitions(target, false);

            var defs = new INamedDefinition[namespaces.Length + commands.Length + 1];
            namespaces.CopyTo(defs, 0);
            commands.CopyTo(defs, namespaces.Length);
            defs[^1] = target;

            int blockStart = this.ResolveHelpBlockStartPosition(defs);
            string indent = new string(' ', RenderEngine.Indent);

            var bindTo = new Dictionary<string, object>()
            {
                { "Target",         target      },
                { "Indent",         indent      },
                { "HelpStartPos",   blockStart  },
                { "Namespaces",     namespaces  },
                { "Commands",       commands    }
            };

            TemplateEngine ngin = new TemplateEngine(template);
            ngin.TrimWhitespace = true;
            ngin.LambdaRepo.Register(nameof(this.GetExecutableName), this.GetExecutableName);
            ngin.LambdaRepo.Register(nameof(this.GetBlockedContent), this.GetBlockedContent);
            ngin.LambdaRepo.Register("Add", (int a, int b) => a + b);

            string output = ngin.Merge(bindTo);

            Console.Write(output);
        }
        #endregion

        #region render namespace wildcard help
        internal void RenderNamespaceWildcardHelp(NamespaceDefinition target)
        {
            string template = TemplateAccessor.GetTemplate("namespace-wildcard-help");
            var registry = CommandDefinitionRegistry.GetInstance();

            //EMPTY..on wildcard, only interested in descendent commands
            NamespaceDefinition[] namespaces = Array.Empty<NamespaceDefinition>();
            CommandDefinition[] commands = registry.GetDescendentCommandDefinitions(target, false);

            var defs = new INamedDefinition[commands.Length + 1];
            commands.CopyTo(defs, 0);
            defs[^1] = target;

            int blockStart = this.ResolveHelpBlockStartPosition(defs);
            string indent = new string(' ', RenderEngine.Indent);

            var bindTo = new Dictionary<string, object>()
            {
                { "Target",         target      },
                { "Indent",         indent      },
                { "HelpStartPos",   blockStart  },
                { "Namespaces",     namespaces  },
                { "Commands",       commands    }
            };

            TemplateEngine ngin = new TemplateEngine(template);
            ngin.TrimWhitespace = true;
            ngin.LambdaRepo.Register(nameof(this.GetExecutableName), this.GetExecutableName);
            ngin.LambdaRepo.Register(nameof(this.GetBlockedContent), this.GetBlockedContent);
            ngin.LambdaRepo.Register("Add", (int a, int b) => a + b);

            string output = ngin.Merge(bindTo);

            Console.Write(output);
        }
        #endregion

        #region render command help
        internal void RenderCommandHelp(CommandDefinition target)
        {
            string template = TemplateAccessor.GetTemplate("command-help");

            //TODO: refactor this... doing this string join here AND within the template...
            int blockStart = this.ResolveHelpBlockStartPosition(target.Options.Max(o => string.Join('|', o.Flags).Length));
            string indent = new string(' ', RenderEngine.Indent);

            var bindTo = new Dictionary<string, object>()
            {
                { "Target",         target      },
                { "Indent",         indent      },
                { "HelpStartPos",   blockStart  },
            };

            TemplateEngine ngin = new TemplateEngine(template);
            ngin.TrimWhitespace = true;
            ngin.LambdaRepo.Register(nameof(this.GetExecutableName), this.GetExecutableName);
            ngin.LambdaRepo.Register(nameof(this.GetBlockedContent), this.GetBlockedContent);
            ngin.LambdaRepo.Register("Add", (int a, int b) => a + b);
            ngin.LambdaRepo.Register(nameof(string.Join), (Func<char, object[], string>)string.Join);

            string output = ngin.Merge(bindTo);

            Console.Write(output);
        }
        #endregion
    }
}
