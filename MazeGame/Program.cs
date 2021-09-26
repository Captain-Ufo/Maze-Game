using System;

namespace MazeGame
{
    class Program
    {
        static void Main(string[] args)
        {
            ConsoleHelper.SetConsole("Heist!", 180, 56, false, false, true, true, true);
            
            Game game = new Game();
            game.Start();
        }
    }
}
