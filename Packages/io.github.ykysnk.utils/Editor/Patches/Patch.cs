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
    /// <summary>
    ///     A group of patches that a <see cref="IPatchLoader" /> applies as a unit. The implementing class is the
    ///     container: its own <see cref="Execute" /> runs first, then every patch method nested inside it.
    /// </summary>
    [PublicAPI]
    public interface IPatch
    {
        /// <summary>Stable identity of the group, used when reporting the patches it applies.</summary>
        string QualifiedName { get; }
        /// <summary>Short name of the group, used as the prefix of every log entry it writes.</summary>
        string DisplayName { get; }
        /// <summary>Whether the group should be applied; a disabled group is skipped without touching Harmony.</summary>
        bool Enabled { get; }
        /// <summary>The Harmony instance that owns this group's patches, assigned by the loader before it runs.</summary>
        Harmony? ThisHarmony { get; internal set; }

        /// <summary>Applies the patches this group declares itself; it runs before the nested patch methods.</summary>
        /// <param name="harmony">The Harmony instance that receives the patches.</param>
        void Execute(Harmony harmony);
        /// <summary>
        ///     Applies the whole group: calls <see cref="Execute" /> and then every nested
        ///     <see cref="PatchMethod{T}" /> and <see cref="ReversePatchMethod{T}" />. Returns immediately when
        ///     <see cref="Enabled" /> is <see langword="false" />.
        /// </summary>
        /// <param name="harmony">The Harmony instance that receives the patches.</param>
        void Run(Harmony harmony);
    }

    /// <summary>
    ///     Base class for a group of patches applied together. A concrete subclass is created once per editor
    ///     session and lists its patches as nested classes deriving from <see cref="PatchMethod{T}" /> or
    ///     <see cref="ReversePatchMethod{T}" />; <see cref="Run" /> applies all of them.
    /// </summary>
    /// <typeparam name="T">
    ///     The concrete patch group; it supplies the shared instance and the type whose nested classes are patched.
    /// </typeparam>
    [PublicAPI]
    public abstract class Patch<T> : IPatch, ILogger where T : Patch<T>, new()
    {
        private static readonly Lazy<T> InstanceInternal = new(() => new(), LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        ///     Lazily created instance, for callers outside the patching pass; the framework creates its own
        ///     instance when it applies the group.
        /// </summary>
        public static T Instance => InstanceInternal.Value;
        /// <summary>The patch group type; its nested types are the patches applied by <see cref="Run" />.</summary>
        public static Type ThisType { get; } = typeof(T);
        /// <summary>The Harmony instance of the owning loader, or <see langword="null" /> before it is assigned.</summary>
        protected Harmony? ThisHarmony { get; set; }

        private static IEnumerable<Type> PatchMethodTypes => ThisType
            .GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic)
            .Where(t => typeof(IPatchMethod).IsAssignableFrom(t));

        private static IEnumerable<Type> ReversePatchMethodTypes => ThisType
            .GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic)
            .Where(t => typeof(IReversePatchMethod).IsAssignableFrom(t));

        private static IEnumerable<IPatchMethod> PatchMethods =>
            PatchMethodTypes.Select(ReflectionUtils.Instantiate<IPatchMethod>).Where(x => x != null)!;

        private static IEnumerable<IReversePatchMethod> ReversePatchMethods =>
            ReversePatchMethodTypes.Select(ReflectionUtils.Instantiate<IReversePatchMethod>).Where(x => x != null)!;

        /// <inheritdoc />
        /// <param name="message">The value to write; it is prefixed with <see cref="DisplayName" />.</param>
        public void Log(object? message) => Utils.Log(DisplayName, message);
        /// <inheritdoc />
        /// <param name="message">The value to write; it is prefixed with <see cref="DisplayName" />.</param>
        /// <param name="context">
        ///     The Unity object the console entry is associated with, so selecting the entry highlights that object
        ///     instead of the calling script.
        /// </param>
        public void Log(object? message, Object context) => Utils.Log(DisplayName, message, context);

        /// <inheritdoc />
        /// <param name="message">The value to write; it is prefixed with <see cref="DisplayName" />.</param>
        public void LogWarning(object? message) => Utils.LogWarning(DisplayName, message);
        /// <inheritdoc />
        /// <param name="message">The value to write; it is prefixed with <see cref="DisplayName" />.</param>
        /// <param name="context">
        ///     The Unity object the console entry is associated with, so selecting the entry highlights that object
        ///     instead of the calling script.
        /// </param>
        public void LogWarning(object? message, Object context) => Utils.LogWarning(DisplayName, message, context);

        /// <inheritdoc />
        /// <param name="message">The value to write; it is prefixed with <see cref="DisplayName" />.</param>
        public void LogError(object? message) => Utils.LogError(DisplayName, message);
        /// <inheritdoc />
        /// <param name="message">The value to write; it is prefixed with <see cref="DisplayName" />.</param>
        /// <param name="context">
        ///     The Unity object the console entry is associated with, so selecting the entry highlights that object
        ///     instead of the calling script.
        /// </param>
        public void LogError(object? message, Object context) => Utils.LogError(DisplayName, message, context);

        /// <inheritdoc />
        /// <param name="condition">The condition that is expected to hold.</param>
        /// <param name="message">
        ///     The text reported when <paramref name="condition" /> is <see langword="false" />; it is prefixed with
        ///     <see cref="DisplayName" />.
        /// </param>
        public void Assert(bool condition, object? message) => Utils.Assert(condition, DisplayName, message);

        /// <inheritdoc />
        /// <param name="condition">The condition that is expected to hold.</param>
        /// <param name="message">
        ///     The text reported when <paramref name="condition" /> is <see langword="false" />; it is prefixed with
        ///     <see cref="DisplayName" />.
        /// </param>
        /// <param name="context">
        ///     The Unity object the console entry is associated with, so selecting the entry highlights that object
        ///     instead of the calling script.
        /// </param>
        public void Assert(bool condition, object? message, Object context) =>
            Utils.Assert(condition, DisplayName, message, context);

        /// <summary>Unique name of the group; it defaults to the full type name, or the short name if there is none.</summary>
        public virtual string QualifiedName { get; } = ThisType.FullName ?? ThisType.Name;
        /// <summary>Short name of the group, used as the log prefix; it defaults to the type name.</summary>
        public virtual string DisplayName { get; } = ThisType.Name;
        /// <summary>Whether the group is applied; it is enabled by default.</summary>
        public virtual bool Enabled { get; } = true;

        /// <inheritdoc />
        Harmony? IPatch.ThisHarmony
        {
            get => ThisHarmony;
            set => ThisHarmony = value;
        }

        /// <inheritdoc />
        void IPatch.Execute(Harmony harmony) => Execute(harmony);
        /// <inheritdoc />
        void IPatch.Run(Harmony harmony) => Run(harmony);
        /// <summary>Logs a message prefixed with the patch class name, for use from static members.</summary>
        /// <param name="message">The value to write; it is prefixed with the patch class name.</param>
        public static void Log2(object? message) => Utils.Log(ThisType.Name, message);
        /// <summary>Logs a message prefixed with the patch class name, for use from static members.</summary>
        /// <param name="message">The value to write; it is prefixed with the patch class name.</param>
        /// <param name="context">
        ///     The Unity object the console entry is associated with, so selecting the entry highlights that object
        ///     instead of the calling script.
        /// </param>
        public static void Log2(object? message, Object context) => Utils.Log(ThisType.Name, message, context);
        /// <summary>Logs a warning prefixed with the patch class name, for use from static members.</summary>
        /// <param name="message">The value to write; it is prefixed with the patch class name.</param>
        public static void LogWarning2(object? message) => Utils.LogWarning(ThisType.Name, message);

        /// <summary>Logs a warning prefixed with the patch class name, for use from static members.</summary>
        /// <param name="message">The value to write; it is prefixed with the patch class name.</param>
        /// <param name="context">
        ///     The Unity object the console entry is associated with, so selecting the entry highlights that object
        ///     instead of the calling script.
        /// </param>
        public static void LogWarning2(object? message, Object context) =>
            Utils.LogWarning(ThisType.Name, message, context);

        /// <summary>Logs an error prefixed with the patch class name, for use from static members.</summary>
        /// <param name="message">The value to write; it is prefixed with the patch class name.</param>
        public static void LogError2(object? message) => Utils.LogError(ThisType.Name, message);
        /// <summary>Logs an error prefixed with the patch class name, for use from static members.</summary>
        /// <param name="message">The value to write; it is prefixed with the patch class name.</param>
        /// <param name="context">
        ///     The Unity object the console entry is associated with, so selecting the entry highlights that object
        ///     instead of the calling script.
        /// </param>
        public static void LogError2(object? message, Object context) => Utils.LogError(ThisType.Name, message, context);

        /// <summary>
        ///     Asserts <paramref name="condition" /> and reports the message with the patch class name as prefix, for
        ///     use from static members.
        /// </summary>
        /// <param name="condition">The condition that is expected to hold.</param>
        /// <param name="message">The text reported when <paramref name="condition" /> is <see langword="false" />.</param>
        public static void Assert2(bool condition, object? message) => Utils.Assert(condition, ThisType.Name, message);

        /// <summary>
        ///     Asserts <paramref name="condition" /> and reports the message with the patch class name as prefix, for
        ///     use from static members.
        /// </summary>
        /// <param name="condition">The condition that is expected to hold.</param>
        /// <param name="message">The text reported when <paramref name="condition" /> is <see langword="false" />.</param>
        /// <param name="context">
        ///     The Unity object the console entry is associated with, so selecting the entry highlights that object
        ///     instead of the calling script.
        /// </param>
        public static void Assert2(bool condition, object? message, Object context) =>
            Utils.Assert(condition, ThisType.Name, message, context);

        /// <summary>Applies the patches this group declares itself; it runs before the nested patch methods.</summary>
        /// <param name="harmony">The Harmony instance that receives the patches.</param>
        protected abstract void Execute(Harmony harmony);

        internal void Run(Harmony harmony)
        {
            if (!Enabled) return;
            Execute(harmony);
            PatchAll(harmony);
        }

        internal void PatchAll(Harmony harmony)
        {
            var patchMethods = PatchMethods.ToArray();
            var reversePatchMethods = ReversePatchMethods.ToArray();

            foreach (var patchMethod in patchMethods)
                patchMethod.Patch(harmony);

            foreach (var reversePatchMethod in reversePatchMethods)
                reversePatchMethod.Patch(harmony);
        }
    }
}
