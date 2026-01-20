using System;
using JetBrains.Annotations;

namespace io.github.ykysnk.utils.Editor
{
    [PublicAPI]
    public class MenuItemObject
    {
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

        public MenuItemObject(string name, string shortcut, int priority, Action execute) : this(name, shortcut, false,
            priority, execute, null)
        {
        }

        public MenuItemObject(string name, int priority, Action execute) : this(name, "", priority, execute)
        {
        }

        public MenuItemObject(string name, Action execute) : this(name,
            name.StartsWith("GameObject/Create Other") ? 10 : 1000, execute)
        {
        }

        public string Name { get; private set; }
        public string Shortcut { get; private set; }
        public bool Checked { get; private set; }
        public int Priority { get; private set; }
        public Action Execute { get; private set; }
        public Func<bool>? Validate { get; private set; }
    }
}