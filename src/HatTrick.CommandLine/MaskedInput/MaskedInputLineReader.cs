// SPDX-License-Identifier: Apache-2.0
// Copyright (c) HatTrick Labs, LLC

using System;
using System.Collections.Generic;
using System.IO;

namespace HatTrick.CommandLine
{
    public class MaskedInputLineReader
    {
        #region constants
        private const char MaskChar = '*';

        private const int MaxLength = 65536;
        #endregion

        #region internals
        private List<char> _buffer;

        private int _position;
        private bool _pendingWrap;

        private bool _insertMode;
        private bool _isMasked;

        private TextWriter _writer;
        private int _width;

        //reports whether another key is already queued (Console.KeyAvailable in real usage) - lets
        //HandleInsertBurst tell a fast paste apart from slow human typing without needing bracketed
        //paste support.
        private Func<bool> _hasBufferedInput;

        //the console's actual, live width - compared against the cached _width each key to detect a
        //mid-read resize (see _width's comment above).
        private Func<int> _getCurrentWidth;
        #endregion

        #region constructors
        public MaskedInputLineReader()
        {
        }
        #endregion

        #region read masked input
        public string ReadMaskedInput()
        {
            //cursor position is real Console state, not something a test harness can fake, so this
            //check stays here rather than in the injectable core.
            if (Console.GetCursorPosition().Left > 0)
                throw new InvalidOperationException("Masked line input reader must start at console left position of 0.");

            return this.ReadMaskedInputCore(Console.Out, Console.BufferWidth, ReadConsoleKeys(), () => Console.KeyAvailable, () => Console.BufferWidth);
        }

        private static IEnumerable<ConsoleKeyInfo> ReadConsoleKeys()
        {
            while (true)
                yield return Console.ReadKey(true);
        }

        //the actual key-processing loop, decoupled from the real console so it can be driven by a
        //scripted key sequence against a StringWriter/fixed width in tests. hasBufferedInput defaults
        //to "nothing else queued" (i.e. no burst batching), and getCurrentWidth defaults to "never
        //resizes", when not supplied.
        internal string ReadMaskedInputCore(TextWriter writer, int width, IEnumerable<ConsoleKeyInfo> keys, Func<bool> hasBufferedInput = null, Func<int> getCurrentWidth = null)
        {
            _writer = writer;
            _width = width;
            _hasBufferedInput = hasBufferedInput ?? (() => false);
            _getCurrentWidth = getCurrentWidth ?? (() => _width);
            _buffer = new List<char>();
            _position = 0;
            _pendingWrap = false;
            _insertMode = false;
            _isMasked = true;

            bool cancelled = false;

            using (var enumerator = keys.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var input = enumerator.Current;

                    if (this.HasResized())
                    {
                        //the console changed shape out from under us - our row/column math is now
                        //unreliable, and any further rendering (even an "erase and bail" cleanup) would
                        //itself use the stale width and risk making the on-screen mess worse. abandon
                        //the read exactly like Escape: no exception, just null, with no further writes.
                        cancelled = true;
                        break;
                    }

                    if (input.Key == ConsoleKey.Enter)
                        break;

                    //typing/pasting into the middle of existing content is the one path expensive
                    //enough to need batching (see HandleInsertBurst) - everything else is handled key
                    //by key exactly as before.
                    if (IsOutputKey(input.Key) && _position < _buffer.Count && !_insertMode)
                    {
                        var burst = this.HandleInsertBurst(input, enumerator);
                        if (burst.stop)
                        {
                            cancelled = burst.cancelled;
                            break;
                        }
                        continue;
                    }

                    this.HandleKey(input);

                    if (input.Key == ConsoleKey.Escape)
                    {
                        cancelled = true;
                        break;
                    }
                }
            }

            if (cancelled)
                return null;

            if (!_isMasked)
                this.ReMaskOutput();

            //make sure the trailing newline lands after the last rendered row, not wherever the
            //caret happened to be left (e.g. user navigated with Home/arrows before pressing Enter).
            this.MoveCursorTo(_buffer.Count);

            string output = new string(_buffer.ToArray());
            _writer.WriteLine(string.Empty);
            return output;
        }
        #endregion

        #region handle key
        private void HandleKey(ConsoleKeyInfo keyInfo)
        {
            if (IsOutputKey(keyInfo.Key))
            {
                this.HandleOutputKey(keyInfo);
                return;
            }

            switch (keyInfo.Key)
            {
                case ConsoleKey.Backspace:
                    this.HandleBackspaceKey();
                    break;
                case ConsoleKey.Escape:
                    this.HandleEscapeKey();
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
                default:
                    //ignore anything outside known bounds...
                    break;
            }
        }
        #endregion

