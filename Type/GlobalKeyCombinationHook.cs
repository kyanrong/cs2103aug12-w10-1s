using System;
using System.Windows;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace Type
{
    internal delegate void KeyCombinationHandler();

    class GlobalKeyCombinationHook
    {
        private const string ATOM_NAME = "TypeShortcut";

        private Window targetWindow;

        private HwndSource targetWindowHandle;
        private short atom;

        private KeyCombinationHandler combinationHandler;

        private uint modifier;
        private uint vkey;

        public GlobalKeyCombinationHook(Window targetWindow, KeyCombinationHandler combinationHandler, uint modifier, uint vkey)
        {
            this.targetWindow = targetWindow;
            this.combinationHandler = combinationHandler;

            this.modifier = modifier;
            this.vkey = vkey;
        }

        public GlobalKeyCombinationHook StartListening()
        {
            //The window only gets a hwnd after it is shown the first time.
            //If the window has been shown before, the following has no side-effects;
            //Otherwise, a hwnd is created by showing an invisible window.
            var targetProperties = StoreProperties(targetWindow);
            MakeInvisible(targetWindow);
            targetWindow.Show();

            WindowInteropHelper wih = AddHook();
            atom = CreateAtom();
            RegisterCombination(wih);

            //We no longer need the window after the hook and callback have been registered.
            //Hide it and restore its original properties.
            targetWindow.Hide();
            RestoreProperties(targetWindow, targetProperties);

            return this;
        }

        private void RestoreProperties(Window win, Tuple<double, double, WindowStyle, bool, bool> prop)
        {
            win.Height = prop.Item1;
            win.Width = prop.Item2;
            win.WindowStyle = prop.Item3;
            win.ShowInTaskbar = prop.Item4;
            win.ShowActivated = prop.Item5;
        }

        private void MakeInvisible(Window win)
        {
            win.Height = 0;
            win.Width = 0;
            win.WindowStyle = WindowStyle.None;
            win.ShowInTaskbar = false;
            win.ShowActivated = false;
        }

        private Tuple<double, double, WindowStyle, bool, bool> StoreProperties(Window win)
        {
            double height = targetWindow.Height;
            double width = targetWindow.Width;
            WindowStyle style = targetWindow.WindowStyle;
            bool taskbar = targetWindow.ShowInTaskbar;
            bool activation = targetWindow.ShowActivated;

            return new Tuple<double, double, WindowStyle, bool, bool>(height, width, style, taskbar, activation);
        }

        public GlobalKeyCombinationHook StopListening()
        {
            UnregisterCombination();

            return this;
        }

        private void UnregisterCombination()
        {
            if (atom != 0)
            {
                UnregisterHotKey(targetWindowHandle.Handle, atom);
            }
        }

        private WindowInteropHelper AddHook()
        {
            var wih = new WindowInteropHelper(targetWindow);

            targetWindowHandle = HwndSource.FromHwnd(wih.Handle);
            targetWindowHandle.AddHook(HotKeyMsgHandler);

            return wih;
        }

        private void RegisterCombination(WindowInteropHelper wih)
        {
            bool result = RegisterHotKey(wih.Handle, atom, modifier, vkey);

            if (!result)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        private short CreateAtom()
        {
            short atom = GlobalAddAtom(ATOM_NAME);

            if (atom == 0)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            return atom;
        }

        private IntPtr HotKeyMsgHandler(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == GlobalKeyCombinationHook.WM_HOTKEY)
            {
                combinationHandler();
                handled = true;
            }

            return IntPtr.Zero;
        }

        //Win32 functions
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("kernel32", SetLastError = true)]
        public static extern short GlobalAddAtom(string lpString);

        [DllImport("kernel32", SetLastError = true)]
        public static extern short GlobalDeleteAtom(short nAtom);

        //fsModifiers defined by Win32 RegisterHotKey
        public const uint MOD_ALT = 0x0001;
        public const uint MOD_CONTROL = 0x0002;
        public const uint MOD_SHIFT = 0x0004;
        public const uint MOD_WIN = 0x0008;
        public const uint MOD_NOREPEAT = 0x4000;

        //WM_HOTKEY value
        private const int WM_HOTKEY = 0x0312;
    }
}
