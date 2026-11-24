using System;
using JetBrains.Annotations;

namespace io.github.ykysnk.utils.Editor
{
    /// <summary>
    ///     Description of a menu item kept registered by <see cref="Extensions.MenuExtensions" />: path, shortcut,
    ///     checked state, sort priority and the callbacks Unity needs.
    /// </summary>
    [PublicAPI]
    public class MenuItemObject
    {
        /// <summary>
        ///     Creates a full description of a menu item. The name is sanitized the same way Unity sanitizes attribute
        ///     declared menu paths, so both kinds of item end up under the same path.
        /// </summary>
        /// <param name="name">Menu path, e.g. <c>Tools/My Tool/Do Something</c>.</param>
        /// <param name="shortcut">Shortcut string such as <c>#%g</c>; an empty string means none.</param>
        /// <param name="checked">Whether the item is drawn with a checkmark.</param>
        /// <param name="priority">Sort order within its submenu; lower values come first.</param>
        /// <param name="execute">Action run when the item is chosen.</param>
        /// <param name="validate">
        ///     Predicate deciding whether the item is enabled; <c>null</c> means always enabled.
        /// </param>
        public MenuItemObject(string name, string shortcut, bool @checked, int priority, Action execute,
            Func<bool>? validate)
        {
            Name = MenuServiceWrap.SanitizeMenuItemName(name);
            Shortcut = shortcut;
            Checked = @checked;
            Priority = priority;
            Execute = execute;
            Validate = validate;
        }

        /// <summary>
        ///     Creates an unchecked menu item without a validation callback.
        /// </summary>
        /// <param name="name">Menu path, sanitized like Unity does.</param>
        /// <param name="shortcut">Shortcut string such as <c>#%g</c>; an empty string means none.</param>
        /// <param name="priority">Sort order within its submenu; lower values come first.</param>
        /// <param name="execute">Action run when the item is chosen.</param>
        public MenuItemObject(string name, string shortcut, int priority, Action execute) : this(name, shortcut, false,
            priority, execute, null)
        {
        }

        /// <summary>
        ///     Creates an unchecked menu item without a shortcut or validation callback.
        /// </summary>
        /// <param name="name">Menu path, sanitized like Unity does.</param>
        /// <param name="priority">Sort order within its submenu; lower values come first.</param>
        /// <param name="execute">Action run when the item is chosen.</param>
        public MenuItemObject(string name, int priority, Action execute) : this(name, "", priority, execute)
        {
        }

        /// <summary>
        ///     Creates a menu item with the default priority for its kind: 10 under <c>GameObject/Create Other</c>,
        ///     1000 everywhere else, matching Unity's own ordering.
        /// </summary>
        /// <param name="name">Menu path, sanitized like Unity does.</param>
        /// <param name="execute">Action run when the item is chosen.</param>
        public MenuItemObject(string name, Action execute) : this(name,
            name.StartsWith("GameObject/Create Other") ? 10 : 1000, execute)
        {
        }

        /// <summary>
        ///     Menu path after sanitizing; this is the key the item is tracked under.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        ///     Shortcut string passed to Unity, empty when the item has none.
        /// </summary>
        public string Shortcut { get; private set; }

        /// <summary>
        ///     Whether Unity draws the item with a checkmark.
        /// </summary>
        public bool Checked { get; private set; }

        /// <summary>
        ///     Sort order within the submenu; lower values are drawn first.
        /// </summary>
        public int Priority { get; private set; }

        /// <summary>
        ///     Action run when the item is chosen.
        /// </summary>
        public Action Execute { get; private set; }

        /// <summary>
        ///     Predicate deciding whether the item is enabled, or <c>null</c> when it is always enabled.
        /// </summary>
        public Func<bool>? Validate { get; private set; }
    }
}