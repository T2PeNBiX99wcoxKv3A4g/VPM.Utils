using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;

namespace io.github.ykysnk.utils.Editor.HarmonyUtilities
{
    [PublicAPI]
    public static class TranspilerExtensions
    {
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

    [PublicAPI]
    public readonly struct InstructionContext
    {
        public int Index { get; }
        public CodeInstruction? Previous { get; }
        public CodeInstruction Current { get; }
        public CodeInstruction? Next { get; }

        public InstructionContext(int index, CodeInstruction? previous, CodeInstruction current, CodeInstruction? next)
        {
            Index = index;
            Previous = previous;
            Current = current;
            Next = next;
        }

        public bool IsFirst => Previous is null;
        public bool IsLast => Next is null;

        public bool Is(OpCode opcode) => Current.opcode == opcode;

        public bool Calls(MethodInfo method) => Current.Calls(method);
    }
}