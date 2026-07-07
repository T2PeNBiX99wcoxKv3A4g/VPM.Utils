using System;
using System.Threading;
using io.github.ykysnk.utils.NonUdon.Logger;
using JetBrains.Annotations;
using Object = UnityEngine.Object;

namespace io.github.ykysnk.utils.Editor.Patches
{
    [PublicAPI]
    internal interface IPatch
    {
        string QualifiedName { get; }
        string DisplayName { get; }
        bool Enabled { get; }

        void Execute();
    }

    [PublicAPI]
    public abstract class Patch<T> : IPatch, ILogger where T : Patch<T>, new()
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

        void IPatch.Execute() => Execute();
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

        protected abstract void Execute();

        public void Run()
        {
            if (!Enabled) return;
            Execute();
        }
    }
}