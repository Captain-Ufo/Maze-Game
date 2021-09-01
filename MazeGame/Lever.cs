using System;
using System.Collections.Generic;
using System.Text;

namespace MazeGame
{
    /// <summary>
    /// A gameplay element that can switch some impassable blocks on and off
    /// </summary>
    class Lever
    {
        /// <summary>
        /// Whether the lever is on or off
        /// </summary>
        public bool IsOn { get; set; } = false;

        int x;
        int y;

        private Coordinates[] connectedGates;

        /// <summary>
        /// Toggles the lever between on and off, and updates the gates
        /// </summary>
        /// <param name="floor">The world the lever and the connected gates are in</param>
        /// <param name="xOffset">Horizontal offset to account for the centering of the World map on the screen</param>
        /// <param name="yOffset">Vertical offset to account for the centering of the World map on the screen</param>
        public void Toggle(Floor floor, int xOffset, int yOffset)
        {
            IsOn = !IsOn;

            string leverSymbol = SymbolsConfig.LeverOnChar.ToString();

            if (!IsOn)
            {
                leverSymbol = SymbolsConfig.LeverOffChar.ToString();
            }

            foreach (Coordinates coordinates in connectedGates)
            {
                if (floor.GetElementAt(coordinates.X + xOffset, coordinates.Y + yOffset) == SymbolsConfig.GateChar.ToString())
                {
                    floor.ChangeElementAt(coordinates.X, coordinates.Y, SymbolsConfig.EmptySpace.ToString(), false, false);
                }
                else
                {
                    floor.ChangeElementAt(coordinates.X, coordinates.Y, SymbolsConfig.GateChar.ToString(), false, false);
                }
            }

            floor.ChangeElementAt(x + xOffset, y + yOffset, leverSymbol);
        }

        /// <summary>
        /// Sets the lever position on the World Map. To be used only when parsing the level file
        /// </summary>
        /// <param name="x">The X position</param>
        /// <param name="y">The Y position</param>
        public void SetLeverCoordinates(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        /// <summary>
        /// Assigns the gates that are swtiched by this lever. To be used only when parsing the level file
        /// </summary>
        /// <param name="gates">The array of gates (in the form of Coordinates objects)</param>
        public void AssignGates(Coordinates[] gates)
        {
            connectedGates = gates;
        }
    }
}
