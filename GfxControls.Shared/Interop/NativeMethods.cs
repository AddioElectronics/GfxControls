using GfxControls.Shared.Interop;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace GfxControls.Interop
{
    internal static class NativeMethods
    {
        [DllImport("shell32.dll")]
        public static extern void DragAcceptFiles(IntPtr hWnd, bool fAccept);

        [DllImport("shell32.dll")]
        public static extern uint DragQueryFile(IntPtr hDrop, uint iFile, StringBuilder? lpszFile, int cch);

        [DllImport("shell32.dll")]
        public static extern void DragFinish(IntPtr hDrop);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        public static extern bool ScreenToClient(IntPtr hWnd, out POINT lpPoint);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll")]
        public static extern bool TrackMouseEvent(ref TRACKMOUSEEVENT lpPoint);
    }
}