        #region is output key
        private static bool IsOutputKey(ConsoleKey key)
        {
            switch (key)
            {
                case ConsoleKey.Spacebar:
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
                    return true;
                default:
                    return false;
            }
        }
        #endregion

        #region has resized
        private bool HasResized() => _getCurrentWidth() != _width;
        #endregion

        #region handle insert burst
        private (bool stop, bool cancelled) HandleInsertBurst(ConsoleKeyInfo first, IEnumerator<ConsoleKeyInfo> keys)
        {
            //batches a run of characters landing in the middle of existing content into a single
            //List<char>.InsertRange + a single tail repaint, instead of one InsertRange(1 char)/repaint
            //per character...a burst only continues while more input is already queued
            //(a paste arrives far faster than a human types), so ordinary keystroke-by-keystroke typing
            //is unaffected - it just yields batches of size 1.
            int fromIndex = _position;
            var batch = new List<char>();
            this.AppendToBurst(fromIndex, batch, first.KeyChar);

            while (_hasBufferedInput() && keys.MoveNext())
            {
                var next = keys.Current;

                if (this.HasResized())
                {
                    //bail without flushing - the whole read is being abandoned, so there's no point
                    //rendering (with now-stale width math) content that's about to be discarded anyway.
                    return (true, true);
                }

                if (next.Key == ConsoleKey.Enter)
                {
                    this.FlushInsertBurst(fromIndex, batch);
                    return (true, false);
                }

                if (next.Key == ConsoleKey.Escape)
                {
                    this.FlushInsertBurst(fromIndex, batch);
                    this.HandleEscapeKey();
                    return (true, true);
                }

                if (IsOutputKey(next.Key))
                {
                    this.AppendToBurst(fromIndex, batch, next.KeyChar);
                    continue;
                }

                //not part of the burst (arrow key, backspace, Insert-mode toggle, etc.) - flush what
                //we have, then let this key run through the normal single-key path.
                this.FlushInsertBurst(fromIndex, batch);
                this.HandleKey(next);
                return (false, false);
            }

            this.FlushInsertBurst(fromIndex, batch);
            return (false, false);
        }

        private void FlushInsertBurst(int fromIndex, List<char> batch)
        {
            _buffer.InsertRange(fromIndex, batch);
            this.RepaintTail(fromIndex, _buffer.Count, fromIndex + batch.Count, eraseTrailing: false);
        }

        private void AppendToBurst(int fromIndex, List<char> batch, char c)
        {
            //checked per-character, not just once at flush time, so a pathologically large paste stops
            //growing the in-memory batch (and gets a clear error) instead of first buffering the whole
            //thing - the cap is a memory bound, so it has to apply before the allocation, not after.
            if (_buffer.Count + batch.Count >= MaxLength)
            {
                this.FlushInsertBurst(fromIndex, batch);
                throw new InvalidOperationException($"Masked input exceeds the maximum length of {MaxLength} characters.");
            }

            batch.Add(c);
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
            //reaching here means either appending at the end or overwrite mode - growing the buffer
            //mid-string is intercepted earlier, in the read loop, by HandleInsertBurst so a paste can
            //be batched into one splice instead of one per character.
            char ch = keyInfo.KeyChar;

            if (_insertMode && _position < _buffer.Count)
            {
                //overwrite mode (toggled via Insert key): replace in place, no reflow needed
                _buffer[_position] = ch;
                this.Echo(_isMasked ? MaskChar : ch);
            }
            else
            {
                if (_buffer.Count >= MaxLength)
                    throw new InvalidOperationException($"Masked input exceeds the maximum length of {MaxLength} characters.");

                _buffer.Add(ch);
                this.Echo(_isMasked ? MaskChar : ch);
            }
        }
        #endregion

        #region handle home key
        private void HandleHomeKey()
        {
            this.MoveCursorTo(0);
        }
        #endregion

        #region handle end key
        private void HandleEndKey()
        {
            this.MoveCursorTo(_buffer.Count);
        }
        #endregion

        #region handle left arrow key
        private void handleLeftArrowKey()
        {
            if (_position > 0)
                this.MoveCursorTo(_position - 1);
        }
        #endregion

