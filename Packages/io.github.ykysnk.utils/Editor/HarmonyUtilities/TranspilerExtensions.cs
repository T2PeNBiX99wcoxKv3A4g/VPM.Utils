using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;

namespace io.github.ykysnk.utils.Editor.HarmonyUtilities
{
    /// <summary>
    ///     Harmony transpiler helper for walking an instruction list with its neighbours, so a patch can match short
    ///     instruction sequences without tracking indices itself.
    /// </summary>
    [PublicAPI]
    public static class TranspilerExtensions
    {
        /// <summary>
        ///     Yields each instruction together with the one before and after it, in order.
        /// </summary>
        /// <param name="instructions">Instructions to walk; they are enumerated once, lazily.</param>
        /// <returns>One <see cref="InstructionContext" /> per instruction; empty for an empty list.</returns>
        public static IEnumerable<InstructionContext> WithContext(this IEnumerable<CodeInstruction> instructions)
        {
            using var enumerator = instructions.GetEnumerator();

            if (!enumerator.MoveNext())
                yield break;

            var index = 0;

            CodeInstruction? previous = null;
            var current = enumerator.Current;

            while (true)
            {
                var hasNext = enumerator.MoveNext();
                var next = hasNext ? enumerator.Current : null;

                yield return new(index++, previous, current!, next);

                if (!hasNext)
                    break;

                previous = current;
                current = next!;
            }
        }
    }

    /// <summary>
    ///     An instruction plus its position and neighbours in a transpiler instruction list.
    /// </summary>
    [PublicAPI]
    public readonly struct InstructionContext
    {
        /// <summary>
        ///     Position of <see cref="Current" /> in the original list, starting at zero.
        /// </summary>
        public int Index { get; }

        /// <summary>
        ///     Instruction before <see cref="Current" />, or <c>null</c> when it is the first one.
        /// </summary>
        public CodeInstruction? Previous { get; }

        /// <summary>
        ///     Instruction this context describes.
        /// </summary>
        public CodeInstruction Current { get; }

        /// <summary>
        ///     Instruction after <see cref="Current" />, or <c>null</c> when it is the last one.
        /// </summary>
        public CodeInstruction? Next { get; }

        /// <summary>
        ///     Creates a context for one instruction.
        /// </summary>
        /// <param name="index">Position of the instruction in the original list.</param>
        /// <param name="previous">Instruction before it, or <c>null</c> for the first one.</param>
        /// <param name="current">Instruction to describe.</param>
        /// <param name="next">Instruction after it, or <c>null</c> for the last one.</param>
        public InstructionContext(int index, CodeInstruction? previous, CodeInstruction current, CodeInstruction? next)
        {
            Index = index;
            Previous = previous;
            Current = current;
            Next = next;
        }

        /// <summary>
        ///     Whether <see cref="Current" /> is the first instruction of the list.
        /// </summary>
        public bool IsFirst => Previous is null;

        /// <summary>
        ///     Whether <see cref="Current" /> is the last instruction of the list.
        /// </summary>
        public bool IsLast => Next is null;

        /// <summary>
        ///     Whether <see cref="Current" /> has the given opcode.
        /// </summary>
        /// <param name="opcode">Opcode to compare against.</param>
        /// <returns><c>true</c> when the opcodes match.</returns>
        public bool Is(OpCode opcode) => Current.opcode == opcode;

        /// <summary>
        ///     Whether <see cref="Current" /> is a call to the given method, using Harmony's own matching.
        /// </summary>
        /// <param name="method">Method or constructor to look for.</param>
        /// <returns><c>true</c> when the instruction calls that method.</returns>
        public bool Calls(MethodInfo method) => Current.Calls(method);
    }
}