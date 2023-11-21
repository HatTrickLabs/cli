using System;
using System.Linq;
using System.Text;

namespace HatTrick.CommandLine
{
    public static class CommandBuilder
    {
        #region build command
        public static Command Build(string input)
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));

            string[] args = Scanner.Scan(input);
            Token[] tokens = Tokenizer.Tokenize(args);
            Command cmd = Parser.Parse(tokens);
            return cmd;
        }

        public static Command Build(string[] args)
        {
            //WHY???
            //"abc""xyz""aaa" should tokenize as [ "abc", "xyz", "aaa" ]
            //main() will recieve from CLR [ "abc\"xyz\"aaa" ]
            //and that sucks, CLR allows doubling double quotes to act as escape
            //also want to eloquently handle explicity assign chars = and :
            //abc: 123
            //abc :123
            //abc : 123
            //abc:123 
            //all 4 of the above should tokenize equally into [ flag, expl_assign, arg ]
            if (args is null)
                throw new ArgumentNullException(nameof(args));

            if (args.Length == 0)
                return CommandBuilder.Build(string.Empty);

            //length of each arg + spaces + possible quotes around each arg
            int maxCapacity = args.Sum(a => a.Length) + args.Length + (args.Length * 2);
            var sb = new StringBuilder(maxCapacity);

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];

                if (i > 0)
                    sb.Append(' ');

                if (arg.Contains(' '))
                    sb.Append("\"").Append(arg).Append("\"");
                else
                    sb.Append(arg);
            }

            string input = sb.ToString();
            return CommandBuilder.Build(input);
        }
        #endregion
    }
}
