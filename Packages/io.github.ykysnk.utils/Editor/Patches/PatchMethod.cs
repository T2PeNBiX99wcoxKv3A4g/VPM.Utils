using System;
using System.Reflection;
using System.Threading;
using HarmonyLib;
using JetBrains.Annotations;
using io.github.ykysnk.utils.NonUdon;
using ILogger = io.github.ykysnk.utils.NonUdon.Logger.ILogger;
using Object = UnityEngine.Object;

namespace io.github.ykysnk.utils.Editor.Patches
{
    /// <summary>
    ///     One Harmony patch set: a target method plus the prefix, postfix, transpiler and finalizer that belong to
    ///     it. Instances are the classes nested inside a <see cref="Patch{T}" /> group.
    /// </summary>
    [PublicAPI]
    public interface IPatchMethod
    {
        /// <summary>Unique name of the patch, used for logging and error reporting.</summary>
        string QualifiedName { get; }
        /// <summary>Short name of the patch, used as the prefix of every log entry it writes.</summary>
        string DisplayName { get; }
        /// <summary>Whether the patch is applied; a disabled patch is skipped without touching Harmony.</summary>
        bool Enabled { get; }
        /// <summary>The method to patch. When it is <see langword="null" /> nothing is applied and a warning is logged.</summary>
        MethodInfo? TargetMethod { get; }
        /// <summary>The prefix method of this patch, or <see langword="null" /> when the nested class has none.</summary>
        MethodInfo? TargetPrefix { get; }
        /// <summary>The postfix method of this patch, or <see langword="null" /> when the nested class has none.</summary>
        MethodInfo? TargetPostfix { get; }
        /// <summary>The transpiler method of this patch, or <see langword="null" /> when the nested class has none.</summary>
        MethodInfo? TargetTranspiler { get; }
        /// <summary>The finalizer method of this patch, or <see langword="null" /> when the nested class has none.</summary>
        MethodInfo? TargetFinalizer { get; }

        /// <summary>
        ///     Harmony priority given to every patch method of this set; the default -1 means no priority is set
        ///     explicitly.
        /// </summary>
        int Priority { get; }
        /// <summary>Harmony ids this patch must run before, or <see langword="null" /> for no ordering constraint.</summary>
        string[]? Before { get; }
        /// <summary>Harmony ids this patch must run after, or <see langword="null" /> for no ordering constraint.</summary>
        string[]? After { get; }
        /// <summary>Harmony debug flag of the patch methods, or <see langword="null" /> to leave it unset.</summary>
        bool? Debug { get; }

        /// <summary>Runs just before the patch is applied; whatever it sets up is kept for the lifetime of the patch.</summary>
        /// <param name="original">The method about to be patched, already resolved and non-null.</param>
        /// <param name="harmony">The Harmony instance that is going to receive the patch.</param>
        /// <returns><see langword="false" /> to skip patching this time, in which case nothing is applied or cleaned up.</returns>
        bool Prepare(MethodInfo? original, Harmony harmony);

        /// <summary>Runs in the <c>finally</c> block of the patch attempt, whether or not patching succeeded.</summary>
        /// <param name="exception">The exception the patch attempt threw, or <see langword="null" /> on success.</param>
        /// <param name="harmony">The Harmony instance the patch was applied to.</param>
        /// <param name="original">The method that was patched.</param>
        /// <returns>
        ///     The exception to report; the base implementation passes <paramref name="exception" /> through and a
        ///     non-null result is logged as an error.
        /// </returns>
        Exception? Cleanup(Exception? exception, Harmony harmony, MethodInfo? original);
        /// <summary>Applies the patch, including <see cref="Prepare" /> and <see cref="Cleanup" />.</summary>
        /// <param name="harmony">The Harmony instance that receives the patch.</param>
        void Patch(Harmony harmony);
    }

