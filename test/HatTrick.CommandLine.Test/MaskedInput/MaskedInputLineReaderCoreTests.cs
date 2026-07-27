// SPDX-License-Identifier: Apache-2.0
// Copyright (c) HatTrick Labs, LLC

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace HatTrick.CommandLine.Test
{
    public class MaskedInputLineReaderCoreTests
    {
        #region key builders
        //ConsoleKey only drives HandleKey's dispatch; HandleOutputKey reads KeyChar for the actual
        //character, so ConsoleKey.A works as a stand-in "any printable key" for scripted input.
        private static ConsoleKeyInfo CharKey(char c) => new ConsoleKeyInfo(c, ConsoleKey.A, false, false, false);

        private static IEnumerable<ConsoleKeyInfo> CharKeys(string s)
        {
            foreach (var c in s)
                yield return CharKey(c);
        }

        private static ConsoleKeyInfo Key(ConsoleKey key) => new ConsoleKeyInfo('\0', key, false, false, false);

        private static IEnumerable<ConsoleKeyInfo> Concat(params IEnumerable<ConsoleKeyInfo>[] sequences)
        {
            foreach (var seq in sequences)
                foreach (var key in seq)
                    yield return key;
        }
        #endregion

        #region paste larger than one row
        [Fact]
        public void ReadMaskedInputCore_PasteLongerThanBufferWidth_ReturnsFullStringWithoutThrowing()
        {
            //this is the reported bug: pasting a value longer than the console's line buffer used to
            //throw 'Max masked input length exceeded.' because the buffer was hard-capped at
            //Console.BufferWidth - 1.
            var reader = new MaskedInputLineReader();
            string pasted = new string('x', 60); //3x a 20-column width
            var keys = Concat(CharKeys(pasted), new[] { Key(ConsoleKey.Enter) });

            string result = reader.ReadMaskedInputCore(TextWriter.Null, 20, keys);

            Assert.Equal(pasted, result);
        }
        #endregion

        #region exact row boundary + mask toggle
        [Fact]
        public void ReadMaskedInputCore_ExactRowFill_ThenToggleMaskTwice_DoesNotThrow()
        {
            //typing exactly `width` chars leaves the reader in the deferred-autowrap ("pending wrap")
            //state; toggling mask (Up then Down) forces a reposition to column 0, which must resolve
            //that pending wrap before doing its row math or it ends up one row off.
            var reader = new MaskedInputLineReader();
            string input = new string('a', 20);
            var keys = Concat(
                CharKeys(input),
                new[] { Key(ConsoleKey.UpArrow), Key(ConsoleKey.DownArrow), Key(ConsoleKey.Enter) });

            string result = reader.ReadMaskedInputCore(TextWriter.Null, 20, keys);

            Assert.Equal(input, result);
        }

        [Fact]
        public void ReadMaskedInputCore_ExactRowFill_UnmaskEmitsResolvedWrapBeforeRepositioning()
        {
            var writer = new StringWriter();
            var reader = new MaskedInputLineReader();
            string input = new string('a', 20);
            var keys = Concat(CharKeys(input), new[] { Key(ConsoleKey.UpArrow), Key(ConsoleKey.Enter) });

            reader.ReadMaskedInputCore(writer, 20, keys);

            string output = writer.ToString();
            //after 20 chars, pendingWrap is true; RenderFull's MoveCursorTo(0) must resolve it
            //(down 1 / column 1) before computing the up-move back to row 0.
            int wrapResolve = output.IndexOf("\x1B[1B\x1B[1G", StringComparison.Ordinal);
            Assert.True(wrapResolve >= 0, "expected the pending-wrap resolve sequence to be emitted");

            int cuuAfterResolve = output.IndexOf("\x1B[1A", wrapResolve, StringComparison.Ordinal);
            Assert.True(cuuAfterResolve > wrapResolve, "expected a cursor-up to follow the wrap resolve");
        }
        #endregion

        #region backspace across a row boundary
        [Fact]
        public void ReadMaskedInputCore_BackspaceAtRowBoundary_EmptyingOverflowRow_ReturnsShortenedString()
        {
            //21 chars at width 20 spills exactly one char into row 1; backspacing it off again must
            //land the caret back at row 0 without throwing, and must erase the now-stale row 1.
            var reader = new MaskedInputLineReader();
            string input = new string('a', 21);
            var keys = Concat(CharKeys(input), new[] { Key(ConsoleKey.Backspace), Key(ConsoleKey.Enter) });

            string result = reader.ReadMaskedInputCore(TextWriter.Null, 20, keys);

            Assert.Equal(new string('a', 20), result);
        }
        #endregion

        #region navigation across multiple rows
        [Fact]
        public void ReadMaskedInputCore_HomeThenInsertAtStart_AfterMultiRowPaste_InsertsAtCorrectPosition()
        {
            var reader = new MaskedInputLineReader();
            string pasted = new string('a', 45); //spans 3 rows at width 20
            var keys = Concat(
                CharKeys(pasted),
                new[] { Key(ConsoleKey.Home) },
                CharKeys("Z"),
                new[] { Key(ConsoleKey.Enter) });

            string result = reader.ReadMaskedInputCore(TextWriter.Null, 20, keys);

            Assert.Equal("Z" + pasted, result);
        }

        [Fact]
        public void ReadMaskedInputCore_DeleteMidSecondRow_AfterMultiRowPaste_RemovesCorrectChar()
        {
            var reader = new MaskedInputLineReader();
            //row0: 0-19, row1: 20-39 - target index 25 sits mid-row-1
            var sb = new StringBuilder();
            for (int i = 0; i < 40; i++)
                sb.Append((char)('A' + (i % 26)));
            string pasted = sb.ToString();

            var keys = Concat(
                CharKeys(pasted),
                new[] { Key(ConsoleKey.Home) },
                Repeat(Key(ConsoleKey.RightArrow), 25),
                new[] { Key(ConsoleKey.Delete), Key(ConsoleKey.Enter) });

            string result = reader.ReadMaskedInputCore(TextWriter.Null, 20, keys);

            string expected = pasted.Remove(25, 1);
            Assert.Equal(expected, result);
        }

        private static IEnumerable<ConsoleKeyInfo> Repeat(ConsoleKeyInfo key, int count)
        {
            for (int i = 0; i < count; i++)
                yield return key;
        }
        #endregion

        #region paste mid-buffer performance
        //counts every character actually written to the terminal - the proxy for real-world cost,
        //since each one is a console syscall in the non-test path.
        private sealed class CountingWriter : TextWriter
        {
            public int WriteCount { get; private set; }
            public override Encoding Encoding => Encoding.UTF8;
            public override void Write(char value) => WriteCount++;
        }

        [Fact]
        public void ReadMaskedInputCore_PasteAtStart_AfterExistingContent_BatchesIntoRoughlyLinearWriteCount()
        {
            //reported bug: pasting into the middle of existing content (e.g. Home, then paste) repainted
            //the entire tail once per pasted character - O(existing length * pasted length) console writes.
            //hasBufferedInput: () => true simulates "this whole run arrived as a fast paste" - the
            //underlying key sequence is still finite, so the loop terminates normally regardless.
            var writer = new CountingWriter();
            var reader = new MaskedInputLineReader();

            string existing = new string('a', 2000);
            string pasted = new string('b', 2000);

            var keys = Concat(
                CharKeys(existing),
                new[] { Key(ConsoleKey.Home) },
                CharKeys(pasted),
                new[] { Key(ConsoleKey.Enter) });

            string result = reader.ReadMaskedInputCore(writer, 80, keys, hasBufferedInput: () => true);

            Assert.Equal(pasted + existing, result);

            //unbatched (one full tail repaint per pasted character) would be on the order of
            //existing.Length * pasted.Length writes (~4,000,000 here); batched is linear in the
            //combined length. 20,000 is a generous linear ceiling that only a quadratic regression
            //would blow through.
            Assert.True(writer.WriteCount < 20_000, $"expected a roughly linear write count, got {writer.WriteCount}");
        }
        #endregion

        #region max length cap
        //must match MaskedInputLineReader's private MaxLength constant - not exposed publicly, so
        //hardcoded here deliberately.
        private const int MaxLength = 65536;

        [Fact]
        public void ReadMaskedInputCore_AppendExactlyAtCap_Succeeds()
        {
            var reader = new MaskedInputLineReader();
            string input = new string('a', MaxLength);
            var keys = Concat(CharKeys(input), new[] { Key(ConsoleKey.Enter) });

            string result = reader.ReadMaskedInputCore(TextWriter.Null, 80, keys);

            Assert.Equal(input, result);
        }

        [Fact]
        public void ReadMaskedInputCore_AppendPastCap_Throws()
        {
            var reader = new MaskedInputLineReader();
            string input = new string('a', MaxLength + 1);
            var keys = Concat(CharKeys(input), new[] { Key(ConsoleKey.Enter) });

            var ex = Assert.Throws<InvalidOperationException>(() => reader.ReadMaskedInputCore(TextWriter.Null, 80, keys));
            Assert.Contains(MaxLength.ToString(), ex.Message);
        }

        [Fact]
        public void ReadMaskedInputCore_PasteMidBufferPastCap_Throws()
        {
            //same cap, exercised through the batching path (Home, then a paste that would push the
            //buffer over the limit) rather than plain append.
            var reader = new MaskedInputLineReader();
            string existing = new string('a', MaxLength - 10);
            string pasted = new string('b', 20);

            var keys = Concat(
                CharKeys(existing),
                new[] { Key(ConsoleKey.Home) },
                CharKeys(pasted),
                new[] { Key(ConsoleKey.Enter) });

            Assert.Throws<InvalidOperationException>(() =>
                reader.ReadMaskedInputCore(TextWriter.Null, 80, keys, hasBufferedInput: () => true));
        }
        #endregion

        #region escape cancels
        [Fact]
        public void ReadMaskedInputCore_EscapeAfterMultiRowPaste_ReturnsNull()
        {
            var reader = new MaskedInputLineReader();
            string pasted = new string('a', 50);
            var keys = Concat(CharKeys(pasted), new[] { Key(ConsoleKey.Escape) });

            string result = reader.ReadMaskedInputCore(TextWriter.Null, 20, keys);

            Assert.Null(result);
        }
        #endregion

        #region resize mid-read
        //simulates Console.BufferWidth changing partway through a read: returns startWidth for the
        //first `changeAfter` calls, then a different value for every call after that.
        private static Func<int> ResizeAfter(int startWidth, int changeAfter)
        {
            int calls = 0;
            return () => (++calls > changeAfter) ? startWidth + 40 : startWidth;
        }

        [Fact]
        public void ReadMaskedInputCore_ResizeDuringPlainTyping_ReturnsNullWithoutThrowing()
        {
            var reader = new MaskedInputLineReader();
            var keys = Concat(CharKeys(new string('a', 30)), new[] { Key(ConsoleKey.Enter) });

            //width check runs once per key; let 10 keys through at the original width, then "resize".
            string result = reader.ReadMaskedInputCore(TextWriter.Null, 80, keys, getCurrentWidth: ResizeAfter(80, 10));

            Assert.Null(result);
        }

        [Fact]
        public void ReadMaskedInputCore_ResizeMidBurst_ReturnsNullAndDiscardsTheBatch()
        {
            //resize detected while HandleInsertBurst is mid-batch (Home, then a paste) - must bail
            //without flushing the partially-collected batch, not return a truncated string.
            var reader = new MaskedInputLineReader();
            string existing = new string('a', 30);
            string pasted = new string('b', 30);

            var keys = Concat(
                CharKeys(existing),
                new[] { Key(ConsoleKey.Home) },
                CharKeys(pasted),
                new[] { Key(ConsoleKey.Enter) });

            //let typing + Home + a few pasted chars through, then "resize" partway into the burst.
            string result = reader.ReadMaskedInputCore(
                TextWriter.Null, 80, keys,
                hasBufferedInput: () => true,
                getCurrentWidth: ResizeAfter(80, 35));

            Assert.Null(result);
        }

        [Fact]
        public void ReadMaskedInputCore_NoResize_UnaffectedByResizeCheck()
        {
            //sanity check that a stable width (the default when getCurrentWidth isn't supplied) never
            //trips the resize check - i.e. this feature is opt-in via the parameter, not a regression
            //risk for every other test in this file that doesn't pass it.
            var reader = new MaskedInputLineReader();
            string input = "hello";
            var keys = Concat(CharKeys(input), new[] { Key(ConsoleKey.Enter) });

            string result = reader.ReadMaskedInputCore(TextWriter.Null, 80, keys);

            Assert.Equal(input, result);
        }
        #endregion
    }
}
