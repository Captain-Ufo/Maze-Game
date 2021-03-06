using System;
using System.Collections.Generic;
using System.Text;

using static System.Console;

namespace MazeGame
{
    /// <summary>
    /// The player's avatar in the game
    /// </summary>
    class Player
    {
        /// <summary>
        /// The player's current X position
        /// </summary>
        public int X { get; private set; }
        /// <summary>
        /// The player's current Y position
        /// </summary>
        public int Y { get; private set; }
        /// <summary>
        /// The current amount of collected treasure
        /// </summary>
        public int Booty { get; set; }
        /// <summary>
        /// Whether the player has moved in the current frame or not
        /// </summary>
        public bool HasMoved { get; set; }

        private string playerMarker;
        private ConsoleColor playerColor;

        /// <summary>
        /// Instantiates a Player object
        /// </summary>
        /// <param name="startingX">The initial X position</param>
        /// <param name="startingY">The initial Y position</param>
        /// <param name="marker">(Optional) The symbol that represents the player on the map</param>
        /// <param name="color">(Optional) The color of the player's symbol</param>
        public Player(int startingX, int startingY, string marker = "☺", ConsoleColor color = ConsoleColor.Cyan)
        {
            X = startingX;
            Y = startingY;

            playerMarker = marker;
            playerColor = color;
        }

        /// <summary>
        /// Sets the player's starting position at the beginning of a level
        /// </summary>
        /// <param name="x">The X coordinate of the position</param>
        /// <param name="y">The Y coordinate of the position</param>
        public void SetStartingPosition(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Updates the player's coordinates, moving them by one tile at a time
        /// </summary>
        /// <param name="floor">The level the player is moving in</param>
        /// <param name="direction">The direction of the movement</param>
        /// <param name="deltaTimeMS">frame timing, to handle movement speed</param>
        public void Move(Floor floor, Directions direction)
        { 
            Clear(floor);

            switch (direction)
            {
                case Directions.up:
                    Y--;
                    break;
                case Directions.down:
                    Y++;
                    break;
                case Directions.left:
                    X--;
                    break;
                case Directions.right:
                    X++;
                    break;
            }
            HasMoved = true;
        }

        /// <summary>
        /// Draws the player's symbol
        /// </summary>
        public void Draw()
        {
            ConsoleColor previousColor = ForegroundColor;
            ForegroundColor = playerColor;
            SetCursorPosition(X, Y);
            Write(playerMarker);
            ForegroundColor = previousColor;
        }

        /// <summary>
        /// Replaces the player's symbol with whatever map symbol should be present in that position
        /// </summary>
        /// <param name="floor">The level from which to gather the information required (which symbol to use, the state of the exit, etc)</param>
        public void Clear(Floor floor)
        {
            string symbol = floor.GetElementAt(X, Y);

            SetCursorPosition(X, Y);

            if (symbol == SymbolsConfig.ExitChar.ToString())
            {
                if (floor.IsLocked)
                {
                    ForegroundColor = ConsoleColor.Red;
                }
            }

            Write(symbol);
            ResetColor();
        }
    }

    public enum Directions { up, right, down, left }
}
