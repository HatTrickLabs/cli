// SPDX-License-Identifier: Apache-2.0
// Copyright (c) HatTrick Labs, LLC

using System;

namespace HatTrick.CommandLine
{
    public class MaskedInputLineReader
    {
        #region constants
        private const char MaskChar = '*';
        #endregion

        #region internals
        private int _maxLength;

        private char[] _buffer;

        private int _position;
        private int _length;

        private bool _insertMode;
        private bool _isMasked;
        #endregion

        #region constructors
        public MaskedInputLineReader()
        {
        }
        #endregion

        #region initialize
        private void Initialize()
        {
            //validate console state and reset per-read state so the reader can be reused across calls.

            if (Console.GetCursorPosition().Left > 0)
                throw new InvalidOperationException("Masked line input reader must start at console left position of 0.");

            _maxLength = Console.BufferWidth - 1; //CANNOT get to actual buffer width without exception...
            _buffer = new char[_maxLength];
            _position = 0;
            _length = 0;
            _insertMode = false;
            _isMasked = true;
        }
        #endregion

        #region read masked input
        public string ReadMaskedInput()
        {
            this.Initialize();

            ConsoleKeyInfo input;

            while ((input = Console.ReadKey(true)).Key != ConsoleKey.Enter)
            {
                this.HandleKey(input);

                if (input.Key == ConsoleKey.Escape)
                    break;
            }

            if (input.Key == ConsoleKey.Escape)
                return null;

            if (!_isMasked)
                this.ReMaskOutput();

            string output = new string(_buffer, 0, _length);
            Console.WriteLine(string.Empty);
            return output;
        }
        #endregion

        #region handle key
        private void HandleKey(ConsoleKeyInfo keyInfo)
        {
            switch (keyInfo.Key)
            {
                case ConsoleKey.Backspace:
                    this.HandleBackspaceKey();
                    break;
                case ConsoleKey.Escape:
                    this.HandleEscapeKey();
                    break;
                case ConsoleKey.Spacebar:
                    this.HandleOutputKey(keyInfo);
                    break;
                case ConsoleKey.End:
                    this.HandleEndKey();
                    break;
                case ConsoleKey.Home:
                    this.HandleHomeKey();
                    break;
                case ConsoleKey.LeftArrow:
                    this.handleLeftArrowKey();
                    break;
                case ConsoleKey.UpArrow:
                    this.HandleUpArrowKey();
                    break;
                case ConsoleKey.RightArrow:
                    this.HandleRightArrowKey();
                    break;
                case ConsoleKey.DownArrow:
                    this.HandleDownArrowKey();
                    break;
                case ConsoleKey.Insert:
                    this.HandleInsertKey();
                    break;
                case ConsoleKey.Delete:
                    this.HandleDeleteKey();
                    break;
                case ConsoleKey.D0:
                case ConsoleKey.D1:
                case ConsoleKey.D2:
                case ConsoleKey.D3:
                case ConsoleKey.D4:
                case ConsoleKey.D5:
                case ConsoleKey.D6:
                case ConsoleKey.D7:
                case ConsoleKey.D8:
                case ConsoleKey.D9:
                case ConsoleKey.A:
                case ConsoleKey.B:
                case ConsoleKey.C:
                case ConsoleKey.D:
                case ConsoleKey.E:
                case ConsoleKey.F:
                case ConsoleKey.G:
                case ConsoleKey.H:
                case ConsoleKey.I:
                case ConsoleKey.J:
                case ConsoleKey.K:
                case ConsoleKey.L:
                case ConsoleKey.M:
                case ConsoleKey.N:
                case ConsoleKey.O:
                case ConsoleKey.P:
                case ConsoleKey.Q:
                case ConsoleKey.R:
                case ConsoleKey.S:
                case ConsoleKey.T:
                case ConsoleKey.U:
                case ConsoleKey.V:
                case ConsoleKey.W:
                case ConsoleKey.X:
                case ConsoleKey.Y:
                case ConsoleKey.Z:
                case ConsoleKey.NumPad0:
                case ConsoleKey.NumPad1:
                case ConsoleKey.NumPad2:
                case ConsoleKey.NumPad3:
                case ConsoleKey.NumPad4:
                case ConsoleKey.NumPad5:
                case ConsoleKey.NumPad6:
                case ConsoleKey.NumPad7:
                case ConsoleKey.NumPad8:
                case ConsoleKey.NumPad9:
                case ConsoleKey.Multiply:
                case ConsoleKey.Add:
                case ConsoleKey.Separator:
                case ConsoleKey.Subtract:
                case ConsoleKey.Decimal:
                case ConsoleKey.Divide:
                case ConsoleKey.Oem1:
                case ConsoleKey.OemPlus:
                case ConsoleKey.OemComma:
                case ConsoleKey.OemMinus:
                case ConsoleKey.OemPeriod:
                case ConsoleKey.Oem2:
                case ConsoleKey.Oem3:
                case ConsoleKey.Oem4:
                case ConsoleKey.Oem5:
                case ConsoleKey.Oem6:
                case ConsoleKey.Oem7:
                case ConsoleKey.Oem8:
                case ConsoleKey.Oem102:
                    this.HandleOutputKey(keyInfo);
                    break;
                default:
                    //ignore anything outside known bounds...
                    break;
            }
        }
        #endregion

