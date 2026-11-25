using System.Collections.Generic;

namespace ellabi.Platform
{
    /// <summary>
    /// Maps Windows Virtual Key codes (used in Settings.xml and KeystrokeAction)
    /// to X11 KeySyms and Linux evdev keycodes (for uinput).
    /// </summary>
    internal static class LinuxKeyMap
    {
        private static readonly Dictionary<int, nuint> VkToX11KeySym = new Dictionary<int, nuint>
        {
            [0x03] = 0xFF6B, // CANCEL -> XK_Break
            [0x08] = 0xFF08, // BACK -> XK_BackSpace
            [0x09] = 0xFF09, // TAB -> XK_Tab
            [0x0C] = 0xFF0B, // CLEAR -> XK_Clear
            [0x0D] = 0xFF0D, // RETURN -> XK_Return
            [0x10] = 0xFFE1, // SHIFT -> XK_Shift_L
            [0x11] = 0xFFE3, // CONTROL -> XK_Control_L
            [0x12] = 0xFFE9, // MENU -> XK_Alt_L
            [0x13] = 0xFF13, // PAUSE -> XK_Pause
            [0x14] = 0xFFE5, // CAPITAL -> XK_Caps_Lock
            [0x1B] = 0xFF1B, // ESCAPE -> XK_Escape
            [0x20] = 0x0020, // SPACE -> XK_space
            [0x21] = 0xFF55, // PRIOR -> XK_Page_Up
            [0x22] = 0xFF56, // NEXT -> XK_Page_Down
            [0x23] = 0xFF57, // END -> XK_End
            [0x24] = 0xFF50, // HOME -> XK_Home
            [0x25] = 0xFF51, // LEFT -> XK_Left
            [0x26] = 0xFF52, // UP -> XK_Up
            [0x27] = 0xFF53, // RIGHT -> XK_Right
            [0x28] = 0xFF54, // DOWN -> XK_Down
            [0x29] = 0xFF60, // SELECT -> XK_Select
            [0x2A] = 0xFF61, // PRINT -> XK_Print
            [0x2B] = 0xFF62, // EXECUTE -> XK_Execute
            [0x2C] = 0xFF61, // SNAPSHOT -> XK_Print
            [0x2D] = 0xFF63, // INSERT -> XK_Insert
            [0x2E] = 0xFFFF, // DELETE -> XK_Delete
            [0x2F] = 0xFF6A, // HELP -> XK_Help
            [0x30] = 0x0030, [0x31] = 0x0031, [0x32] = 0x0032, [0x33] = 0x0033, [0x34] = 0x0034,
            [0x35] = 0x0035, [0x36] = 0x0036, [0x37] = 0x0037, [0x38] = 0x0038, [0x39] = 0x0039,
            [0x41] = 0x0061, [0x42] = 0x0062, [0x43] = 0x0063, [0x44] = 0x0064, [0x45] = 0x0065,
            [0x46] = 0x0066, [0x47] = 0x0067, [0x48] = 0x0068, [0x49] = 0x0069, [0x4A] = 0x006A,
            [0x4B] = 0x006B, [0x4C] = 0x006C, [0x4D] = 0x006D, [0x4E] = 0x006E, [0x4F] = 0x006F,
            [0x50] = 0x0070, [0x51] = 0x0071, [0x52] = 0x0072, [0x53] = 0x0073, [0x54] = 0x0074,
            [0x55] = 0x0075, [0x56] = 0x0076, [0x57] = 0x0077, [0x58] = 0x0078, [0x59] = 0x0079,
            [0x5A] = 0x007A,
            [0x5B] = 0xFFEB, // LWIN -> XK_Super_L
            [0x5C] = 0xFFEC, // RWIN -> XK_Super_R
            [0x5D] = 0xFF67, // APPS -> XK_Menu
            [0x60] = 0xFFB0, [0x61] = 0xFFB1, [0x62] = 0xFFB2, [0x63] = 0xFFB3, [0x64] = 0xFFB4,
            [0x65] = 0xFFB5, [0x66] = 0xFFB6, [0x67] = 0xFFB7, [0x68] = 0xFFB8, [0x69] = 0xFFB9,
            [0x6A] = 0xFFAA, // MULTIPLY -> XK_KP_Multiply
            [0x6B] = 0xFFAB, // ADD -> XK_KP_Add
            [0x6C] = 0xFFAC, // SEPARATOR -> XK_KP_Separator
            [0x6D] = 0xFFAD, // SUBTRACT -> XK_KP_Subtract
            [0x6E] = 0xFFAE, // DECIMAL -> XK_KP_Decimal
            [0x6F] = 0xFFAF, // DIVIDE -> XK_KP_Divide
            [0x70] = 0xFFBE, [0x71] = 0xFFBF, [0x72] = 0xFFC0, [0x73] = 0xFFC1,
            [0x74] = 0xFFC2, [0x75] = 0xFFC3, [0x76] = 0xFFC4, [0x77] = 0xFFC5,
            [0x78] = 0xFFC6, [0x79] = 0xFFC7, [0x7A] = 0xFFC8, [0x7B] = 0xFFC9,
            [0x7C] = 0xFFCA, [0x7D] = 0xFFCB, [0x7E] = 0xFFCC, [0x7F] = 0xFFCD,
            [0x80] = 0xFFCE, [0x81] = 0xFFCF, [0x82] = 0xFFD0, [0x83] = 0xFFD1,
            [0x84] = 0xFFD2, [0x85] = 0xFFD3, [0x86] = 0xFFD4, [0x87] = 0xFFD5,
            [0x90] = 0xFF7F, // NUMLOCK -> XK_Num_Lock
            [0x91] = 0xFF14, // SCROLL -> XK_Scroll_Lock
            [0xA0] = 0xFFE1, // LSHIFT
            [0xA1] = 0xFFE2, // RSHIFT
            [0xA2] = 0xFFE3, // LCONTROL
            [0xA3] = 0xFFE4, // RCONTROL
            [0xA4] = 0xFFE9, // LMENU
            [0xA5] = 0xFFEA, // RMENU
            [0xA6] = 0x1008FF26, // XF86Back
            [0xA7] = 0x1008FF27, // XF86Forward
            [0xA8] = 0x1008FF29, // XF86Refresh
            [0xA9] = 0x1008FF28, // XF86Stop
            [0xAA] = 0x1008FF1B, // XF86Search
            [0xAB] = 0x1008FF30, // XF86Favorites
            [0xAC] = 0x1008FF18, // XF86HomePage
            [0xAD] = 0x1008FF12, // XF86AudioMute
            [0xAE] = 0x1008FF11, // XF86AudioLowerVolume
            [0xAF] = 0x1008FF13, // XF86AudioRaiseVolume
            [0xB0] = 0x1008FF17, // XF86AudioNext
            [0xB1] = 0x1008FF16, // XF86AudioPrev
            [0xB2] = 0x1008FF15, // XF86AudioStop
            [0xB3] = 0x1008FF14, // XF86AudioPlay
            [0xB4] = 0x1008FF19, // XF86Mail
        };

