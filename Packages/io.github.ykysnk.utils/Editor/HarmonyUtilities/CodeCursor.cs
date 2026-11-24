using System;
using System.Collections.Generic;
using HarmonyLib;
using JetBrains.Annotations;

namespace io.github.ykysnk.utils.Editor.HarmonyUtilities
{
    /// <summary>
    ///     Streaming cursor over the instructions of a Harmony transpiler.
    ///     <para>
    ///         Gives access to <see cref="Current" />, a one-instruction <see cref="Next" /> peek, the already visited
    ///         instructions through <see cref="Previous" /> and the current <see cref="Index" /> while enumerating the
    ///         sequence only once, so there is no need to materialise it with <c>ToList()</c>.
    ///     </para>
    /// </summary>
    [PublicAPI]
    public sealed class CodeCursor : IDisposable
    {
        private readonly IEnumerator<CodeInstruction> _enumerator;
        private readonly List<CodeInstruction> _history = new();

        private bool _hasNextBuffered;
        private bool _started;

        /// <summary>Creates a cursor over <paramref name="instructions" />. Do not enumerate the sequence elsewhere.</summary>
        public CodeCursor(IEnumerable<CodeInstruction> instructions) => _enumerator = instructions.GetEnumerator();

        /// <summary>Zero-based position of <see cref="Current" />, or -1 before the first <see cref="MoveNext" />.</summary>
        public int Index { get; private set; } = -1;

        /// <summary>The instruction the cursor points at, or null before the first step and after the end.</summary>
        public CodeInstruction? Current { get; private set; }

        /// <summary>Peek of the next instruction, or null when the cursor is on the last one.</summary>
        public CodeInstruction? Next { get; private set; }

        /// <summary>Whether the cursor is on the first instruction.</summary>
        public bool IsFirst => Index == 0;

        /// <summary>Whether the cursor is on the last instruction, i.e. there is nothing left to peek.</summary>
        public bool IsLast => _started && Next == null;

        /// <summary>
        ///     Looks up an instruction relative to <see cref="Current" />: negative values go back in history,
        ///     0 returns the current one and 1 returns <see cref="Next" />.
        /// </summary>
        /// <exception cref="NotSupportedException">A positive offset larger than 1 was requested.</exception>
        public CodeInstruction? this[int offset]
        {
            get
            {
                return offset switch
                {
                    0 => Current,
                    < 0 => Previous(-offset),
                    1 => Next,
                    _ => throw new NotSupportedException("Streaming cursor cannot peek more than one instruction ahead.")
                };
            }
        }

        /// <summary>Releases the underlying enumerator.</summary>
        public void Dispose() => _enumerator.Dispose();

        /// <summary>
        ///     Advances to the next instruction and buffers the following one for <see cref="Next" />.
        ///     Returns false once the cursor has moved past the last instruction.
        /// </summary>
        public bool MoveNext()
        {
            if (!_started)
            {
                _started = true;

                if (!_enumerator.MoveNext())
                    return false;

                Current = _enumerator.Current;
                Index = 0;

                _hasNextBuffered = _enumerator.MoveNext();
                Next = _hasNextBuffered ? _enumerator.Current : null;

                return true;
            }

            if (Current == null)
                return false;

            _history.Add(Current);

            if (!_hasNextBuffered)
            {
                Current = null;
                Next = null;
                return false;
            }

            Current = Next;
            Index++;

            _hasNextBuffered = _enumerator.MoveNext();
            Next = _hasNextBuffered ? _enumerator.Current : null;

            return true;
        }

        /// <summary>
        ///     Returns an already visited instruction: <c>Previous(1)</c> is the one before <see cref="Current" />,
        ///     <c>Previous(2)</c> the one before that, and so on. Returns null when the history is not deep enough.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="distance" /> is zero or negative.</exception>
        public CodeInstruction? Previous(int distance = 1)
        {
            if (distance <= 0)
                throw new ArgumentOutOfRangeException(nameof(distance));

            var index = _history.Count - distance;
            return index >= 0 ? _history[index] : null;
        }
    }
}