        #region handle insert key
        private void HandleInsertKey()
        {
            _insertMode = !_insertMode;
        }
        #endregion

        #region handle output key
        private void HandleOutputKey(ConsoleKeyInfo keyInfo)
        {
            if (_length == _maxLength)
                throw new InvalidOperationException("Max masked input length exceeded.");

            if (_position < _length)
            {
                if (_insertMode)
                {
                    Console.Write(_isMasked ? MaskChar : keyInfo.KeyChar);
                }
                else
                {
                    //shift all buffer chars past current position to the right by 1
                    this.ShiftRight(_buffer, _position, _length);

                    //insert one space at current position to RIGHT of cursor, cursor left DOES NOT increment, shifts all chars to the right by 1
                    Console.Write("\x1B[1@");
                    Console.Write(_isMasked ? MaskChar : keyInfo.KeyChar);
                    _length += 1;
                }
            }
            else//position must eq length
            {
                Console.Write(_isMasked ? MaskChar : keyInfo.KeyChar);
                _length += 1;
            }
            _buffer[_position++] = keyInfo.KeyChar;
        }
        #endregion

        #region handle home key
        private void HandleHomeKey()
        {
            _position = 0;
            Console.CursorLeft = 0;
        }
        #endregion

        #region handle end key
        private void HandleEndKey()
        {
            _position = _length;
            Console.CursorLeft = _position;
        }
        #endregion

        #region handle left arrow key
        private void handleLeftArrowKey()
        {
            if (_position > 0)
            {
                _position -= 1;
                Console.Write("\x1B[1D");//left one position
            }
        }
        #endregion

        #region handle right arrow key
        private void HandleRightArrowKey()
        {
            if (_position < _length)
            {
                _position += 1;
                Console.Write("\x1B[1C");//right one position
            }
        }
        #endregion

        #region handle backspace key
        private void HandleBackspaceKey()
        {
            if (_position > 0)
            {
                Console.Write("\x1B[1D"); //left one position
                Console.Write("\x1B[1P");//delete 1 char shifting LEFT chars from the RIGHT

                //shift out the deleted char
                _position -= 1;
                this.ShiftLeft(_buffer, _position, _length);

                _length -= 1;
            }
        }
        #endregion

        #region handle delete key
        private void HandleDeleteKey()
        {
            if (_position < _length)
            {
                //shift out the deleted char
                this.ShiftLeft(_buffer, _position, _length);

                //length decreases due to the left shift
                _length -= 1;

                Console.Write("\x1B[1P");//delete 1 char shifting LEFT chars from the RIGHT
            }
        }
        #endregion

        #region handle escape key
        private void HandleEscapeKey()
        {
            //carriage return to 0, overwrite any mask chars with empty space, then set curser back to 0
            Console.Write("\r" + new string(' ', _length));
            Console.Write("\r");
            _length = 0;
            _position = 0;
        }
        #endregion

        #region handle up arrow key [un-mask]
        private void HandleUpArrowKey()
        {
            this.ToggleMask();
        }
        #endregion

        #region handle down arrow key [re-mask]
        private void HandleDownArrowKey()
        {
            this.ToggleMask();
        }
        #endregion

        #region toggle mask
        private void ToggleMask()
        {
            if (_isMasked)
                this.UnMaskOutput();
            else
                this.ReMaskOutput();
        }
        #endregion

        #region un mask output
        private void UnMaskOutput()
        {
            Console.CursorLeft = 0;
            Console.Write(_buffer, 0, _length);
            Console.CursorLeft = _position;
            _isMasked = false;
        }
        #endregion

        #region re mask output
        private void ReMaskOutput()
        {
            Console.CursorLeft = 0;
            for (int i = 0; i < _length; i++)
            {
                Console.Write(MaskChar);
            }
            Console.CursorLeft = _position;
            _isMasked = true;
        }
        #endregion

        #region shift left
        private void ShiftLeft(char[] target, int from, int to)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (to > target.Length)
                throw new ArgumentOutOfRangeException(nameof(to));

            do
            {
                target[from] = target[++from];
            }
            while (from < to);
        }
        #endregion

        #region shift right
        private void ShiftRight(char[] target, int from, int to)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (to > target.Length)
                throw new ArgumentOutOfRangeException(nameof(to));

            do
            {
                target[to] = target[--to];
            }
            while (to > from);

            target[from] = '\0';
        }
        #endregion
    }
}