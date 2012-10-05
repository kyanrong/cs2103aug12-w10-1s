using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using System.Windows.Input;
using System.Runtime.InteropServices;

namespace Type
{
    delegate void UIDisplayHandler();

    class GlobalKeyCombinationHook
    {
        private delegate IntPtr GlobalKeypressEventCallback(int nCode, IntPtr wParam, IntPtr lParam);

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int KEYPRESS_DELAY = 200;

        private GlobalKeypressEventCallback callback;
        private IntPtr hookId = IntPtr.Zero;

        private UIDisplayHandler UIHandler;

        private HashSet<Keys> requiredCombination;
        private HashSet<Keys> currentCombination;
        private DateTime firstKey;
        private bool hasKeys;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, GlobalKeypressEventCallback lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private static IntPtr SetHook(GlobalKeypressEventCallback proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private IntPtr GlobalKeypressEventHandler(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (IsCallback(nCode, wParam))
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Keys pressed = (Keys)vkCode;

                if (requiredCombination.Contains(pressed))
                {
                    ProcessValidKeypress(pressed);
                }
                else
                {
                    ResetCurrentCombination();
                }
            }
            return CallNextHookEx(hookId, nCode, wParam, lParam);
        }

        private void ProcessValidKeypress(Keys pressed)
        {
            currentCombination.Add(pressed);

            if (hasKeys)
            {
                ProcessCombination();
            }
            else
            {
                RecordFirstValidKeypress();
            }
        }

        private void ProcessCombination()
        {
            if (IsRequiredCombination())
            {
                if (IsWithin(KEYPRESS_DELAY, firstKey))
                {
                    UIHandler();
                }
                else
                {
                    ResetCurrentCombination();
                }
            }
        }

        private bool IsRequiredCombination()
        {
            return currentCombination.Count == requiredCombination.Count;
        }

        private void RecordFirstValidKeypress()
        {
            firstKey = DateTime.Now;
            hasKeys = true;
        }

        private bool IsWithin(int delay, DateTime time)
        {
            TimeSpan elapsed = DateTime.Now - time;
            TimeSpan limit = new TimeSpan(0, 0, 0, 0, delay);
            return elapsed <= limit;
        }

        private void ResetCurrentCombination()
        {
            currentCombination.Clear();
            hasKeys = false;
        }

        private static bool IsCallback(int nCode, IntPtr wParam)
        {
            return nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN;
        }

        public GlobalKeyCombinationHook(UIDisplayHandler UIHandler, Key[] combination)
        {
            callback = GlobalKeypressEventHandler;

            this.UIHandler = UIHandler;
            requiredCombination = new HashSet<Keys>();
            currentCombination = new HashSet<Keys>();
            hasKeys = false;

            BuildRequiredKeyCombinationSet(combination);

            StartListening();
        }

        private void BuildRequiredKeyCombinationSet(Key[] combination)
        {
            foreach (Key key in combination)
            {
                requiredCombination.Add((Keys)KeyInterop.VirtualKeyFromKey(key));
            }
        }

        private void StartListening()
        {
            hookId = GlobalKeyCombinationHook.SetHook(callback);
        }

        public void StopListening()
        {
            GlobalKeyCombinationHook.UnhookWindowsHookEx(hookId);
        }
    }
}
