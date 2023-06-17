using System;
using System.Collections.Generic;

namespace HatTrick.CommandLine
{
    public class CommandInputException : Exception
    {
        public CommandInputException(params string[] messages) : base(FormatMessages(messages))
        { }

        private static string FormatMessages(string[] messages)
        {
            for (int i = 0; i < messages.Length; i++)
            {
                messages[i] = "- " + messages[i];
            }

            return string.Join(Environment.NewLine, messages);
        }
    }
}
