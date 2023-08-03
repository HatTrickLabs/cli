using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HatTrick.CommandLine
{
    public sealed class CommandOptionSet
    {
        #region internals
        private CommandOption[] _ops;
        private int _cnt;
        #endregion

        #region interface
        public int Count => _cnt;

        public CommandOption this[int i]
        {
            get => _ops[i];
            set => _ops[i] = value;
        }
        #endregion

        #region constructors
        public CommandOptionSet()
        { }
        #endregion

        #region add
        public void Add(CommandOption option)
        {
            if (_ops is null)
            {
                _ops = new CommandOption[1];
            }
            else
            {
                var newOps = new CommandOption[_cnt + 1];
                Array.Copy(_ops, newOps, _cnt);
                _ops = newOps;
            }

            _ops[_cnt++] = option;
        }
        #endregion

        #region exists
        public bool Exists(Predicate<CommandOption> where)
        {
            if (_ops is null)
                return false;

            return Array.Exists(_ops, where);
        }
        #endregion

        #region find index
        public int FindIndex(Predicate<CommandOption> where)
        {
            if (_ops is null)
                return -1;

            return Array.FindIndex(_ops, where);
        }
        #endregion

        #region find
        public CommandOption Find(Predicate<CommandOption> where)
        {
            if (_ops is null)
                return default;

            return Array.Find(_ops, where);
        }
        #endregion

        #region find all
        public CommandOption[] FindAll(Predicate<CommandOption> where)
        {
            if (_ops is null)
                return new CommandOption[0];

            return Array.FindAll(_ops, where);
        }
        #endregion
    }
}
