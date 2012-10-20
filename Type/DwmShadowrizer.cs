using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing.Printing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.ComponentModel;

namespace Type
{
    class DwmShadowrizer
    {
        public static void Shadowrize(Window wind)
        {
            WindowInteropHelper wih = new WindowInteropHelper(wind);
            int pvAttribute = DWMWA_NCRENDERING_POLICY;
            int result = DwmSetWindowAttribute(wih.Handle, DWMWA_NCRENDERING_POLICY, ref pvAttribute, sizeof(int));
            if (result == 0)
            {
                var margins = new Margins { Top = 0, Left = 0, Bottom = 0, Right = 0 };
                int extResult = DwmExtendFrameIntoClientArea(wih.Handle, ref margins);
            }
        }

        //Win32 functions
        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        [DllImport("dwmapi.dll")]
        private static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref Margins pMarInset);

        private const int DWMWA_NCRENDERING_POLICY = 2;
    }
}
