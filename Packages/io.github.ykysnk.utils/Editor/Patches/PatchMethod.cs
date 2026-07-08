using System;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using HarmonyLib;
using JetBrains.Annotations;
using ILogger = io.github.ykysnk.utils.NonUdon.Logger.ILogger;
using Object = UnityEngine.Object;

namespace io.github.ykysnk.utils.Editor.Patches
{
    [PublicAPI]
    internal interface IPatchMethod
    {
        string QualifiedName { get; }
        string DisplayName { get; }
        bool Enabled { get; }
        MethodInfo? TargetMethod { get; }
        MethodInfo? TargetPrefix { get; }
        MethodInfo? TargetPostfix { get; }
        MethodInfo? TargetTranspiler { get; }
        MethodInfo? TargetFinalizer { get; }

        int Priority { get; }
        string[]? Before { get; }
        string[]? After { get; }
        bool? Debug { get; }

        bool Prepare(MethodInfo? original, Harmony harmony);

        Exception? Cleanup(Exception? exception, Harmony harmony, MethodInfo? original);
    }

    [PublicAPI]
    public abstract class PatchMethod<T> : IPatchMethod, ILogger where T : PatchMethod<T>, new()
    {
        private static readonly Lazy<T> InstanceInternal = new(() => new(), LazyThreadSafetyMode.ExecutionAndPublication);

        public static T Instance => InstanceInternal.Value;
        public static Type ThisType { get; } = typeof(T);

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
        public virtual MethodInfo? TargetPrefix { get; }
        public virtual MethodInfo? TargetPostfix { get; }
        public virtual MethodInfo? TargetTranspiler { get; }
        public virtual MethodInfo? TargetFinalizer { get; }

        public virtual int Priority { get; } = -1;
        public virtual string[]? Before { get; }
        public virtual string[]? After { get; }
        public virtual bool? Debug { get; }

        bool IPatchMethod.Prepare(MethodInfo? original, Harmony harmony) => Prepare(original, harmony);

        Exception? IPatchMethod.Cleanup(Exception? exception, Harmony harmony, MethodInfo? original) =>
            Cleanup(exception, harmony, original);

        protected virtual bool Prepare(MethodInfo? original, Harmony harmony) => true;
        protected virtual Exception? Cleanup(Exception? exception, Harmony harmony, MethodInfo? original) => null;

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

        public void Apply(Harmony harmony)
        {
            var original = TargetMethod;
            var prefix = TargetPrefix;
            var postfix = TargetPostfix;
            var transpiler = TargetTranspiler;
            var finalizer = TargetFinalizer;
            Exception? exception = null;

            try
            {
                if (!Enabled || !Prepare(original, harmony))
                    return;

                if (original == null)
                {
                    LogWarning("Patch is failed! Target method is null.");
                    return;
                }

                harmony.Patch(
                    original,
                    prefix == null ? null : new HarmonyMethod(prefix, Priority, Before, After, Debug),
                    postfix == null ? null : new HarmonyMethod(postfix, Priority, Before, After, Debug),
                    transpiler == null ? null : new HarmonyMethod(transpiler, Priority, Before, After, Debug),
                    finalizer == null ? null : new HarmonyMethod(finalizer, Priority, Before, After, Debug));
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            finally
            {
                exception = Cleanup(exception, harmony, original);
                if (exception != null)
                    ExceptionDispatchInfo.Capture(exception).Throw();
            }
        }
    }
}