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
    delegate void UIDisplayHandler();

    class ShortcutKeyHook
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int KEYPRESS_DELAY = 300;

        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        private static Keys[] combination;
        private static int combinationIndex;
        private static UIDisplayHandler showUI;
        private static DateTime lastPressed;

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
            if (IsCallback(nCode, wParam))
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Keys pressed = (Keys)vkCode;

                if (IsNextValidKeystroke(pressed))
                {
                    if (IsFirstKeystroke())
                    {
                        lastPressed = DateTime.Now;
                    }

                    combinationIndex++;

                    if (PressedWithin(KEYPRESS_DELAY))
                    {
                        if (IsLastKeystroke())
                        {
                            showUI();
                            combinationIndex = 0;
                        }
                    }
                    else
                    {
                        combinationIndex = 0;;
                    }
                }
                else
                {
                    combinationIndex = 0;
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private static bool IsFirstKeystroke()
        {
            return combinationIndex == 0;
        }

        private static bool PressedWithin(int delay)
        {
            TimeSpan elapsed = DateTime.Now.Subtract(lastPressed);
            TimeSpan limit = new TimeSpan(0, 0, 0, 0, delay);
            return elapsed.CompareTo(limit) <= 0;
        }

        private static bool IsNextValidKeystroke(Keys pressed)
        {
            return pressed == combination[combinationIndex];
        }

        private static bool IsLastKeystroke()
        {
            return combinationIndex == combination.Length;
        }

        private static bool IsCallback(int nCode, IntPtr wParam)
        {
            return nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN;
        }

        public ShortcutKeyHook(UIDisplayHandler handler, Key[] combination)
        {
            ShortcutKeyHook.showUI = handler;

            ShortcutKeyHook.combination = new Keys[combination.Length];
            for (int i = 0; i < combination.Length; i++)
            {
                ShortcutKeyHook.combination[i] = (Keys)KeyInterop.VirtualKeyFromKey(combination[i]);
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
