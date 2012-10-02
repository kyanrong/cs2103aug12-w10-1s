using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Type
{
    class Controller
    {
        private Key[] START_KEY_COMBINATION = { Key.LeftShift, Key.Space };
        private ShortcutKeyHook globalHook;
        private MainWindow ui;

        public Controller()
        {
            ui = new MainWindow(this);
            globalHook = new ShortcutKeyHook(this, START_KEY_COMBINATION);

            Task t;
        }

        ~Controller()
        {
            globalHook.StopListening();
        }

        public void ShowUi()
        {
            ui.Show();
        }
    }
}
