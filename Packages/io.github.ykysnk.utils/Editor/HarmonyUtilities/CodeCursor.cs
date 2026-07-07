using System;
using System.Collections.Generic;
using HarmonyLib;
using JetBrains.Annotations;

// Made by ChatGPT

namespace io.github.ykysnk.utils.Editor.HarmonyUtilities
{
    /// <summary>
    ///     Streaming cursor for Harmony transpilers without calling ToList().
    ///     Supports Current/Next/Previous/Index while enumerating only once.
    /// </summary>
    [PublicAPI]
    public sealed class CodeCursor : IDisposable
    {
        private readonly IEnumerator<CodeInstruction> _enumerator;
        private readonly List<CodeInstruction> _history = new();

        private bool _hasNextBuffered;
        private bool _started;

        public CodeCursor(IEnumerable<CodeInstruction> instructions) => _enumerator = instructions.GetEnumerator();

        public int Index { get; private set; } = -1;

        public CodeInstruction? Current { get; private set; }

        /// <summary>Peek of the next instruction, or null if at end.</summary>
        public CodeInstruction? Next { get; private set; }

        public bool IsFirst => Index == 0;
        public bool IsLast => _started && Next == null;

        /// <summary>
        ///     Negative = previous, 0 = current, +1 = next.
        ///     Positive values larger than 1 are unsupported in streaming mode.
        /// </summary>
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

        public void Dispose() => _enumerator.Dispose();

        /// <summary>
        ///     Advances to the next instruction.
        ///     Returns false after the last instruction has already been processed.
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
        ///     Gets a previous instruction.
        ///     Previous(1) = previous instruction.
        ///     Previous(2) = two instructions back.
        ///     Returns null if out of range.
        /// </summary>
        public CodeInstruction? Previous(int distance = 1)
        {
            if (distance <= 0)
                throw new ArgumentOutOfRangeException(nameof(distance));

            var index = _history.Count - distance;
            return index >= 0 ? _history[index] : null;
        }
    }
}