using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEditor;

namespace io.github.ykysnk.utils.Editor.Extensions
{
    /// <summary>
    ///     Polls a task from <see cref="EditorApplication.update" /> and reports the outcome through callbacks, for
    ///     callers that cannot await, such as inspector and menu code.
    /// </summary>
    [PublicAPI]
    public static class TaskExtensions
    {
        /// <summary>
        ///     Invokes <paramref name="onCompleted" /> on the editor loop once the task finishes. A faulted task calls
        ///     <paramref name="onError" /> with the inner exception, a canceled one with a
        ///     <see cref="TaskCanceledException" />.
        /// </summary>
        /// <param name="task">Task to observe.</param>
        /// <param name="onCompleted">Invoked on the editor loop when the task succeeds.</param>
        /// <param name="onError">
        ///     Invoked with the failure on the editor loop; <c>null</c> makes the failure silent.
        /// </param>
        public static void WaitEditor(this Task task, Action onCompleted, Action<Exception>? onError = null)
        {
            EditorApplication.update += Waiting;
            return;

            void Waiting()
            {
                if (!task.IsCompleted) return;
                EditorApplication.update -= Waiting;

                if (task.IsCanceled)
                    onError?.Invoke(new TaskCanceledException(task));
                else if (task.IsFaulted)
                    InvokeError(task, onError);
                else
                    onCompleted();
            }
        }

        /// <summary>
        ///     Same as the non generic overload, but passes the task's result to <paramref name="onCompleted" />.
        /// </summary>
        /// <param name="task">Task to observe.</param>
        /// <param name="onCompleted">Invoked with the result on the editor loop when the task succeeds.</param>
        /// <param name="onError">
        ///     Invoked with the failure on the editor loop; <c>null</c> makes the failure silent.
        /// </param>
        /// <typeparam name="T">Result type of the observed task.</typeparam>
        public static void WaitEditor<T>(this Task<T> task, Action<T> onCompleted, Action<Exception>? onError = null)
        {
            EditorApplication.update += Waiting;
            return;

            void Waiting()
            {
                if (!task.IsCompleted) return;
                EditorApplication.update -= Waiting;

                if (task.IsCanceled)
                    onError?.Invoke(new TaskCanceledException(task));
                else if (task.IsFaulted)
                    InvokeError(task, onError);
                else
                    onCompleted(task.Result);
            }
        }

        // Task.Exception is an AggregateException; callers expect the exception the task actually threw.
        private static void InvokeError(Task task, Action<Exception>? onError)
        {
            if (onError == null) return;

            var exception = task.Exception;
            if (exception == null) return;

            onError(exception.InnerException ?? exception);
        }
    }
}
