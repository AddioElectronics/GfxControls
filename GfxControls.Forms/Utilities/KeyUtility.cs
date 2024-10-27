using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace GfxControls.Forms.Utilities
{
    public static class KeyUtility
    {
        private const int MAPVK_VK_TO_VSC = 0;
        private const int MAPVK_VSC_TO_VK = 1;
        private const int MAPVK_VK_TO_CHAR = 2;
        private const int MAPVK_VSC_TO_VK_EX = 3;
        private const int MAPVK_VK_TO_VSC_EX = 4;

        private enum MapType : uint
        {
            VirtualKeyToScanCode = MAPVK_VK_TO_VSC,
            ScanCodeToVirtualKey = MAPVK_VSC_TO_VK,
            VirtualKeyToChar = MAPVK_VK_TO_CHAR,
            ScanCodeToVirtualKeyEx = MAPVK_VSC_TO_VK_EX,
            VirtualKeyToScanCodeEx = MAPVK_VK_TO_VSC_EX
        }

        [DllImport("user32.dll")]
        private static extern uint MapVirtualKeyW(uint uCode, uint uMapType);

        [DllImport("user32.dll")]
        private static extern uint MapVirtualKeyW(uint uCode, MapType uMapType);


        public static char KeysToChar(Keys key)
        {
            return (char)MapVirtualKeyW((uint)key, MAPVK_VK_TO_CHAR);
        }

        public static bool IsCharacterKey(Keys key)
        {
            char c = KeysToChar(key);

            return char.IsLetterOrDigit(c)
                || char.IsSymbol(c)
                || char.IsPunctuation(c)
                || c == ' ';
        }


        //[DllImport("user32.dll")]
        //public static extern uint MapVirtualKeyExW(uint uCode, uint uMapType, IntPtr dwhkl);

        //[DllImport("user32.dll")]
        //public static extern IntPtr LoadKeyboardLayoutW([MarshalAs(UnmanagedType.LPWStr)]in string pwszKLID, uint flags);
    }
}
