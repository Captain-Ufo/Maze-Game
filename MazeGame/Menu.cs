using System;
using static System.Console;

namespace MazeGame
{
    /// <summary>
    /// Creates a console based keyboard controlled menu and handles user interactions with it
    /// </summary>
    class Menu
    {
        private int selectedIndex;
        private string[] options;
        private string[] prompt;

        /// <summary>
        /// Instantiates a Menu object
        /// </summary>
        /// <param name="prompt">The single string that prompts the player to chose</param>
        /// <param name="options">The list of options the menu displays</param>
        public Menu(string prompt, string[] options)
        {
            this.prompt = new string[] { prompt };
            this.options = options;
            selectedIndex = 0;
        }

        /// <summary>
        /// Instantiates a Menu object
        /// </summary>
        /// <param name="prompt">The prompt in string form, to be used for multiple lines prompts, or ascii/text based art</param>
        /// <param name="options">The list of options the menu displays</param>
        public Menu (string[] prompt, string[] options)
        {
            this.prompt = prompt;
            this.options = options;
            selectedIndex = 0;
        }


        /// <summary>
        /// Displays the menu and handles user inputs for options selection
        /// </summary>
        /// <param name="xPos">Horizontal position of the menu (prompt and options) on the screen. Input 0 for the left side of the screen, 
        /// any other number to center the menu around that position</param>
        /// <param name="yPos">Vertical position of the prompt on the screen. Input 0 for the very top of the screen</param>
        /// <param name="optionsOffset">The verticfal distance between the menu prompt and the option list</param>
        /// <returns>The index of the chosen option, after the user selects one and hits enter</returns>
        public int Run(int xPos, int yPos, int optionsOffset)
        {
            ConsoleKey keyPressed;

            do
            {
                SetCursorPosition(0, 0);
                DisplayPrompt(xPos, yPos);
                DisplayOptions(xPos, optionsOffset);

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

        /// <summary>
        /// Updates prompt and options in an already instantiated Menu object
        /// </summary>
        /// <param name="prompt">The new prompt, as a string array</param>
        /// <param name="options">The new array of options</param>
        public void UpdateMenuItems(string[] prompt, string[] options)
        {
            this.prompt = prompt;
            this.options = options;
        }

        /// <summary>
        /// Updates the prompt alone in an already instantiated Menu object
        /// </summary>
        /// <param name="prompt">The new prompt, as a string array</param>
        public void UpdateMenuPrompt(string[] prompt)
        {
            this.prompt = prompt;
        }

        /// <summary>
        /// Updates the options alone in an already instantiated Menu object
        /// </summary>
        /// <param name="prompt">The new array of options</param>
        public void UpdateMenuPrompt(string prompt)
        {
            this.prompt = new string[] { prompt };
        }

        public void UpdateMenuOptions(string[] options)
        {
            this.options = options;
        }

        private void DisplayPrompt(int xPosition, int yPosition)
        {
            SetCursorPosition(0, yPosition);

            foreach (string s in prompt)
            {
                int posX = xPosition - (s.Length / 2);
                SetCursorPosition(posX, CursorTop);
                WriteLine(s);
            }
        }

        private void DisplayOptions(int xPosition, int optionsOffset)
        {
            SetCursorPosition(xPosition, CursorTop + optionsOffset);
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
