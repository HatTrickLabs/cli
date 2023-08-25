using System;
using System.Collections.Generic;

namespace HatTrick.CommandLine.Parsing
{
    public static class Parser
    {
        #region parse
        public static Command Parse(string[] tokens)
        {
            if (tokens is null)
                throw new ArgumentNullException(nameof(tokens));

            if (tokens.Length == 0)
                return new Command(CommandDefinition.DefaultCommandName);//optionless default command...

            bool isDefault = tokens[0][0] == '-';//no command, just jumps right into option flags

            int startAt = isDefault ? 0 : 1;
            var opTokens = new ReadOnlySpan<string>(tokens, startAt, tokens.Length - startAt);

            SetOf<CommandOption> options = ParseCommandOptions(opTokens);

            string name = isDefault ? CommandDefinition.DefaultCommandName : tokens[0];
            return new Command(name, options);
        }
        #endregion

        #region parse command options
        public static SetOf<CommandOption> ParseCommandOptions(ReadOnlySpan<string> tokens)
        {
            var ops = new SetOf<CommandOption>();

            Func<string, bool> isExplicitAssign = (a) => a == "=" || a == ":";

            string prev = null;
            for (int i = 0; i < tokens.Length; i++)
            {
                string token = tokens[i];

                if (isExplicitAssign(token))
                {
                    //TODO: expand on this...it could be a syntax error...ie: copy :--from c:\test -to c:\test2
                    //maybe check i%2 == 1
                    prev = token;
                    continue;
                }

                //starts with '-' then must be option flag unless prev arg is an explicit assignment char
                if (token[0] == '-' && !isExplicitAssign(prev))
                {
                    if (token.Length == 1)
                        throw new CommandInputException($"Invalid command line argument provided: '{token}' at position: {i + 1}");

                    if (token[1] == '-') //verbose option flag
                        ops.Add(new CommandOption(token));

                    else if (token.Length == 2) //single terse option flag
                        ops.Add(new CommandOption(token));

                    else //must be a compound terse option flag, unroll into individual flags
                        UnrollCompoundFlag(token, (f) => ops.Add(f));

                }
                else //no '-' and prev not an explicit assign, must be an option argument
                {
                    ops[^1].ApplyArgument(token);//apply current arg to the last op in the set
                }

                prev = token;
            }

            return ops;
        }
        #endregion

        #region unroll compound flag
        private static void UnrollCompoundFlag(string flag, Action<CommandOption> onFlagUnrolled)
        {
            //start at idx 1 to skip the '-'
            for (int i = 1; i < flag.Length; i++)
            {
                var op = new CommandOption(flag: "-" + flag[i]);
                onFlagUnrolled(op);
            }
        }
        #endregion
    }
}
