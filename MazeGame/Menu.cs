using System;
using System.Collections.Generic;
using System.Text;
using static System.Console;

namespace MazeGame
{
    class Menu
    {
        private int selectedIndex;
        private string[] options;
        private string[] prompt;

        public Menu(string prompt, string[] options)
        {
            this.prompt = new string[] { prompt };
            this.options = options;
            selectedIndex = 0;
        }

        public Menu (string[] prompt, string[] options)
        {
            this.prompt = prompt;
            this.options = options;
            selectedIndex = 0;
        }

        public int Run(int xPos)
        {
            
            ConsoleKey keyPressed;

            do
            {
                SetCursorPosition(0, 0);
                DisplayOptions(xPos);

                ConsoleKeyInfo info = ReadKey(true);
                keyPressed = info.Key;

                switch (keyPressed)
                {
                    case ConsoleKey.UpArrow:
                    case ConsoleKey.NumPad8:
                    case ConsoleKey.W:

                        selectedIndex--;
                        if (selectedIndex < 0)
                        {
                            selectedIndex = options.Length - 1;
                        }
                        Beep(1000, 100);
                        break;

                    case ConsoleKey.DownArrow:
                    case ConsoleKey.NumPad2:
                    case ConsoleKey.S:

                        selectedIndex++;
                        if (selectedIndex == options.Length)
                        {
                            selectedIndex = 0;
                        }
                        Beep(1000, 100);
                        break;
                }
            }
            while (keyPressed != ConsoleKey.Enter);

            return selectedIndex;
        }

        public void UpdateMenuItems(string[] prompt, string[] options)
        {
            this.prompt = prompt;
            this.options = options;
        }

        private void DisplayOptions(int xPosition)
        {
            foreach (string s in prompt)
            {
                int posX = xPosition - (s.Length / 2);
                SetCursorPosition(posX, CursorTop);
                WriteLine(s);
            }

            for (int i = 0; i < options.Length; i++)
            {
                string option = options[i];
                int posX = xPosition - (option.Length / 2) - 4;
                string prefix = " ";
                string suffix = " ";

                if (i == selectedIndex)
                {
                    prefix = ">";
                    suffix = "<";
                    ForegroundColor = ConsoleColor.Black;
                    BackgroundColor = ConsoleColor.White;
                }
                else
                {
                    ForegroundColor = ConsoleColor.White;
                    BackgroundColor = ConsoleColor.Black;
                }

                SetCursorPosition(posX, CursorTop);
                WriteLine($"{prefix} [ {option} ] {suffix}");
            }
            CursorVisible = false;
            ResetColor();
        }
    }
}
