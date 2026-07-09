using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using HarmonyLib;
using io.github.ykysnk.utils.NonUdon;
using io.github.ykysnk.utils.NonUdon.Logger;
using JetBrains.Annotations;
using Object = UnityEngine.Object;

namespace io.github.ykysnk.utils.Editor.Patches
{
    [PublicAPI]
    public interface IPatch
    {
        string QualifiedName { get; }
        string DisplayName { get; }
        bool Enabled { get; }
        Harmony? Harmony { get; internal set; }

        void Execute();
        void Run();
    }

    [PublicAPI]
    public abstract class Patch<T> : IPatch, ILogger where T : Patch<T>, new()
    {
        private static readonly Lazy<T> InstanceInternal = new(() => new(), LazyThreadSafetyMode.ExecutionAndPublication);

        public static T Instance => InstanceInternal.Value;
        public static Type ThisType { get; } = typeof(T);
        protected Harmony? Harmony { get; set; }

        private static IEnumerable<Type> PatchMethodTypes => ThisType
            .GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic)
            .Where(t => typeof(IPatchMethod).IsAssignableFrom(t));

        private static IEnumerable<IPatchMethod> PatchMethods =>
            PatchMethodTypes.Select(ReflectionUtils.Instantiate<IPatchMethod>).Where(x => x != null)!;

        private static IEnumerable<IReversePatchMethod> ReversePatchMethods =>
            ReversePatchMethodTypes.Select(ReflectionUtils.Instantiate<IReversePatchMethod>).Where(x => x != null)!;

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

        Harmony? IPatch.Harmony
        {
            get => Harmony;
            set => Harmony = value;
        }

        void IPatch.Execute() => Execute();
        void IPatch.Run() => Run();
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

        internal void Run()
        {
            if (!Enabled) return;
            Execute();
            PatchAll();
        }

        internal void PatchAll()
        {
            var patchMethods = PatchMethods.ToArray();
            if (patchMethods.Length < 1) return;
            foreach (var patchMethod in patchMethods)
                patchMethod.Patch(Harmony);
        }

        }
    }
}