        #region handle right arrow key
        private void HandleRightArrowKey()
        {
            if (_position < _buffer.Count)
                this.MoveCursorTo(_position + 1);
        }
        #endregion

        #region handle backspace key
        private void HandleBackspaceKey()
        {
            if (_position > 0)
            {
                int removeIndex = _position - 1;
                _buffer.RemoveAt(removeIndex);
                this.RepaintTail(removeIndex, _buffer.Count, removeIndex, eraseTrailing: true);
            }
        }
        #endregion

        #region handle delete key
        private void HandleDeleteKey()
        {
            if (_position < _buffer.Count)
            {
                _buffer.RemoveAt(_position);
                this.RepaintTail(_position, _buffer.Count, _position, eraseTrailing: true);
            }
        }
        #endregion

        #region handle escape key
        private void HandleEscapeKey()
        {
            //carriage return to the true origin, erase every rendered row (not just the current one), reset state
            this.MoveCursorTo(0);
            _writer.Write("\x1B[0J");
            _buffer.Clear();
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
            this.RenderFull(masked: false);
            _isMasked = false;
        }
        #endregion

        #region re mask output
        private void ReMaskOutput()
        {
            this.RenderFull(masked: true);
            _isMasked = true;
        }
        #endregion

        #region render full
        private void RenderFull(bool masked)
        {
            //repaint every row from the origin, then restore the caret to where it logically is.
            //writing the whole span in one shot (rather than char-by-char) is safe here because we
            //always start the write at column 0 of a row, so the terminal's own line wrap lays out
            //subsequent rows exactly the way our position/BufferWidth math expects.
            int caret = _position;
            this.MoveCursorTo(0);

            if (_buffer.Count > 0)
                _writer.Write(masked ? new string(MaskChar, _buffer.Count) : new string(_buffer.ToArray()));

            _position = _buffer.Count;
            _pendingWrap = _buffer.Count > 0 && _buffer.Count % _width == 0;

            this.MoveCursorTo(caret);
        }
        #endregion

        #region repaint tail
        private void RepaintTail(int fromIndex, int toIndexExclusive, int caretTarget, bool eraseTrailing)
        {
            //used after an insert/delete: repaint from the edit point up to (not including)
            //toIndexExclusive, optionally erasing whatever used to occupy the now-shorter tail, then
            //restore the caret.
            this.MoveCursorTo(fromIndex);

            for (int i = fromIndex; i < toIndexExclusive; i++)
                this.Echo(_isMasked ? MaskChar : _buffer[i]);

            if (eraseTrailing)
            {
                this.ResolvePendingWrap();
                _writer.Write("\x1B[0J");
            }

            this.MoveCursorTo(caretTarget);
        }
        #endregion

        #region echo
        private void Echo(char c)
        {
            //writes exactly one visible character and advances the caret...deliberately relies on the
            //terminal's own autowrap when a row fills up, rather than an explicit cursor move
            _writer.Write(c);
            _position++;
            _pendingWrap = _position % _width == 0;
        }
        #endregion

        #region resolve pending wrap
        private void ResolvePendingWrap()
        {
            //most VT-compatible terminals defer wrapping to the next row until another character is
            //printed (the "pending wrap" / DECAWM last-column quirk) - the cursor doesn't actually
            //move until then. an explicit position command (CUU/CUD/CHA) does NOT trigger that
            //deferred move on its own, so anything that positions the cursor explicitly must force
            //it first, or the row/column math (based purely on buffer index / BufferWidth) will be
            //off by one row versus where the terminal's real cursor sits.
            if (!_pendingWrap)
                return;

            _writer.Write("\x1B[1B\x1B[1G");
            _pendingWrap = false;
        }
        #endregion

        #region move cursor to
        private void MoveCursorTo(int index)
        {
            //moves the cursor to an arbitrary buffer index, wherever that lands across however many
            //rows the input currently spans. uses only cursor-relative escapes (CUU/CUD to change row,
            //CHA to set the absolute column within that row) rather than Console.CursorLeft/CursorTop
            //or SetCursorPosition
            this.ResolvePendingWrap();

            int currentRow = _position / _width;
            int targetRow = index / _width;
            int deltaRows = targetRow - currentRow;

            if (deltaRows > 0)
                _writer.Write($"\x1B[{deltaRows}B");
            else if (deltaRows < 0)
                _writer.Write($"\x1B[{-deltaRows}A");

            _writer.Write($"\x1B[{(index % _width) + 1}G");
            _position = index;
        }
        #endregion
    }
}
