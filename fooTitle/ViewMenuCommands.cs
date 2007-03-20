using System;
using System.Collections.Generic;
using System.Text;

namespace fooTitle {

    class ToggleEnabledCommand : fooManagedWrapper.CCommand {

        public override void Execute() {
            Main.GetInstance().ToggleEnabled();
        }

        public override bool GetDescription(ref string desc) {
           desc = "Enables or disables foo_title display.";
           return true;
        }

        public override Guid GetGuid() {
            return new Guid(457, 784, 488, 36, 48, 79, 54, 12, 36, 56, 1);
        }

        public override string GetName() {
            return "Toggle foo_title";
        }
    }

    public class ViewMenuCommands : fooManagedWrapper.CManagedMainMenuCommands{
        public ViewMenuCommands() {
            this.cmds.Add(new ToggleEnabledCommand());
        }

        public override Guid GetCommandsParent() {
            return (Guid)fooManagedWrapper.CManagedMainMenuCommands.view;
        }

        public override uint GetCommandsSortPriority() {
            unchecked {
                return (uint)(int)fooManagedWrapper.CManagedMainMenuCommands.Flags.PriorityDontCare;
            }
        }
    }
}
