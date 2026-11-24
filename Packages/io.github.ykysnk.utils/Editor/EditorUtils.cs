using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace io.github.ykysnk.utils.Editor
{
    /// <summary>
    ///     Message dialog helpers that wrap <see cref="CustomDialogWindow" />.
    /// </summary>
    [PublicAPI]
    public static class EditorUtils
    {
        /// <summary>
        ///     Shows the dialog and returns a task that resolves to <c>true</c> when Ok is pressed and <c>false</c> when
        ///     Cancel is pressed or the window is closed without a choice.
        /// </summary>
        /// <param name="title">Window title, also used for the title bar icon.</param>
        /// <param name="message">Text shown in the window.</param>
        /// <param name="ok">Caption of the confirm button; <c>OK</c> when omitted.</param>
        /// <param name="cancel">Caption of the cancel button; an empty string hides the button.</param>
        /// <param name="waitTime">Seconds the Ok button stays disabled; zero disables the delay.</param>
        /// <returns>Task resolving to the user's answer.</returns>
        public static async Task<bool> DisplayDialogAsync(string title, string message,
            string ok = "OK", string cancel = "", int waitTime = 0)
        {
            var tcs = new TaskCompletionSource<bool>();

            CustomDialogWindow.Show(title, message, ok, cancel, waitTime,
                () => tcs.TrySetResult(true),
                () => tcs.TrySetResult(false));

            return await tcs.Task;
        }

        /// <summary>
        ///     Shows the dialog and reports the answer through callbacks instead of a task. Closing the window without
        ///     pressing a button counts as cancel.
        /// </summary>
        /// <param name="title">Window title, also used for the title bar icon.</param>
        /// <param name="message">Text shown in the window.</param>
        /// <param name="ok">Caption of the confirm button; <c>OK</c> when omitted.</param>
        /// <param name="cancel">Caption of the cancel button; an empty string hides the button.</param>
        /// <param name="onOk">Invoked when Ok is pressed; <c>null</c> ignores it.</param>
        /// <param name="onCancel">
        ///     Invoked when Cancel is pressed or the window is closed without a choice; <c>null</c> ignores it.
        /// </param>
        /// <param name="waitTime">Seconds the Ok button stays disabled; zero disables the delay.</param>
        public static void DisplayDialog(string title, string message, string ok = "OK", string cancel = "",
            Action? onOk = null, Action? onCancel = null, int waitTime = 0)
        {
            CustomDialogWindow.Show(title, message, ok, cancel, waitTime,
                () => onOk?.Invoke(),
                () => onCancel?.Invoke());
        }
    }
}