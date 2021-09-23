﻿using System;

namespace MazeGame
{
    class Program
    {
        static void Main(string[] args)
        {
            ConsoleHelper.SetConsole("Heist!", 180, 56, false, false, true, true);

            Game game = new Game();
            game.Start();
            //MenuTesting.Test();
        }
    }

    class MenuTesting
    {
        public static void Test()
        {
            Console.Title = "Heist!";
            Console.SetWindowSize(180, 56);

            string prompt = "testing long menues";

            string[] shortOptions = new string[]
            {
                "0",
                "1",
                "2",
                "3"
            };

            string[] options = new string[]
            {
                "0",
                "1",
                "2",
                "3",
                "4",
                "5",
                "6",
                "7",
                "8",
                "9",
                "10",
                "11",
                "12",
                "13",
                "14",
                "15",
                "16",
                "17",
                "18",
                "19",
                "20",
                "21",
                "22",
                "23"
            };

            Menu testMenu = new Menu(prompt, options);

            int selectedIndex = testMenu.RunWithScrollingOptions(Console.WindowWidth / 2, 2, 2, 0, Console.WindowWidth, 7);

            Console.SetCursorPosition(0, Console.WindowHeight - 2);
            Console.WriteLine($"You chose {selectedIndex}");
            Console.ReadKey(true);
        }
    }
}
