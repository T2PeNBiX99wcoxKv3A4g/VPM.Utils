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
        public static Task<T> AsTask<T>(this T request) where T : Request
        {
            var tcs = new TaskCompletionSource<T>();

            EditorApplication.update += Check;

            return tcs.Task;

            void Check()
            {
                if (!request.IsCompleted) return;
                EditorApplication.update -= Check;
                tcs.SetResult(request);
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
                    tcs.TrySetCanceled();
                    return;
                }

                if (!request.IsCompleted) return;
                EditorApplication.update -= Check;
                tcs.TrySetResult(request);
            }
        }
    }
}