        // Linux input-event-codes.h keycodes for /dev/uinput
        private static readonly Dictionary<int, ushort> VkToEvdev = new Dictionary<int, ushort>
        {
            [0x08] = 14,  // KEY_BACKSPACE
            [0x09] = 15,  // KEY_TAB
            [0x0D] = 28,  // KEY_ENTER
            [0x10] = 42,  // KEY_LEFTSHIFT
            [0x11] = 29,  // KEY_LEFTCTRL
            [0x12] = 56,  // KEY_LEFTALT
            [0x13] = 119, // KEY_PAUSE
            [0x14] = 58,  // KEY_CAPSLOCK
            [0x1B] = 1,   // KEY_ESC
            [0x20] = 57,  // KEY_SPACE
            [0x21] = 104, // KEY_PAGEUP
            [0x22] = 109, // KEY_PAGEDOWN
            [0x23] = 107, // KEY_END
            [0x24] = 102, // KEY_HOME
            [0x25] = 105, // KEY_LEFT
            [0x26] = 103, // KEY_UP
            [0x27] = 106, // KEY_RIGHT
            [0x28] = 108, // KEY_DOWN
            [0x2C] = 99,  // KEY_SYSRQ
            [0x2D] = 110, // KEY_INSERT
            [0x2E] = 111, // KEY_DELETE
            [0x2F] = 138, // KEY_HELP
            [0x30] = 11, [0x31] = 2, [0x32] = 3, [0x33] = 4, [0x34] = 5,
            [0x35] = 6, [0x36] = 7, [0x37] = 8, [0x38] = 9, [0x39] = 10,
            [0x41] = 30, [0x42] = 48, [0x43] = 46, [0x44] = 32, [0x45] = 18,
            [0x46] = 33, [0x47] = 34, [0x48] = 35, [0x49] = 23, [0x4A] = 36,
            [0x4B] = 37, [0x4C] = 38, [0x4D] = 50, [0x4E] = 49, [0x4F] = 24,
            [0x50] = 25, [0x51] = 16, [0x52] = 19, [0x53] = 31, [0x54] = 20,
            [0x55] = 22, [0x56] = 47, [0x57] = 17, [0x58] = 45, [0x59] = 21,
            [0x5A] = 44,
            [0x5B] = 125, // KEY_LEFTMETA
            [0x5C] = 126, // KEY_RIGHTMETA
            [0x5D] = 127, // KEY_COMPOSE
            [0x5F] = 142, // KEY_SLEEP
            [0x60] = 82, [0x61] = 79, [0x62] = 80, [0x63] = 81, [0x64] = 75,
            [0x65] = 76, [0x66] = 77, [0x67] = 71, [0x68] = 72, [0x69] = 73,
            [0x6A] = 55,  // KEY_KPASTERISK
            [0x6B] = 78,  // KEY_KPPLUS
            [0x6D] = 74,  // KEY_KPMINUS
            [0x6E] = 83,  // KEY_KPDOT
            [0x6F] = 98,  // KEY_KPSLASH
            [0x70] = 59, [0x71] = 60, [0x72] = 61, [0x73] = 62,
            [0x74] = 63, [0x75] = 64, [0x76] = 65, [0x77] = 66,
            [0x78] = 67, [0x79] = 68, [0x7A] = 87, [0x7B] = 88,
            [0x7C] = 183, [0x7D] = 184, [0x7E] = 185, [0x7F] = 186,
            [0x80] = 187, [0x81] = 188, [0x82] = 189, [0x83] = 190,
            [0x84] = 191, [0x85] = 192, [0x86] = 193, [0x87] = 194,
            [0x90] = 69,  // KEY_NUMLOCK
            [0x91] = 70,  // KEY_SCROLLLOCK
            [0xA0] = 42,  // KEY_LEFTSHIFT
            [0xA1] = 54,  // KEY_RIGHTSHIFT
            [0xA2] = 29,  // KEY_LEFTCTRL
            [0xA3] = 97,  // KEY_RIGHTCTRL
            [0xA4] = 56,  // KEY_LEFTALT
            [0xA5] = 100, // KEY_RIGHTALT
            [0xA6] = 158, // KEY_BACK
            [0xA7] = 159, // KEY_FORWARD
            [0xA8] = 173, // KEY_REFRESH
            [0xA9] = 128, // KEY_STOP
            [0xAA] = 217, // KEY_SEARCH
            [0xAB] = 156, // KEY_BOOKMARKS
            [0xAC] = 172, // KEY_HOMEPAGE
            [0xAD] = 113, // KEY_MUTE
            [0xAE] = 114, // KEY_VOLUMEDOWN
            [0xAF] = 115, // KEY_VOLUMEUP
            [0xB0] = 163, // KEY_NEXTSONG
            [0xB1] = 165, // KEY_PREVIOUSSONG
            [0xB2] = 166, // KEY_STOPCD
            [0xB3] = 164, // KEY_PLAYPAUSE
            [0xB4] = 155, // KEY_MAIL
        };

        public static bool TryGetX11KeySym(int vk, out nuint keysym) => VkToX11KeySym.TryGetValue(vk, out keysym);

        public static bool TryGetEvdevCode(int vk, out ushort evdev) => VkToEvdev.TryGetValue(vk, out evdev);
    }
}
