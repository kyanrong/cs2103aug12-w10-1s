using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;
using System.Windows.Input;
using System.Runtime.InteropServices;

namespace Type
{
    class ShortcutKeyHook
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        private static Keys[] combination;
        private static int combinationIndex;
        private static Controller parent;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Keys pressed = (Keys)vkCode;

                if (pressed == combination[combinationIndex])
                {
                    combinationIndex++;
                    if (combinationIndex == (combination.Length))
                    {
                        parent.ShowUi();
                        combinationIndex = 0;
                    }
                }
                else
                {
                    combinationIndex = 0;
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        public ShortcutKeyHook(Controller parent, Key[] combination)
        {
            ShortcutKeyHook.parent = parent;

            ShortcutKeyHook.combination = new Keys[combination.Length];
            for (int i = 0; i < combination.Length; i++)
            {
                ShortcutKeyHook.combination[i] = (Keys) KeyInterop.VirtualKeyFromKey(combination[i]);
            }

            combinationIndex = 0;

            StartListening();
        }

        private void StartListening()
        {
            _hookID = SetHook(_proc);
        }

        public void StopListening()
        {
            UnhookWindowsHookEx(_hookID);
        }
    }
}