    /// <summary>
    ///     Base class for one Harmony patch set. A concrete nested class names its target with
    ///     <see cref="TargetMethod" /> and exposes its patch methods under the conventional names
    ///     <c>Prefix</c>, <c>Postfix</c>, <c>Transpiler</c> and <c>Finalizer</c>.
    /// </summary>
    /// <typeparam name="T">The concrete patch class, used for the shared instance and for method lookup.</typeparam>
    [PublicAPI]
    public abstract class PatchMethod<T> : IPatchMethod, ILogger where T : PatchMethod<T>, new()
    {
        private static readonly Lazy<T> InstanceInternal = new(() => new(), LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        ///     Lazily created instance, for callers outside the patching pass; the framework instantiates the patch
        ///     set itself.
        /// </summary>
        public static T Instance => InstanceInternal.Value;
        /// <summary>The patch type; it is the type whose methods are reflected and it names the log prefix.</summary>
        public static Type ThisType { get; } = typeof(T);
        /// <summary>Name of the nested method used as the prefix; override it to use a different name.</summary>
        public virtual string PrefixMethod { get; } = "Prefix";
        /// <summary>Name of the nested method used as the postfix; override it to use a different name.</summary>
        public virtual string PostfixMethod { get; } = "Postfix";
        /// <summary>Name of the nested method used as the transpiler; override it to use a different name.</summary>
        public virtual string TranspilerMethod { get; } = "Transpiler";
        /// <summary>Name of the nested method used as the finalizer; override it to use a different name.</summary>
        public virtual string FinalizerMethod { get; } = "Finalizer";

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

        /// <summary>Unique name of the patch; it defaults to the full type name, or the short name if there is none.</summary>
        public virtual string QualifiedName { get; } = ThisType.FullName ?? ThisType.Name;
        /// <summary>Short name of the patch, used as the log prefix; it defaults to the type name.</summary>
        public virtual string DisplayName { get; } = ThisType.Name;
        /// <summary>Whether the patch is applied; it is enabled by default.</summary>
        public virtual bool Enabled { get; } = true;
        /// <summary>
        ///     The method to patch. The base value is <see langword="null" />, so every concrete patch class must
        ///     override it, usually by resolving the method with <see cref="Method" />.
        /// </summary>
        public virtual MethodInfo? TargetMethod { get; }
        /// <summary>The nested method named <see cref="PrefixMethod" />, or <see langword="null" /> when it does not exist.</summary>
        public virtual MethodInfo? TargetPrefix => Method(PrefixMethod);
        /// <summary>The nested method named <see cref="PostfixMethod" />, or <see langword="null" /> when it does not exist.</summary>
        public virtual MethodInfo? TargetPostfix => Method(PostfixMethod);
        /// <summary>The nested method named <see cref="TranspilerMethod" />, or <see langword="null" /> when it does not exist.</summary>
        public virtual MethodInfo? TargetTranspiler => Method(TranspilerMethod);
        /// <summary>The nested method named <see cref="FinalizerMethod" />, or <see langword="null" /> when it does not exist.</summary>
        public virtual MethodInfo? TargetFinalizer => Method(FinalizerMethod);

        /// <inheritdoc />
        public virtual int Priority { get; } = -1;
        /// <inheritdoc />
        public virtual string[]? Before { get; }
        /// <inheritdoc />
        public virtual string[]? After { get; }
        /// <inheritdoc />
        public virtual bool? Debug { get; }

        /// <inheritdoc />
        bool IPatchMethod.Prepare(MethodInfo? original, Harmony harmony) => Prepare(original, harmony);

        /// <inheritdoc />
        Exception? IPatchMethod.Cleanup(Exception? exception, Harmony harmony, MethodInfo? original) =>
            Cleanup(exception, harmony, original);

        /// <inheritdoc />
        void IPatchMethod.Patch(Harmony harmony) => Patch(harmony);

        /// <summary>Runs just before the patch is applied, for state the patch methods need while they run.</summary>
        /// <param name="original">The method about to be patched, already resolved and non-null.</param>
        /// <param name="harmony">The Harmony instance that is going to receive the patch.</param>
        /// <returns><see langword="true" /> to continue patching; <see langword="false" /> to skip it.</returns>
        protected virtual bool Prepare(MethodInfo? original, Harmony harmony) => true;
        /// <summary>Runs in the <c>finally</c> block of the patch attempt, whether or not patching succeeded.</summary>
        /// <param name="exception">The exception the patch attempt threw, or <see langword="null" /> on success.</param>
        /// <param name="harmony">The Harmony instance the patch was applied to.</param>
        /// <param name="original">The method that was patched.</param>
        /// <returns>The exception to report; the base implementation passes it through unchanged.</returns>
        protected virtual Exception? Cleanup(Exception? exception, Harmony harmony, MethodInfo? original) => exception;

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

        /// <summary>
        ///     Resolves a method of the patch class by name. Results are cached in <see cref="PatchCore" />, and names
        ///     that do not resolve are remembered so the failed reflection lookup happens only once.
        /// </summary>
        /// <param name="name">The method name to look for on the patch class; matching is by name only.</param>
        /// <returns>The method, or <see langword="null" /> when the patch class has no such method.</returns>
        protected static MethodInfo? Method(string name)
        {
            var key = $"{ThisType.FullName}.{name}";
            if (PatchCore.MethodCache.TryGetValue(key, out var cache))
                return cache;
            if (PatchCore.MissingMethods.Contains(key))
                return null;

            var method = ReflectionUtils.FindMethod(ThisType, name);
            if (method == null)
            {
                PatchCore.MissingMethods.Add(key);
                return null;
            }

            return PatchCore.MethodCache[key] = method;
        }

        internal void Patch(Harmony harmony)
        {
            if (!Enabled) return;

            var original = TargetMethod;
            if (original == null)
            {
                LogWarning("Failed to apply patch: target method not found.");
                return;
            }

            var prefix = TargetPrefix;
            var postfix = TargetPostfix;
            var transpiler = TargetTranspiler;
            var finalizer = TargetFinalizer;

            if (prefix == null && postfix == null && transpiler == null && finalizer == null)
            {
                LogWarning("Failed to apply patch: no patch methods specified.");
                return;
            }

            if (!Prepare(original, harmony)) return;

            Exception? exception = null;

            try
            {
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
                    LogError($"Failed to apply patch: {exception.Message}\n{exception.StackTrace}");
            }
        }
    }
}
