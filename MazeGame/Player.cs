using System;
using System.Collections.Generic;
using System.Text;

using static System.Console;

namespace MazeGame
{
    class Player
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Booty { get; set; }

        public bool HasPlayerMoved { get; set; }

        private string playerMarker;
        private ConsoleColor playerColor;

        public Player(int startingX, int startingY, string marker = "☺", ConsoleColor color = ConsoleColor.Cyan)
        {
            X = startingX;
            Y = startingY;
            playerMarker = marker;
            playerColor = color;
        }

        public void Draw()
        {
            ConsoleColor previousColor = ForegroundColor;
            ForegroundColor = playerColor;
            SetCursorPosition(X, Y);
            Write(playerMarker);
            ForegroundColor = previousColor;
        }

        public void ClearPlayer(string symbol)
        {
            SetCursorPosition(X, Y);
            Write(symbol);
        }
    }
}
