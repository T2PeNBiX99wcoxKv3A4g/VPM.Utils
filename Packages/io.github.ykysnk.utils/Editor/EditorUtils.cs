using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace io.github.ykysnk.utils.Editor
{
    [PublicAPI]
    public static class EditorUtils
    {
        public static async Task<bool> DisplayDialogAsync(string title, string message,
            string ok = "OK", string cancel = "")
        {
            var tcs = new TaskCompletionSource<bool>();

            CustomDialogWindow.Show(title, message, ok, cancel,
                () => tcs.TrySetResult(true),
                () => tcs.TrySetResult(false));

            return await tcs.Task;
        }

        public static void DisplayDialog(string title, string message, string ok = "OK", string cancel = "",
            Action? onOk = null, Action? onCancel = null)
        {
            CustomDialogWindow.Show(title, message, ok, cancel,
                () => onOk?.Invoke(),
                () => onCancel?.Invoke());
        }
    }
}