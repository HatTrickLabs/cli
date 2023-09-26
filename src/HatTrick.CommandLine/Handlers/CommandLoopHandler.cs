using System;
using System.IO;

namespace HatTrick.CommandLine
{
    internal class CommandLoopHandler
    {
        #region internals
        private static CommandLoopHandler _instance;
        private static object _lock = new();
        private static bool _isRunning;
        #endregion

        #region constructors
        private CommandLoopHandler()
        { 
            //this must be a singleton without any substantial internal state...MUST avoid init of multiple instances.
            //clean up any state on ReleaseInstanceLock
        }
        #endregion

        #region get instance
        internal static CommandLoopHandler GetInstance()
        {
            lock (_lock)
            {
                if (_instance is null)
                    _instance = new CommandLoopHandler();
            }
            return _instance;
        }
        #endregion

        #region go
        internal void Go(Command cmd)
        {
            if (!this.TryAquireInstanceLock())
                return;

            try
            {
                this.RunCommandLoop();
            }
            finally
            {
                this.ReleaseInstanceLock();
            }
        }
        #endregion

        #region run command loop
        private void RunCommandLoop()
        {
            var registry = DefinitionRegistry.GetInstance();
            string prompt = this.ResolvePrompt();

            while (true)
            {
                try
                {
                    this.EnsureCarriageReturn();

                    Console.Write(prompt);
                    string line = Console.ReadLine();

                    if (IsClearScreenCommand(line))
                    {
                        Console.Clear();
                        continue;
                    }

                    //TODO: imple local var declaration command
                    

                    if (this.IsExitCommand(line))
                        break;

                    registry.ExecuteCommand(Parser.Parse(Tokenizer.Tokenize(line)));
                    Console.WriteLine(string.Empty);
                }
                catch (Exception ex)
                {
                    string name = ex.GetType().Name;
                    string nl = Environment.NewLine;
                    string msg = $"{name}:{nl}{ex.Message}{nl}";
#if DEBUG
                    msg += $"{nl}Stack Trace:{nl}{ex.StackTrace}{nl}";
#endif
                    Console.Error.WriteLine(msg);
                }
            }

        }
        #endregion

        #region aquire instance lock
        private bool TryAquireInstanceLock()
        {
            lock (_lock)
            {
                if (_isRunning)
                    return false;
                else
                    return (_isRunning = true);
            }
        }
        #endregion

        #region release instance lock
        private void ReleaseInstanceLock()
        {
            lock (_lock)
            {
                _isRunning = false;
            }
        }
        #endregion

        #region resolve prompt
        private string ResolvePrompt()
        {
            string path = Environment.ProcessPath;
            string name = Path.GetFileNameWithoutExtension(path);
            string prompt = name + ">";
            return prompt;
        }
        #endregion

        #region ensure carriage return
        private void EnsureCarriageReturn()
        {
            if (Console.CursorLeft != 0)
                Console.WriteLine(string.Empty);
        }
        #endregion

        #region is exit command
        private bool IsExitCommand(string command)
        {
            if (command == null)
                return false;

            string cmd = command;
            Func<string, string, bool, int> comp = string.Compare;
            return comp(cmd, "exit", true) == 0 || comp(cmd, "bye", true) == 0;
        }
        #endregion

        #region is clear screen command
        private bool IsClearScreenCommand(string command)
        {
            if (command == null)
                return false;

            return string.Compare(command, "cls", true) == 0;
        }
        #endregion
    }
}
