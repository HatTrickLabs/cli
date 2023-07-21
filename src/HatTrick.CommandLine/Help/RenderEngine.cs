using System;
using System.Linq;
using System.Collections.Generic;
using HatTrick.Text.Templating;
using System.Text;
using System.IO;

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

        #region resolve block start position
        private int ResolveBlockStartPosition(int maxLeftContentLength)
        {
            //add the indent and the desired padding
            int blockStart = RenderEngine.Indent + maxLeftContentLength + RenderEngine.HelpBlockLeftPad;

            return Math.Max(blockStart, RenderEngine.MinHelpBlockStartPosition);
        }
        #endregion

        #region get blocked content
        private string GetBlockedContent(string content, int blockAt, int startingAt, char padChar = '.')
        {
            if (content is null || content == string.Empty)
                return content;

            int maxLen = _bufferLen;
            StringBuilder output = new StringBuilder((int)(content.Length * 1.1));//add 10%

            char[] delims = new char[] { ' ', '\n', '\r', '\t' };

            string[] words = content.Split(delims, StringSplitOptions.RemoveEmptyEntries);

            int linePostion = startingAt;

            string pad = new string(padChar, (blockAt - startingAt));

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
            var target = Registry.GetInstance().GetCommandDefinition(CommandDefinition.DefaultCommandName);
            string template = TemplateAccessor.GetTemplate("usage-help");

            int blockStart = this.ResolveBlockStartPosition(target.Options.Max(o => string.Join('|', o.Flags).Length));
            string indent = new string(' ', RenderEngine.Indent);

            Func<CommandOptionDefinition, string> GetOpDefHelp = (op) =>
            {
                string flags = string.Join('|', op.Flags);
                int startAt = indent.Length + flags.Length;
                return $"{indent}{flags}{this.GetBlockedContent(op.Help, blockStart, startAt)}";
            };

            var ngin = new TemplateEngine(template);
            ngin.TrimWhitespace = true;
            ngin.LambdaRepo.Register(nameof(this.GetExecutableName), this.GetExecutableName);
            ngin.LambdaRepo.Register(nameof(GetOpDefHelp), GetOpDefHelp);

            string output = ngin.Merge(target);

            Console.Write(output);
        }
        #endregion

        #region render root help
        internal void RenderRootHelp()
        {
            string template = TemplateAccessor.GetTemplate("root-help");
            var registry = Registry.GetInstance();

            var namespaces = registry.GetNamespaceDefinitions((nsd) => !nsd.Hidden && nsd.Depth == 0);
            var commands = registry.GetCommandDefinitions((cmd) => !cmd.Hidden && cmd.Depth == 0);

            int maxNSLen = namespaces.Length > 0 ? namespaces.Max(ns => ns.Name.Length) : 0;
            int maxCmdLen = commands.Length > 0 ? commands.Max(cmd => cmd.Name.Length) : 0;
            int maxLen = Math.Max(maxNSLen, maxCmdLen);

            int blockStart = this.ResolveBlockStartPosition(maxLen);
            string indent = new string(' ', RenderEngine.Indent);

            var bindTo = new Dictionary<string, object>()
            {
                { "Namespaces", namespaces  },
                { "Commands",   commands    }
            };

            Func<NamespaceDefinition, string> GetNamespaceHelp = (ns) =>
            {
                int startAt = indent.Length + ns.Name.Length;
                return $"{indent}{ns.Name}{this.GetBlockedContent(ns.Help, blockStart, startAt)}";
            };

            Func<CommandDefinition, string> GetCommandHelp = (cmd) =>
            {
                int startAt = indent.Length + cmd.Name.Length;
                return $"{indent}{cmd.Name}{this.GetBlockedContent(cmd.Help, blockStart, startAt)}";
            };

            TemplateEngine ngin = new TemplateEngine(template);
            ngin.LambdaRepo.Register(nameof(this.GetExecutableName), this.GetExecutableName);
            ngin.LambdaRepo.Register(nameof(GetNamespaceHelp), GetNamespaceHelp);
            ngin.LambdaRepo.Register(nameof(GetCommandHelp), GetCommandHelp);

            string output = ngin.Merge(bindTo);

            Console.Write(output);
        }
        #endregion

        #region render namespace help
        internal void RenderNamespaceHelp(NamespaceDefinition target)
        {
            string template = TemplateAccessor.GetTemplate("namespace-help");
            var registry = Registry.GetInstance();

            NamespaceDefinition[] namespaces = registry.GetChildNamespaceDefinitions(target, false);
            CommandDefinition[] commands = registry.GetChildCommandDefinitions(target, false);

            int maxNSLen = namespaces.Length > 0 ? namespaces.Max(ns => ns.Name.Length) : 0;
            int maxCmdLen = commands.Length > 0 ? commands.Max(cmd => cmd.Name.Length) : 0;
            int maxDefLen = Math.Max(maxNSLen, maxCmdLen);
            int maxLen = Math.Max(target.Name.Length, maxDefLen);

            int blockStart = this.ResolveBlockStartPosition(maxLen);
            string indent = new string(' ', RenderEngine.Indent);

            var bindTo = new Dictionary<string, object>()
            {
                { "Target",         target      },
                { "Indent",         indent      },
                { "BlockStart",     blockStart  },
                { "Namespaces",     namespaces  },
                { "Commands",       commands    }
            };

            Func<NamespaceDefinition, string> GetNamespaceHelp = (ns) =>
            {
                int startAt = indent.Length + ns.Name.Length;
                return $"{indent}{ns.Name}{this.GetBlockedContent(ns.Help, blockStart, startAt)}";
            };

            Func<CommandDefinition, string> GetCommandHelp = (cmd) =>
            {
                int startAt = indent.Length + cmd.Name.Length;
                return $"{indent}{cmd.Name}{this.GetBlockedContent(cmd.Help, blockStart, startAt)}";
            };

            TemplateEngine ngin = new TemplateEngine(template);
            ngin.TrimWhitespace = true;
            ngin.LambdaRepo.Register(nameof(this.GetExecutableName), this.GetExecutableName);
            ngin.LambdaRepo.Register(nameof(this.GetBlockedContent), this.GetBlockedContent);
            ngin.LambdaRepo.Register(nameof(GetNamespaceHelp), GetNamespaceHelp);
            ngin.LambdaRepo.Register(nameof(GetCommandHelp), GetCommandHelp);

            string output = ngin.Merge(bindTo);

            Console.Write(output);
        }
        #endregion

        #region render namespace wildcard help
        internal void RenderNamespaceWildcardHelp(NamespaceDefinition target)
        {
            string template = TemplateAccessor.GetTemplate("namespace-wildcard-help");
            var registry = Registry.GetInstance();

            //namespaces EMPTY on wildcard, only interested in descendent commands
            NamespaceDefinition[] namespaces = Array.Empty<NamespaceDefinition>();//0 length...
            CommandDefinition[] commands = registry.GetDescendentCommandDefinitions(target, false);

            int maxCmdLen = commands.Length > 0 ? commands.Max(cmd => cmd.Name.Length) : 0;
            int maxLen = Math.Max(target.Name.Length, maxCmdLen);

            int blockStart = this.ResolveBlockStartPosition(maxLen);
            string indent = new string(' ', RenderEngine.Indent);

            var bindTo = new Dictionary<string, object>()
            {
                { "Target",         target      },
                { "BlockStart",     blockStart  },
                { "Namespaces",     namespaces  },
                { "Commands",       commands    }
            };

            Func<CommandDefinition, string> GetCommandHelp = (cmd) =>
            {
                int startAt = indent.Length + cmd.Name.Length;
                return $"{indent}{cmd.Name}{this.GetBlockedContent(cmd.Help, blockStart, startAt)}";
            };

            TemplateEngine ngin = new TemplateEngine(template);
            ngin.TrimWhitespace = true;
            ngin.LambdaRepo.Register(nameof(this.GetExecutableName), this.GetExecutableName);
            ngin.LambdaRepo.Register(nameof(this.GetBlockedContent), this.GetBlockedContent);
            
            ngin.LambdaRepo.Register(nameof(GetCommandHelp), GetCommandHelp);

            string output = ngin.Merge(bindTo);

            Console.Write(output);
        }
        #endregion

        #region render command help
        internal void RenderCommandHelp(CommandDefinition target)
        {
            string template = TemplateAccessor.GetTemplate("command-help");

            int opBlockStart = this.ResolveBlockStartPosition(target.Options.Max(o => string.Join('|', o.Flags).Length));
            int cmdConstBlockStart = this.ResolveBlockStartPosition(target.HasConstraints ? target.Constraints.Max(c => c.Name.Length) : 0);
            string indent = new string(' ', RenderEngine.Indent);

            Func<Type, string> GetFriendlyTypeName = (t) =>
            {
                return TypeMap.GetAliasOrName(t);
            };

            Func<CommandOptionDefinition, string> GetOpDefHelp = (op) =>
            {
                string flags = string.Join('|', op.Flags);
                int startAt = indent.Length + flags.Length;
                return $"{indent}{flags}{this.GetBlockedContent(op.Help, opBlockStart, startAt)}";
            };

            Func<ArgumentConstraint, string> GetOpDefArgConstraintHelp = (opConst) =>
            {
                int blockAt = opBlockStart;
                int startAt = 0;
                string content = $"{opConst.Name}:  {opConst.Description}";
                return this.GetBlockedContent(content, blockAt, startAt, ' ');
            };

            Func<CommandConstraint, string> GetCommandConstraintHelp = (c) =>
            {
                int startAt = indent.Length + c.Name.Length;
                return $"{indent}{c.Name}{this.GetBlockedContent(c.Description, cmdConstBlockStart, startAt, ' ')}";
            };

            TemplateEngine ngin = new TemplateEngine(template);
            ngin.TrimWhitespace = true;
            ngin.LambdaRepo.Register(nameof(this.GetExecutableName), this.GetExecutableName);
            ngin.LambdaRepo.Register(nameof(GetFriendlyTypeName), GetFriendlyTypeName);
            ngin.LambdaRepo.Register(nameof(GetOpDefHelp), GetOpDefHelp);
            ngin.LambdaRepo.Register(nameof(GetOpDefArgConstraintHelp), GetOpDefArgConstraintHelp);
            ngin.LambdaRepo.Register(nameof(GetCommandConstraintHelp), GetCommandConstraintHelp);
            string output = ngin.Merge(target);

            Console.Write(output);
        }
        #endregion
    }
}
