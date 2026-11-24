using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.PackageManager.Requests;

namespace io.github.ykysnk.utils.Editor.Extensions
{
    [PublicAPI]
    public static class RequestExtensions
    {
        /// <summary>
        ///     Awaits a package manager request. The task is completed from <see cref="EditorApplication.delayCall" />
        ///     so the awaiting continuation always resumes on the main thread with a shallow call stack, which is what
        ///     lets it call UnityEditor APIs safely.
        /// </summary>
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
