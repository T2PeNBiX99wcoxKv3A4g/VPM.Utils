using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEditor;

namespace io.github.ykysnk.utils.Editor.Extensions
{
    [PublicAPI]
    public static class TaskExtensions
    {
        public delegate void TaskWait();

        public delegate void TaskWait<in T>(T result);

        public delegate void TaskWaitError(Exception? e);

        public static void WaitEditor(this Task task, TaskWait onCompleted, TaskWaitError? onError = null)
        {
            EditorApplication.update += Waiting;
            return;

            void Waiting()
            {
                if (!task.IsCompleted) return;
                EditorApplication.update -= Waiting;

                if (task.IsFaulted)
                    onError?.Invoke(task.Exception);
                else
                    onCompleted();
            }
        }

        public static void WaitEditor<T>(this Task<T> task, TaskWait<T> onCompleted, TaskWaitError? onError = null)
        {
            EditorApplication.update += Waiting;
            return;

            void Waiting()
            {
                if (!task.IsCompleted) return;
                EditorApplication.update -= Waiting;

                if (task.IsFaulted)
                    onError?.Invoke(task.Exception);
                else
                    onCompleted(task.Result);
            }
        }
    }
}