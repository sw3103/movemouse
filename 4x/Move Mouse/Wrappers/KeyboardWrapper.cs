using System;
using System.Threading;
using System.Windows.Forms;

namespace ellabi.Wrappers
{
    public static class KeyboardWrapper
    {
        public static void Start(string windowName, string input)
        {
            IntPtr zero = IntPtr.Zero;
            for (int i = 0; (i < 10) && (zero == IntPtr.Zero); i++)
            {
                Thread.Sleep(500);
                zero = NativeMethods.FindWindow(null, windowName);
            }
            if (zero != IntPtr.Zero)
            {
                NativeMethods.SetForegroundWindow(zero);
                SendKeys.SendWait(input);//"^{TAB}");
                SendKeys.Flush();
            }
        }
    }
}