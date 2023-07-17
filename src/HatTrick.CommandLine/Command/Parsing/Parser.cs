using System;
using System.Collections.Generic;

namespace HatTrick.CommandLine
{
    public static class Parser
    {
        #region parse
        public static Command Parse(string[] args)
        {
            if (args is null)
                throw new ArgumentNullException(nameof(args));

            if (args.Length == 0)
                return new Command(CommandDefinition.DefaultCommandName);//optionless default command...

            bool isDefault = args[0][0] == '-';//no command, just jumps right into option flags

            int startAt = isDefault ? 0 : 1;
            var argSet = new ReadOnlySpan<string>(args, startAt, (args.Length - startAt));

            List<CommandOption> options = Parser.ParseCommandOptions(argSet);

            return new Command(isDefault ? CommandDefinition.DefaultCommandName : args[0], options);
        }
        #endregion

        #region parse command options
        public static List<CommandOption> ParseCommandOptions(ReadOnlySpan<string> arguments)
        {
            var ops = new List<CommandOption>();

            Func<string, bool> isExplicitAssign = (a) => a == "=" || a == ":";

            string prev = null;
            for (int i = 0; i < arguments.Length; i++)
            {
                string arg = arguments[i];

                if (isExplicitAssign(arg))
                {
                    //TODO: expand on this...it could be a syntax error...ie: copy :--from c:\test -to c:\test2
                    //maybe check i%2 == 1
                    prev = arg;
                    continue;
                }

                //starts with '-' then must be option flag unless prev arg is an explicit assignment char
                if (arg[0] == '-' && !isExplicitAssign(prev))
                {
                    if (arg.Length == 1)
                        throw new CommandInputException($"Invalid command line argument provided: '{arg}' at position: {i + 1}");

                    if (arg[1] == '-') //verbose option flag
                        ops.Add(new CommandOption(arg));

                    else if (arg.Length == 2) //single terse option flag
                        ops.Add(new CommandOption(arg));

                    else //must be a compound terse option flag, unroll into individual flags
                        Parser.UnrollCompoundFlag(arg, (f) => ops.Add(f));

                }
                else //no '-' and prev not an explicit assign, must be an option argument
                {
                    ops[^1].SetArgument(arg);//set the last option in the list with the current arg
                }

                prev = arg;
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
