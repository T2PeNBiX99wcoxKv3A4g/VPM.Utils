using System;
using System.Threading;
using HarmonyLib;
using io.github.ykysnk.utils.NonUdon.Logger;
using JetBrains.Annotations;
using Object = UnityEngine.Object;

namespace io.github.ykysnk.utils.Editor.Patches
{
    /// <summary>
    ///     Entry point of one assembly: receives the Harmony instance created for that assembly and applies its
    ///     <see cref="IPatch" /> groups. <see cref="ExportsPatchLoader" /> names the implementations to load.
    /// </summary>
    [PublicAPI]
    public interface IPatchLoader
    {
        /// <summary>
        ///     Unique name of the loader; it becomes the Harmony id of its instance, so it is also the key used to
        ///     remove its patches again.
        /// </summary>
        string QualifiedName { get; }
        /// <summary>Short name of the loader, used in log entries and error reports.</summary>
        string DisplayName { get; }
        /// <summary>Whether the loader runs at all; a disabled loader is never loaded and never unloaded.</summary>
        bool Enabled { get; }
        /// <summary>The Harmony instance created for this loader, assigned before <see cref="Load" /> is called.</summary>
        Harmony? ThisHarmony { get; internal set; }

        /// <summary>Applies every patch of the assembly; this is where its patch groups are run.</summary>
        /// <param name="harmony">The Harmony instance created for this loader; it owns every patch applied here.</param>
        void Load(Harmony harmony);
        /// <summary>
        ///     Called right before the assemblies are reloaded, while patches still exist; use it to release state
        ///     owned by the loader.
        /// </summary>
        /// <param name="harmony">
        ///     The same Harmony instance that was passed to <see cref="Load" />; its patches are removed by the
        ///     framework after this call.
        /// </param>
        void Unload(Harmony harmony);
    }

    /// <summary>
    ///     Base class for the patch loader of an assembly. It is instantiated once, given a Harmony instance and
    ///     asked to <see cref="Load" />, which normally runs the assembly's <see cref="IPatch" /> groups through
    ///     <see cref="Run" />.
    /// </summary>
    /// <typeparam name="T">The concrete loader; it supplies the shared instance and its type name.</typeparam>
    [PublicAPI]
    public abstract class PatchLoader<T> : IPatchLoader, ILogger where T : PatchLoader<T>, new()
    {
        private static readonly Lazy<T> InstanceInternal = new(() => new(), LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        ///     Lazily created instance, for callers outside the patching pass; the framework creates its own
        ///     instance when it loads the assembly.
        /// </summary>
        public static T Instance => InstanceInternal.Value;
        /// <summary>The loader type, used for the log prefix and as the default name of the loader.</summary>
        public static Type ThisType { get; } = typeof(T);
        /// <summary>The Harmony instance of this loader, or <see langword="null" /> before the framework assigns it.</summary>
        protected Harmony? ThisHarmony { get; set; }

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

        /// <summary>
        ///     Unique name of the loader; it defaults to the full type name, or the short name if there is none, and
        ///     is used as the Harmony id.
        /// </summary>
        public virtual string QualifiedName { get; } = ThisType.FullName ?? ThisType.Name;
        /// <summary>Short name of the loader, used in log entries; it defaults to the type name.</summary>
        public virtual string DisplayName { get; } = ThisType.Name;
        /// <summary>Whether the loader runs; it is enabled by default.</summary>
        public virtual bool Enabled { get; } = true;

        /// <inheritdoc />
        Harmony? IPatchLoader.ThisHarmony
        {
            get => ThisHarmony;
            set => ThisHarmony = value;
        }

        /// <inheritdoc />
        void IPatchLoader.Load(Harmony harmony) => Load(harmony);
        /// <inheritdoc />
        void IPatchLoader.Unload(Harmony harmony) => Unload(harmony);

        /// <summary>Logs a message prefixed with the loader class name, for use from static members.</summary>
        /// <param name="message">The value to write; it is prefixed with the loader class name.</param>
        public static void Log2(object? message) => Utils.Log(ThisType.Name, message);
        /// <summary>Logs a message prefixed with the loader class name, for use from static members.</summary>
        /// <param name="message">The value to write; it is prefixed with the loader class name.</param>
        /// <param name="context">
        ///     The Unity object the console entry is associated with, so selecting the entry highlights that object
        ///     instead of the calling script.
        /// </param>
        public static void Log2(object? message, Object context) => Utils.Log(ThisType.Name, message, context);
        /// <summary>Logs a warning prefixed with the loader class name, for use from static members.</summary>
        /// <param name="message">The value to write; it is prefixed with the loader class name.</param>
        public static void LogWarning2(object? message) => Utils.LogWarning(ThisType.Name, message);

        /// <summary>Logs a warning prefixed with the loader class name, for use from static members.</summary>
        /// <param name="message">The value to write; it is prefixed with the loader class name.</param>
        /// <param name="context">
        ///     The Unity object the console entry is associated with, so selecting the entry highlights that object
        ///     instead of the calling script.
        /// </param>
        public static void LogWarning2(object? message, Object context) =>
            Utils.LogWarning(ThisType.Name, message, context);

        /// <summary>Logs an error prefixed with the loader class name, for use from static members.</summary>
        /// <param name="message">The value to write; it is prefixed with the loader class name.</param>
        public static void LogError2(object? message) => Utils.LogError(ThisType.Name, message);
        /// <summary>Logs an error prefixed with the loader class name, for use from static members.</summary>
        /// <param name="message">The value to write; it is prefixed with the loader class name.</param>
        /// <param name="context">
        ///     The Unity object the console entry is associated with, so selecting the entry highlights that object
        ///     instead of the calling script.
        /// </param>
        public static void LogError2(object? message, Object context) => Utils.LogError(ThisType.Name, message, context);

        /// <summary>
        ///     Asserts <paramref name="condition" /> and reports the message with the loader class name as prefix, for
        ///     use from static members.
        /// </summary>
        /// <param name="condition">The condition that is expected to hold.</param>
        /// <param name="message">The text reported when <paramref name="condition" /> is <see langword="false" />.</param>
        public static void Assert2(bool condition, object? message) => Utils.Assert(condition, ThisType.Name, message);

        /// <summary>
        ///     Asserts <paramref name="condition" /> and reports the message with the loader class name as prefix, for
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

        /// <summary>Applies every patch of the assembly through <see cref="Run" />.</summary>
        /// <param name="harmony">The Harmony instance created for this loader; it owns every patch applied here.</param>
        public abstract void Load(Harmony harmony);

        /// <summary>
        ///     Hook called before the assemblies are reloaded. The base implementation does nothing because the
        ///     framework removes the Harmony patches itself.
        /// </summary>
        /// <param name="harmony">
        ///     The same Harmony instance that was passed to <see cref="Load" />; its patches are removed after this call.
        /// </param>
        public virtual void Unload(Harmony harmony)
        {
        }

        /// <summary>
        ///     Runs a patch group with this loader's Harmony instance, so the group does not have to hold the
        ///     instance itself.
        /// </summary>
        /// <param name="patch">The group to run; its <see cref="IPatch.ThisHarmony" /> is overwritten here.</param>
        /// <exception cref="InvalidOperationException">
        ///     The loader has no Harmony instance yet, i.e. it was used before <c>PatchCore</c> loaded it.
        /// </exception>
        protected void Run(IPatch patch)
        {
            var harmony = ThisHarmony ?? throw new InvalidOperationException(
                $"Patch loader '{DisplayName}' was used before PatchCore assigned its Harmony instance.");

            patch.ThisHarmony = harmony;
            patch.Run(harmony);
        }
    }
}
