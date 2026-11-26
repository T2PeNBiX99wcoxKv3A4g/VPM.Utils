using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UIElements;

namespace io.github.ykysnk.utils.Editor
{
    /// <summary>
    ///     Body of a <see cref="CustomDialogWindow" />. Subclass this to show a custom interface instead of the plain
    ///     message, while the dialog keeps its frame, buttons and wait time. The window calls
    ///     <see cref="CreateContent" /> once, when it is created.
    /// </summary>
    [PublicAPI]
    public abstract class DialogContent
    {
        /// <summary>Size of the dialog window, in points.</summary>
        [PublicAPI]
        public virtual Vector2 Size => new(500, 250);

        /// <summary>Text copied by the dialog's Copy context menu entry; return <c>null</c> to hide that entry.</summary>
        [PublicAPI]
        [CanBeNull]
        public virtual string? CopyText => null;

        /// <summary>Builds the content shown between the title bar and the buttons.</summary>
        [PublicAPI]
        [NotNull]
        public abstract VisualElement CreateContent();
    }

    /// <summary>The default dialog body: a scrollable message that can be copied.</summary>
    [PublicAPI]
    public sealed class MessageDialogContent : DialogContent
    {
        private readonly string _message;

        public MessageDialogContent([NotNull] string message) => _message = message;

        [CanBeNull]
        public override string? CopyText => _message;

        [NotNull]
        public override VisualElement CreateContent()
        {
            var scroll = new ScrollView { name = "messageScroll" };
            scroll.AddToClassList("dialog-scroll");

            var label = new Label(_message)
            {
                name = "message",
                parseEscapeSequences = true
            };
            label.AddToClassList("dialog-message");

            scroll.Add(label);
            return scroll;
        }
    }
}
