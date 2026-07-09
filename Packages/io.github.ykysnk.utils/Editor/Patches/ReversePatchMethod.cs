using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using HarmonyLib;
using JetBrains.Annotations;
using ILogger = io.github.ykysnk.utils.NonUdon.Logger.ILogger;
using Object = UnityEngine.Object;

namespace io.github.ykysnk.utils.Editor.Patches
{
    [PublicAPI]
    public interface IReversePatchMethod
    {
        string QualifiedName { get; }
        string DisplayName { get; }
        bool Enabled { get; }
        MethodInfo? TargetMethod { get; }
        MethodInfo? TargetReverse { get; }
        MethodInfo? TargetTranspiler { get; }
        HarmonyReversePatchType ReverseType { get; }

        int Priority { get; }
        string[]? Before { get; }
        string[]? After { get; }
        bool? Debug { get; }

        bool Prepare(MethodInfo? original, Harmony harmony);

        Exception? Cleanup(Exception? exception, Harmony harmony, MethodInfo? original);
        void Patch(Harmony harmony);
    }

    [PublicAPI]
    public abstract class ReversePatchMethod<T> : IReversePatchMethod, ILogger where T : ReversePatchMethod<T>, new()
    {
        private static readonly Lazy<T> InstanceInternal = new(() => new(), LazyThreadSafetyMode.ExecutionAndPublication);

        public static T Instance => InstanceInternal.Value;
        public static Type ThisType { get; } = typeof(T);
        public virtual string ReverseMethod { get; } = "Reverse";
        public virtual string TranspilerMethod { get; } = "Transpiler";
        public void Log(object? message) => Utils.Log(DisplayName, message);
        public void Log(object? message, Object context) => Utils.Log(DisplayName, message, context);

        public void LogWarning(object? message) => Utils.LogWarning(DisplayName, message);
        public void LogWarning(object? message, Object context) => Utils.LogWarning(DisplayName, message, context);

        public void LogError(object? message) => Utils.LogError(DisplayName, message);
        public void LogError(object? message, Object context) => Utils.LogError(DisplayName, message, context);

        public void Assert(bool condition, object? message) => Utils.Assert(condition, DisplayName, message);

        public void Assert(bool condition, object? message, Object context) =>
            Utils.Assert(condition, DisplayName, message, context);

        public virtual string QualifiedName { get; } = ThisType.FullName ?? ThisType.Name;
        public virtual string DisplayName { get; } = ThisType.Name;
        public virtual bool Enabled { get; } = true;
        public virtual MethodInfo? TargetMethod { get; }
        public virtual MethodInfo? TargetReverse => Method(ReverseMethod);
        public virtual MethodInfo? TargetTranspiler => Method(TranspilerMethod);
        public virtual HarmonyReversePatchType ReverseType { get; } = HarmonyReversePatchType.Original;
        public virtual int Priority { get; } = -1;
        public virtual string[]? Before { get; }
        public virtual string[]? After { get; }
        public virtual bool? Debug { get; }

        bool IReversePatchMethod.Prepare(MethodInfo? original, Harmony harmony) => Prepare(original, harmony);

        Exception? IReversePatchMethod.Cleanup(Exception? exception, Harmony harmony, MethodInfo? original) =>
            Cleanup(exception, harmony, original);

        void IReversePatchMethod.Patch(Harmony harmony) => Patch(harmony);
        public virtual bool Prepare(MethodInfo? original, Harmony harmony) => true;

        public virtual Exception? Cleanup(Exception? exception, Harmony harmony, MethodInfo? original) => exception;

        public static void Log2(object? message) => Utils.Log(ThisType.Name, message);
        public static void Log2(object? message, Object context) => Utils.Log(ThisType.Name, message, context);
        public static void LogWarning2(object? message) => Utils.LogWarning(ThisType.Name, message);

        public static void LogWarning2(object? message, Object context) =>
            Utils.LogWarning(ThisType.Name, message, context);

        public static void LogError2(object? message) => Utils.LogError(ThisType.Name, message);
        public static void LogError2(object? message, Object context) => Utils.LogError(ThisType.Name, message, context);

        public static void Assert2(bool condition, object? message) => Utils.Assert(condition, ThisType.Name, message);

        public static void Assert2(bool condition, object? message, Object context) =>
            Utils.Assert(condition, ThisType.Name, message, context);

        protected static MethodInfo? Method(string name)
        {
            var key = $"{ThisType.FullName}.{name}";
            if (PatchCore.MethodCache.TryGetValue(key, out var cache))
                return cache;
            var method = AccessTools.Method(ThisType, name);
            if (method == null) return null;
            return PatchCore.MethodCache[key] = method;
        }

        internal static MethodInfo? GetTranspiler(MethodInfo method)
        {
            var methodName = method.Name;
            List<MethodInfo> declaredMethods = AccessTools.GetDeclaredMethods(method.DeclaringType);
            var ici = typeof(IEnumerable<CodeInstruction>);
            var predicate = (Func<MethodInfo, bool>)(m => !(m.ReturnType != ici) && m.Name.StartsWith($"<{methodName}>"));
            return declaredMethods.FirstOrDefault(predicate);
        }

        internal void Patch(Harmony harmony)
        {
            var original = TargetMethod;
            var reverse = TargetReverse;
            var reverseTranspiler = TargetTranspiler;
            Exception? exception = null;

            if (reverse == null)
            {
                LogWarning("Failed to apply reverse patch: no patch methods specified.");
                return;
            }

            try
            {
                if (!Enabled || !Prepare(original, harmony))
                    return;

                if (original == null)
                {
                    LogWarning("Failed to apply reverse patch: target method not found.");
                    return;
                }

                Harmony.ReversePatch(original, new(reverse, Priority, Before, After, Debug),
                    reverseTranspiler == null ? GetTranspiler(reverse) : reverseTranspiler);
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            finally
            {
                exception = Cleanup(exception, harmony, original);
                if (exception != null)
                    LogError($"Failed to apply reverse patch: {exception.Message}\n{exception.StackTrace}");
            }
        }
    }
}