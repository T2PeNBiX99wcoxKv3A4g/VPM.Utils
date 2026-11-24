using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEditor;

namespace io.github.ykysnk.utils.Editor
{
    /// <summary>
    ///     Adapts a .NET task to the editor loop: the returned task is completed from
    ///     <see cref="EditorApplication.update" />, so awaiting it resumes on the main thread where UnityEditor APIs
    ///     are allowed.
    /// </summary>
    [PublicAPI]
    public static class EditorTask
    {
        /// <summary>
        ///     Returns a task that finishes after the editor loop sees <paramref name="task" /> complete. Fault and
        ///     cancel states are forwarded to the returned task instead of being thrown, so the caller decides how to
        ///     handle them.
        /// </summary>
        /// <param name="task">Task to observe; its result, if it has one, is discarded.</param>
        /// <returns>
        ///     Task that completes on the editor loop with the same fault or cancel state. Await it from the main
        ///     thread, since continuations run there.
        /// </returns>
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

        /// <summary>
        ///     Same as <see cref="Run(Task)" /> but also carries the result of <paramref name="task" /> over to the
        ///     returned task.
        /// </summary>
        /// <param name="task">Task to observe.</param>
        /// <returns>Task that yields <paramref name="task" />'s result on the editor loop.</returns>
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