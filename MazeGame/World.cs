﻿using System;
using System.Collections.Generic;
using System.Text;
using static System.Console;

namespace MazeGame
{
    class World
    {
        private string[,] grid;

        private int rows;
        private int columns;

        private int xOffset;

        private Dictionary<Coordinates, Lever> leversDictionary;

        private Guard[] levelGuards;

        public bool IsLocked { get; set; }
        public int PlayerStartX { get; private set; }
        public int PlayerStartY { get; private set; }

        public World(string[,] grid, bool hasKey, int startX, int startY, Dictionary<Coordinates, Lever> levers, Guard[] guards)
        {
            this.grid = grid;

            rows = this.grid.GetLength(0);
            columns = this.grid.GetLength(1);

            xOffset = (WindowWidth / 2) - (columns / 2);

            leversDictionary = new Dictionary<Coordinates, Lever>();

            foreach(KeyValuePair<Coordinates, Lever> leverInfo in levers)
            {
                Coordinates coordinatesWithOffset = new Coordinates(leverInfo.Key.X + xOffset, leverInfo.Key.Y);
                Lever lever = leverInfo.Value;

                leversDictionary[coordinatesWithOffset] = lever;
            }

            //leversDictionary = levers;
            levelGuards = guards;

            foreach( Guard guard in guards)
            {
                guard.AssignXOffset(xOffset);
            }

            IsLocked = hasKey;
            PlayerStartX = startX + xOffset;
            PlayerStartY = startY;
        }

        public void Draw()
        {
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    string element = grid[y, x];
                    SetCursorPosition(x + xOffset, y);
                    if (element == SymbolsConfig.ExitChar.ToString())
                    {
                        if (IsLocked)
                        {
                            ForegroundColor = ConsoleColor.Red;
                        }
                        else
                        {
                            ForegroundColor = ConsoleColor.Green;
                        }
                    }
                    else if (element == SymbolsConfig.KeyChar.ToString())
                    {
                        ForegroundColor = ConsoleColor.DarkYellow;
                    }
                    else if (element == SymbolsConfig.TreasureChar.ToString())
                    {
                        ForegroundColor = ConsoleColor.Yellow;
                    }
                    else
                    {
                        ForegroundColor = ConsoleColor.White;
                    }
                    Write(element);
                }
            }
        }

        public bool IsPositionWalkable(int x, int y)
        {
            x -= xOffset;

            if (x < 0 || y < 0 || x >= columns || y >= rows)
            {
                return false;
            }

            if(IsLocked && grid[y, x] == SymbolsConfig.ExitChar.ToString())
            {
                return false;
            }

            return grid[y, x] == SymbolsConfig.EmptySpace.ToString() ||
                   grid[y, x] == "░" ||
                   grid[y, x] == SymbolsConfig.ExitChar.ToString() ||
                   grid[y, x] == SymbolsConfig.KeyChar.ToString() ||
                   grid[y, x] == SymbolsConfig.TreasureChar.ToString() ||
                   grid[y, x] == SymbolsConfig.LeverOffChar.ToString() ||
                   grid[y, x] == SymbolsConfig.LeverOnChar.ToString();
        }

        public string GetElementAt(int x, int y)
        {
            return grid[y, x - xOffset];
        }

        public void ChangeElementAt(int x, int y, string newElement)
        {
            x -= xOffset;

            grid[y, x] = newElement;
            Draw();
        }

        public void ToggleLevers(int x, int y)
        {
            Beep(100, 100);

            Coordinates leverCoord = new Coordinates(x, y);

            if (leversDictionary.ContainsKey(leverCoord))
            {
                Lever lever = leversDictionary[leverCoord];
                lever.Toggle(this, xOffset);
            }
            Draw();
        }

        public void UpdateGuards(int deltaDimeMS, Game game)
        {
            if (levelGuards.Length > 0)
            {
                foreach (Guard guard in levelGuards)
                {
                    guard.Patrol(this, game, deltaDimeMS);
                }
            }
        }

        public void ResetGuards()
        {
            foreach (Guard guard in levelGuards)
            {
                guard.Reset();
            }
        }

        public void DrawGuards()
        {
            if (levelGuards.Length > 0)
            {
                foreach (Guard guard in levelGuards)
                {
                    guard.Draw();
                }
            }
        }
    }

    public struct Coordinates
    {
        public int X;
        public int Y;

        public Coordinates(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}