using System;
using System.Collections.Generic;
using System.Linq;
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
    ///     One reverse patch: a stub method that receives a callable copy of another method's body, optionally run
    ///     through a transpiler. Instances are the classes nested inside a <see cref="Patch{T}" /> group.
    /// </summary>
    [PublicAPI]
    public interface IReversePatchMethod
    {
        /// <summary>Unique name of the reverse patch, used for logging and error reporting.</summary>
        string QualifiedName { get; }
        /// <summary>Short name of the reverse patch, used as the prefix of every log entry it writes.</summary>
        string DisplayName { get; }
        /// <summary>Whether the reverse patch is applied; a disabled one is skipped without touching Harmony.</summary>
        bool Enabled { get; }
        /// <summary>
        ///     The method whose body is copied. When it is <see langword="null" /> nothing is applied and a warning is
        ///     logged.
        /// </summary>
        MethodInfo? TargetMethod { get; }
        /// <summary>
        ///     The stub method that receives the copied body. When it is <see langword="null" /> nothing is applied and
        ///     a warning is logged.
        /// </summary>
        MethodInfo? TargetReverse { get; }
        /// <summary>
        ///     The transpiler applied to the copied body, or <see langword="null" /> to fall back to the transpiler
        ///     declared next to the stub.
        /// </summary>
        MethodInfo? TargetTranspiler { get; }

        /// <summary>
        ///     Harmony priority given to this reverse patch; the default -1 means no priority is set explicitly.
        /// </summary>
        int Priority { get; }
        /// <summary>Harmony ids this reverse patch must run before, or <see langword="null" /> for no ordering constraint.</summary>
        string[]? Before { get; }
        /// <summary>Harmony ids this reverse patch must run after, or <see langword="null" /> for no ordering constraint.</summary>
        string[]? After { get; }
        /// <summary>Harmony debug flag of the reverse patch, or <see langword="null" /> to leave it unset.</summary>
        bool? Debug { get; }

        /// <summary>Runs just before the reverse patch is applied; whatever it sets up is kept for the lifetime of the stub.</summary>
        /// <param name="original">The method whose body is going to be copied, already resolved and non-null.</param>
        /// <param name="harmony">The Harmony instance that owns the patches of the group this reverse patch belongs to.</param>
        /// <returns><see langword="false" /> to skip the reverse patch, in which case nothing is applied or cleaned up.</returns>
        bool Prepare(MethodInfo? original, Harmony harmony);

        /// <summary>Runs in the <c>finally</c> block of the reverse patch attempt, whether or not it succeeded.</summary>
        /// <param name="exception">The exception the attempt threw, or <see langword="null" /> on success.</param>
        /// <param name="harmony">The Harmony instance passed to <see cref="Prepare" />.</param>
        /// <param name="original">The method whose body was copied.</param>
        /// <returns>
        ///     The exception to report; the base implementation passes <paramref name="exception" /> through and a
        ///     non-null result is logged as an error.
        /// </returns>
        Exception? Cleanup(Exception? exception, Harmony harmony, MethodInfo? original);
        /// <summary>Applies the reverse patch, including <see cref="Prepare" /> and <see cref="Cleanup" />.</summary>
        /// <param name="harmony">The Harmony instance that owns the patches of the owning group.</param>
        void Patch(Harmony harmony);
    }

    /// <summary>
    ///     Base class for one reverse patch. A concrete nested class names the copied method with
    ///     <see cref="TargetMethod" /> and declares the stub that receives it as <c>Reverse</c>, plus an optional
    ///     <c>Transpiler</c>.
    /// </summary>
    /// <typeparam name="T">
    ///     The concrete reverse patch class, used for the shared instance and for method lookup.
    /// </typeparam>
    [PublicAPI]
    public abstract class ReversePatchMethod<T> : IReversePatchMethod, ILogger where T : ReversePatchMethod<T>, new()
    {
        private static readonly Lazy<T> InstanceInternal = new(() => new(), LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        ///     Lazily created instance, for callers outside the patching pass; the framework instantiates the reverse
        ///     patch itself.
        /// </summary>
        public static T Instance => InstanceInternal.Value;
        /// <summary>The reverse patch type; it is the type whose methods are reflected and it names the log prefix.</summary>
        public static Type ThisType { get; } = typeof(T);
        /// <summary>Name of the nested stub method that receives the copied body; override it to use a different name.</summary>
        public virtual string ReverseMethod { get; } = "Reverse";
        /// <summary>Name of the nested method used as the transpiler; override it to use a different name.</summary>
        public virtual string TranspilerMethod { get; } = "Transpiler";
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
        ///     Unique name of the reverse patch; it defaults to the full type name, or the short name if there is none.
        /// </summary>
        public virtual string QualifiedName { get; } = ThisType.FullName ?? ThisType.Name;
        /// <summary>Short name of the reverse patch, used as the log prefix; it defaults to the type name.</summary>
        public virtual string DisplayName { get; } = ThisType.Name;
        /// <summary>Whether the reverse patch is applied; it is enabled by default.</summary>
        public virtual bool Enabled { get; } = true;
        /// <summary>
        ///     The method whose body is copied. The base value is <see langword="null" />, so every concrete reverse
        ///     patch class must override it.
        /// </summary>
        public virtual MethodInfo? TargetMethod { get; }
        /// <summary>The nested method named <see cref="ReverseMethod" />, or <see langword="null" /> when it does not exist.</summary>
        public virtual MethodInfo? TargetReverse => Method(ReverseMethod);
        /// <summary>The nested method named <see cref="TranspilerMethod" />, or <see langword="null" /> when it does not exist.</summary>
        public virtual MethodInfo? TargetTranspiler => Method(TranspilerMethod);
        /// <inheritdoc />
        public virtual int Priority { get; } = -1;
        /// <inheritdoc />
        public virtual string[]? Before { get; }
        /// <inheritdoc />
        public virtual string[]? After { get; }
        /// <inheritdoc />
        public virtual bool? Debug { get; }

        /// <inheritdoc />
        bool IReversePatchMethod.Prepare(MethodInfo? original, Harmony harmony) => Prepare(original, harmony);

        /// <inheritdoc />
        Exception? IReversePatchMethod.Cleanup(Exception? exception, Harmony harmony, MethodInfo? original) =>
            Cleanup(exception, harmony, original);

        /// <inheritdoc />
        void IReversePatchMethod.Patch(Harmony harmony) => Patch(harmony);
        /// <summary>Runs just before the reverse patch is applied, for state the stub needs while it runs.</summary>
        /// <param name="original">The method whose body is going to be copied, already resolved and non-null.</param>
        /// <param name="harmony">The Harmony instance that owns the patches of the owning group.</param>
        /// <returns><see langword="true" /> to continue; <see langword="false" /> to skip the reverse patch.</returns>
        protected virtual bool Prepare(MethodInfo? original, Harmony harmony) => true;

        /// <summary>Runs in the <c>finally</c> block of the reverse patch attempt, whether or not it succeeded.</summary>
        /// <param name="exception">The exception the attempt threw, or <see langword="null" /> on success.</param>
        /// <param name="harmony">The Harmony instance passed to <see cref="Prepare" />.</param>
        /// <param name="original">The method whose body was copied.</param>
        /// <returns>The exception to report; the base implementation passes it through unchanged.</returns>
        protected virtual Exception? Cleanup(Exception? exception, Harmony harmony, MethodInfo? original) => exception;

        /// <summary>Logs a message prefixed with the reverse patch class name, for use from static members.</summary>
        /// <param name="message">The value to write; it is prefixed with the reverse patch class name.</param>
        public static void Log2(object? message) => Utils.Log(ThisType.Name, message);
        /// <summary>Logs a message prefixed with the reverse patch class name, for use from static members.</summary>
        /// <param name="message">The value to write; it is prefixed with the reverse patch class name.</param>
        /// <param name="context">
        ///     The Unity object the console entry is associated with, so selecting the entry highlights that object
        ///     instead of the calling script.
        /// </param>
        public static void Log2(object? message, Object context) => Utils.Log(ThisType.Name, message, context);
        /// <summary>Logs a warning prefixed with the reverse patch class name, for use from static members.</summary>
        /// <param name="message">The value to write; it is prefixed with the reverse patch class name.</param>
        public static void LogWarning2(object? message) => Utils.LogWarning(ThisType.Name, message);

        /// <summary>Logs a warning prefixed with the reverse patch class name, for use from static members.</summary>
        /// <param name="message">The value to write; it is prefixed with the reverse patch class name.</param>
        /// <param name="context">
        ///     The Unity object the console entry is associated with, so selecting the entry highlights that object
        ///     instead of the calling script.
        /// </param>
        public static void LogWarning2(object? message, Object context) =>
            Utils.LogWarning(ThisType.Name, message, context);

        /// <summary>Logs an error prefixed with the reverse patch class name, for use from static members.</summary>
        /// <param name="message">The value to write; it is prefixed with the reverse patch class name.</param>
        public static void LogError2(object? message) => Utils.LogError(ThisType.Name, message);
        /// <summary>Logs an error prefixed with the reverse patch class name, for use from static members.</summary>
        /// <param name="message">The value to write; it is prefixed with the reverse patch class name.</param>
        /// <param name="context">
        ///     The Unity object the console entry is associated with, so selecting the entry highlights that object
        ///     instead of the calling script.
        /// </param>
        public static void LogError2(object? message, Object context) => Utils.LogError(ThisType.Name, message, context);

        /// <summary>
        ///     Asserts <paramref name="condition" /> and reports the message with the reverse patch class name as
        ///     prefix, for use from static members.
        /// </summary>
        /// <param name="condition">The condition that is expected to hold.</param>
        /// <param name="message">The text reported when <paramref name="condition" /> is <see langword="false" />.</param>
        public static void Assert2(bool condition, object? message) => Utils.Assert(condition, ThisType.Name, message);

        /// <summary>
        ///     Asserts <paramref name="condition" /> and reports the message with the reverse patch class name as
        ///     prefix, for use from static members.
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
        ///     Resolves a method of the reverse patch class by name. Results are cached in <see cref="PatchCore" />,
        ///     and names that do not resolve are remembered so the failed reflection lookup happens only once.
        /// </summary>
        /// <param name="name">The method name to look for on the reverse patch class; matching is by name only.</param>
        /// <returns>The method, or <see langword="null" /> when the class has no such method.</returns>
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

        // A stub may declare its transpiler as a local function; the compiler emits that as a generated method named
        // after the stub and returning the instruction stream, so it is found by name instead of by declaration.
        internal static MethodInfo? GetTranspiler(MethodInfo method)
        {
            var methodName = method.Name;
            var declaredMethods = ReflectionUtils.GetDeclaredMethods(method.DeclaringType);
            var ici = typeof(IEnumerable<CodeInstruction>);
            var predicate = (Func<MethodInfo, bool>)(m => !(m.ReturnType != ici) && m.Name.StartsWith($"<{methodName}>"));
            return declaredMethods.FirstOrDefault(predicate);
        }

        internal void Patch(Harmony harmony)
        {
            if (!Enabled) return;

            var reverse = TargetReverse;
            if (reverse == null)
            {
                LogWarning("Failed to apply reverse patch: reverse method not found.");
                return;
            }

            var original = TargetMethod;
            if (original == null)
            {
                LogWarning("Failed to apply reverse patch: target method not found.");
                return;
            }

            if (!Prepare(original, harmony)) return;

            var reverseTranspiler = TargetTranspiler;
            Exception? exception = null;

            try
            {
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
