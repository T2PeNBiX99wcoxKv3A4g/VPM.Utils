using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.PackageManager.Requests;

namespace io.github.ykysnk.utils.Editor.Extensions
{
    /// <summary>
    ///     Turns package manager requests into tasks that complete on the editor loop, so awaiting code can stay on the
    ///     main thread.
    /// </summary>
    [PublicAPI]
    public static class RequestExtensions
    {
        /// <summary>
        ///     Awaits a package manager request. The request is polled from <see cref="EditorApplication.update" /> and
        ///     the task is completed later from <see cref="EditorApplication.delayCall" />, so the awaiting continuation
        ///     resumes on the main thread with a shallow call stack, which is what lets it call UnityEditor APIs safely.
        /// </summary>
        /// <param name="request">Request returned by the package manager client; only its completion is observed.</param>
        /// <typeparam name="T">Concrete request type, preserved so the caller can read its <c>Result</c>.</typeparam>
        /// <returns>Task that resolves to the finished request on the main thread.</returns>
        public static Task<T> AsTask<T>(this T request) where T : Request
        {
            var tcs = new TaskCompletionSource<T>();

            EditorApplication.update += Check;

            return tcs.Task;

            void Check()
            {
                if (!request.IsCompleted) return;
                EditorApplication.update -= Check;
                Complete(tcs, request);
            }
        }

        /// <summary>
        ///     Same as <see cref="AsTask{T}(T)" />, but stops waiting when the token is cancelled. The request itself is
        ///     not cancelled, it just stops being observed.
        /// </summary>
        /// <param name="request">Request returned by the package manager client.</param>
        /// <param name="token">Cancels the wait; the returned task is then cancelled with this token.</param>
        /// <typeparam name="T">Concrete request type, preserved so the caller can read its <c>Result</c>.</typeparam>
        /// <returns>Task that resolves to the finished request, or is cancelled with <paramref name="token" />.</returns>
        public static Task<T> AsTask<T>(this T request, CancellationToken token) where T : Request
        {
            var tcs = new TaskCompletionSource<T>();

            EditorApplication.update += Check;

            return tcs.Task;

            void Check()
            {
                if (token.IsCancellationRequested)
                {
                    EditorApplication.update -= Check;
                    tcs.TrySetCanceled(token);
                    return;
                }

                if (!request.IsCompleted) return;
                EditorApplication.update -= Check;
                Complete(tcs, request);
            }
        }

        private static void Complete<T>(TaskCompletionSource<T> tcs, T request) where T : Request =>
            EditorApplication.delayCall += () => tcs.TrySetResult(request);
    }
}
