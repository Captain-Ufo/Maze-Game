﻿using System;
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
        public int X { get; set; }
        /// <summary>
        /// The player's current Y position
        /// </summary>
        public int Y { get; set; }
        /// <summary>
        /// The current amount of collected treasure
        /// </summary>
        public int Booty { get; set; }
        /// <summary>
        /// Whether the player has moved in the current frame or not
        /// </summary>
        public bool HasPlayerMoved { get; set; }

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
        /// <param name="world">The level from which to gather the information required (which symbol to use, the state of the exit, etc)</param>
        public void Clear(World world)
        {
            string symbol = world.GetElementAt(X, Y);

            SetCursorPosition(X, Y);

            if (symbol == SymbolsConfig.ExitChar.ToString())
            {
                if (world.IsLocked)
                {
                    ForegroundColor = ConsoleColor.Red;
                }
            }

            Write(symbol);
            ResetColor();
        }
    }
}
