using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace GfxControls.Shared.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct TRACKMOUSEEVENT
    {
        public enum Flags : uint
        {
            TME_CANCEL = 0x80000000,
            TME_HOVER = 0x00000001,
            TME_LEAVE = 0x00000002,
            TME_NONCLIENT = 0x00000010,
            TME_QUERY = 0x40000000,
        }

        public uint cbSize;
        [MarshalAs(UnmanagedType.U4)]
        public Flags dwFlags;
        [MarshalAs(UnmanagedType.SysInt)]
        public IntPtr hwndTrack;
        public uint dwHoverTime;
    }
}
