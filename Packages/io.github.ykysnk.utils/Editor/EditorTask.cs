using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEditor;

namespace io.github.ykysnk.utils.Editor
{
    [PublicAPI]
    public static class EditorTask
    {
        public static Task Run(Task task)
        {
            var tcs = new TaskCompletionSource<bool>();

            EditorApplication.update += Poll;

            return tcs.Task;

            void Poll()
            {
                if (!task.IsCompleted)
                    return;
                EditorApplication.update -= Poll;

                if (task.IsFaulted)
                    tcs.SetException(task.Exception ?? new("Unknown error"));
                else if (task.IsCanceled)
                    tcs.SetCanceled();
                else
                    tcs.SetResult(true);
            }
        }

        public static Task<T> Run<T>(Task<T> task)
        {
            var tcs = new TaskCompletionSource<T>();

            EditorApplication.update += Poll;

            return tcs.Task;

            void Poll()
            {
                if (!task.IsCompleted)
                    return;
                EditorApplication.update -= Poll;

                if (task.IsFaulted)
                    tcs.SetException(task.Exception ?? new("Unknown error"));
                else if (task.IsCanceled)
                    tcs.SetCanceled();
                else
                    tcs.SetResult(task.Result);
            }
        }
    }
}