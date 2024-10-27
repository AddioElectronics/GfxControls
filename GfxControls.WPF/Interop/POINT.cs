using System;
using System.Runtime.InteropServices;

namespace GfxControls.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int x;
        public int y;
    }
}
