using System;
using System.Collections.Generic;
using System.Text;

namespace MazeGame
{
    class Lever
    {
        public bool IsOn { get; set; } = false;

        int x;
        int y;

        private Coordinates[] connectedGates;

        public void Toggle(World world, int xOffset)
        {
            IsOn = !IsOn;

            string leverSymbol = SymbolsConfig.LeverOnChar.ToString();

            if (!IsOn)
            {
                leverSymbol = SymbolsConfig.LeverOffChar.ToString();
            }

            foreach (Coordinates coordinates in connectedGates)
            {
                if (world.GetElementAt(coordinates.X + xOffset, coordinates.Y) == SymbolsConfig.GateChar.ToString())
                {
                    world.ChangeElementAt(coordinates.X + xOffset, coordinates.Y, SymbolsConfig.EmptySpace.ToString());
                }
                else
                {
                    world.ChangeElementAt(coordinates.X + xOffset, coordinates.Y, SymbolsConfig.GateChar.ToString());
                }
            }

            world.ChangeElementAt(x + xOffset, y, leverSymbol);
        }

        public void SetLeverCoordinates(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public void AssignGates(Coordinates[] gates)
        {
            connectedGates = gates;
        }
    }
}
