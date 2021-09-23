using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using static System.Console;

namespace MazeGame
{
    class ConsoleSetter
    {
        private const int MF_BYCOMMAND = 0x00000000;
        public const int SC_CLOSE = 0xF060;
        public const int SC_MINIMIZE = 0xF020;
        public const int SC_MAXIMIZE = 0xF030;
        public const int SC_SIZE = 0xF000;

        [DllImport("user32.dll")]
        public static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();

        public static void SetConsole(string title, int width, int height, bool blockClosing, bool blockMinimize, bool blockMaximize, bool blockResize )
        {
            IntPtr handle = GetConsoleWindow();
            IntPtr sysMenu = GetSystemMenu(handle, false);

            if (handle != IntPtr.Zero)
            {
                if (blockClosing) { DeleteMenu(sysMenu, SC_CLOSE, MF_BYCOMMAND); }
                if (blockMinimize) { DeleteMenu(sysMenu, SC_MINIMIZE, MF_BYCOMMAND); }
                if (blockMaximize) { DeleteMenu(sysMenu, SC_MAXIMIZE, MF_BYCOMMAND); }
                if (blockResize) { DeleteMenu(sysMenu, SC_SIZE, MF_BYCOMMAND); }
            }

            Title = title;

            try
            {
                SetWindowSize(width, height);
            }
            catch (ArgumentOutOfRangeException)
            {
                DisplayConsoleSizeWarning();
            }
        }

        private static void DisplayConsoleSizeWarning()
        {
            WriteLine("Error setting the preferred console size.");
            WriteLine("You can continue using the program, but glitches may occour, and it will probably not be displayed correctly.");
            WriteLine("To fix this error, please try changing the character size of the Console.");

            WriteLine("\n\nPress any key to continue...");
            ReadKey(true);
        }
    }
}
