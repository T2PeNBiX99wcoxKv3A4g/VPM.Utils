using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace io.github.ykysnk.utils.Editor
{
    [PublicAPI]
    public static class EditorUtils
    {
        public static async Task<bool> DisplayDialogAsync(string title, string message,
            string ok = "OK", string cancel = "", int waitTime = 0)
        {
            var tcs = new TaskCompletionSource<bool>();

            CustomDialogWindow.Show(title, message, ok, cancel, waitTime,
                () => tcs.TrySetResult(true),
                () => tcs.TrySetResult(false));

            return await tcs.Task;
        }

        public static void DisplayDialog(string title, string message, string ok = "OK", string cancel = "",
            Action? onOk = null, Action? onCancel = null, int waitTime = 0)
        {
            CustomDialogWindow.Show(title, message, ok, cancel, waitTime,
                () => onOk?.Invoke(),
                () => onCancel?.Invoke());
        }
    }
}