using System;
using System.Collections.Generic;
using System.Diagnostics;
using static System.Console;

namespace MazeGame
{
    /// <summary>
    /// Holds informations about the current level
    /// </summary>
    class Floor
    {
        private string[,] grid;

        private int rows;
        private int columns;

        private int xOffset;
        private int yOffset;

        private Coordinates exit;

        private LevelLock levelLock;

        private Dictionary<Coordinates, Lever> leversDictionary;

        private Coordinates[] treasures;

        private Guard[] levelGuards;

        private Stopwatch stopwatch;

        /// <summary>
        /// The name of the floor, extracted from the level file name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Whether the exit is open (either because there's no key in the level or because the player has collected the key) or not
        /// </summary>
        public bool IsLocked { get; private set; }

        /// <summary>
        /// The X coordinate of the player's starting position
        /// </summary>
        public int PlayerStartX { get; private set; }
        /// <summary>
        /// The Y coordinate of the player's starting position
        /// </summary>
        public int PlayerStartY { get; private set; }

        /// <summary>
        /// Instantiates a World object
        /// </summary>
        /// <param name="grid">The grid of sumbols that represents the level, in the form of a bi-dimensional string array</param>
        /// <param name="hasKey">Whether the level has a key and the exit is initially locked or not</param>
        /// <param name="startX">The player's starting X coordinate</param>
        /// <param name="startY">The player's starting Y coordinate</param>
        /// <param name="levers">The collection of levers in the level</param>
        /// <param name="guards">The collection of guards in the level</param>
        /// <param name="stopwatch">The game's Stopwatch field</param>
        public Floor(string name, string[,] grid, int startX, int startY, LevelLock levelLock, Coordinates exit,
                     Coordinates[] treasures, Dictionary<Coordinates, Lever> levers, Guard[] guards, Stopwatch stopwatch)
        {
            Name = name;

            this.grid = grid;

            rows = this.grid.GetLength(0);
            columns = this.grid.GetLength(1);

            xOffset = (WindowWidth / 2) - (columns / 2);
            yOffset = ((WindowHeight - 5) / 2) - (rows / 2);
            //yOffset = 0;

            this.stopwatch = stopwatch;

            this.treasures = treasures;

            this.exit = exit;

            leversDictionary = new Dictionary<Coordinates, Lever>();

            foreach(KeyValuePair<Coordinates, Lever> leverInfo in levers)
            {
                Coordinates coordinatesWithOffset = new Coordinates(leverInfo.Key.X + xOffset, leverInfo.Key.Y + yOffset);
                Lever lever = leverInfo.Value;

                leversDictionary[coordinatesWithOffset] = lever;
            }

            levelGuards = guards;

            foreach( Guard guard in guards)
            {
                guard.AssignOffset(xOffset, yOffset);
            }

            this.levelLock = levelLock;

            IsLocked = levelLock.IsLocked();

            PlayerStartX = startX + xOffset;
            PlayerStartY = startY + yOffset;
        }

        /// <summary>
        /// Draws the map on screen, automatically centered
        /// </summary>
        public void Draw()
        {
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    string element = grid[y, x];
                    SetCursorPosition(x + xOffset, y+yOffset);
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
                    else if (element == "☺")
                    {
                        ForegroundColor = ConsoleColor.DarkMagenta;
                    }
                    else
                    {
                        ForegroundColor = ConsoleColor.Gray;
                    }
                    Write(element);
                }
            }

            ResetColor();
        }


        private void DrawTile(int x, int y, string element)
        {
            SetCursorPosition(x, y);
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
            else if (element == "☺")
            {
                ForegroundColor = ConsoleColor.DarkMagenta;
            }
            else
            {
                ForegroundColor = ConsoleColor.Gray;
            }
            Write(element);
            ResetColor();
        }



        /// <summary>
        /// Checks if a certain position on the grid contains a symbol that can be traversed by the player or the guards
        /// </summary>
        /// <param name="x">The X coordinate of the position to check</param>
        /// <param name="y">The Y coordinate of the position to check</param>
        /// <returns>Returns true if walkable, false if not</returns>
        public bool IsPositionWalkable(int x, int y)
        {
            x -= xOffset;
            y -= yOffset;

            if (x < 0 || y < 0 || x >= columns || y >= rows)
            {
                return false;
            }

            return grid[y, x] == SymbolsConfig.EmptySpace.ToString() ||
                   grid[y, x] == "-" ||
                   grid[y, x] == "|" ||
                   grid[y, x] == "≡" ||
                   grid[y, x] == SymbolsConfig.ExitChar.ToString() ||
                   grid[y, x] == SymbolsConfig.KeyChar.ToString() ||
                   grid[y, x] == SymbolsConfig.TreasureChar.ToString() ||
                   grid[y, x] == SymbolsConfig.LeverOffChar.ToString() ||
                   grid[y, x] == SymbolsConfig.LeverOnChar.ToString();
        }

        public bool IsTileTransparent(int x, int y)
        {
            x -= xOffset;
            y -= yOffset;

            if (x < 0 || y < 0 || x >= columns || y >= rows)
            {
                return false;
            }

            return grid[y, x] == SymbolsConfig.EmptySpace.ToString() ||
                   grid[y, x] == SymbolsConfig.ExitChar.ToString() ||
                   grid[y, x] == SymbolsConfig.KeyChar.ToString() ||
                   grid[y, x] == SymbolsConfig.TreasureChar.ToString() ||
                   grid[y, x] == SymbolsConfig.LeverOffChar.ToString() ||
                   grid[y, x] == SymbolsConfig.LeverOnChar.ToString();
        }

        /// <summary>
        /// Gets which symbol is present in a given location
        /// </summary>
        /// <param name="x">The X coordinate of the position to check</param>
        /// <param name="y">The Y coordinate of the position to check</param>
        /// <returns>Returns the symbol found at these coordinates on the grid</returns>
        public string GetElementAt(int x, int y)
        {
            return grid[y - yOffset, x - xOffset];
        }

        public string GetElementAt(int x, int y, bool withOffset = true)
        {
            if (withOffset)
            {
                y -= yOffset;

                x -= xOffset;
            }

            return grid[y, x];
        }

        /// <summary>
        /// Replaces a symbol in a given location
        /// </summary>
        /// <param name="x">The X coordinate of the symbol to replace</param>
        /// <param name="y">The X coordinate of the symbol to replace</param>
        /// <param name="newElement">The new symbol</param>
        /// <param name="withOffset">(optional) default true, set to false if the indicated coordinates are without the offset applied</param>
        public void ChangeElementAt(int x, int y, string newElement, bool withOffset = true, bool redraw = true)
        {
            int destX = x;
            int destY = y;

            if (withOffset)
            {
                destX -= xOffset;
                destY -= yOffset;
            }

            grid[destY, destX] = newElement;

            if (redraw)
            {
                if (!withOffset)
                {
                    x += xOffset;
                    y += yOffset;
                }

                DrawTile(x, y, newElement);
            }
        }

        /// <summary>
        /// Resets the floor elements (levers and gates, treasures, keys, guards) to their original state
        /// </summary>
        public void Reset()
        {
            ResetLevers();
            ResetGuards();
            ResetKeys();
            ResetTreasures();
        }

        /// <summary>
        /// Pathfinding helper function. Returns the immediately adjecent walkable tiles (north, west, south and east) to the one provided
        /// </summary>
        /// <param name="currentTile">The Tile to find neighbors of</param>
        /// <param name="targetTile">The destination of the pathfinding</param>
        /// <returns></returns>
        public List<Tile> GetWalkableNeighborsOfTile(Tile currentTile, Tile targetTile)
        {
            List<Tile> neighborsList = new List<Tile>();

            if (currentTile.Y - 1 >= 0 && IsPositionWalkable(currentTile.X, currentTile.Y - 1))
            {
                neighborsList.Add(CreateNewTile(currentTile.X, currentTile.Y - 1));
            }

            if (currentTile.Y + 1 < rows + yOffset && IsPositionWalkable(currentTile.X, currentTile.Y + 1))
            {
                neighborsList.Add(CreateNewTile(currentTile.X, currentTile.Y + 1));
            }

            if (currentTile.X - 1 >= 0 && IsPositionWalkable(currentTile.X - 1, currentTile.Y))
            {
                neighborsList.Add(CreateNewTile(currentTile.X - 1, currentTile.Y));
            }

            if (currentTile.X + 1 < columns + xOffset && IsPositionWalkable(currentTile.X + 1, currentTile.Y))
            {
                neighborsList.Add(CreateNewTile(currentTile.X + 1, currentTile.Y));
            }

            return neighborsList;

            Tile CreateNewTile(int x, int y)
            {
                Tile tile = new Tile(x, y);
                tile.Parent = currentTile;
                tile.Cost = currentTile.Cost + 1;
                tile.SetDistance(targetTile.X, targetTile.Y);

                return tile;
            }
        }

        /// <summary>
        /// Toggles the lever in a given position on the grid
        /// </summary>
        /// <param name="x">The X coordinate on the grid of the level to toggle</param>
        /// <param name="y">The Y coordinate on the grid of the level to toggle</param>
        public void ToggleLever(int x, int y)
        {
            stopwatch.Stop();

            Coordinates leverCoord = new Coordinates(x, y);

            if (leversDictionary.ContainsKey(leverCoord))
            {
                Lever lever = leversDictionary[leverCoord];
                lever.Toggle(this, xOffset, yOffset);
            }

            stopwatch.Start();
        }

        /// <summary>
        /// Collects the key piece and cheks the locked status
        /// </summary>
        /// <param name="x">The X coordinate of the collected key</param>
        /// <param name="y">The Y coordinate of the collected key</param>
        public void CollectKeyPiece(int x, int y)
        {
            IsLocked = levelLock.CollectKeyPiece(this, x, y);

            if (!IsLocked)
            {
                DrawTile(exit.X + xOffset, exit.Y + yOffset, SymbolsConfig.ExitChar.ToString());
            }
        }

        /// <summary>
        /// Updates all the guards in the level. moving them along their patrols
        /// </summary>
        /// <param name="deltaDimeMS">The time passed since last check, to set the guard's speed</param>
        /// <param name="game">The current game</param>
        public void UpdateGuards(int deltaDimeMS, Game game)
        {
            if (levelGuards.Length > 0)
            {
                foreach (Guard guard in levelGuards)
                {
                    guard.UpdateBehavior(this, game, deltaDimeMS);
                }
            }
        }

        /// <summary>
        /// Draws all guards
        /// </summary>
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

        private void ResetTreasures()
        {
            foreach (Coordinates treasure in treasures)
            {
                ChangeElementAt(treasure.X, treasure.Y, SymbolsConfig.TreasureChar.ToString(), false, false);
            }
        }

        private void ResetLevers()
        {
            foreach (Lever lever in leversDictionary.Values)
            {
                if (lever.IsOn)
                {
                    lever.Toggle(this, xOffset, yOffset);
                }
            }
        }

        private void ResetKeys()
        {
            levelLock.ResetKeys(this);
        }

        private void ResetGuards()
        {
            foreach (Guard guard in levelGuards)
            {
                guard.Reset();
            }
        }
    }

    /// <summary>
    /// Helper struct that holds a int X, int Y pair
    /// </summary>
    public struct Coordinates
    {
        public int X;
        public int Y;

        /// <summary>
        /// Creates a Coordinate
        /// </summary>
        /// <param name="x">The X coordinate</param>
        /// <param name="y">the Y coordinate</param>
        public Coordinates(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    /// <summary>
    /// Helper class for pathfinding
    /// </summary>
    public class Tile
    {
        /// <summary>
        /// The X coordinate of this tile
        /// </summary>
        public int X { get; private set; }

        /// <summary>
        /// The Y coordinate of this tile
        /// </summary>
        public int Y { get; private set; }

        /// <summary>
        /// The cost to move into this tile, for the A* pathfinding
        /// </summary>
        public int Cost { get; set; }

        /// <summary>
        /// The as the crow flies distance between this tile and the destination, regardless of obstacles
        /// </summary>
        public int Distance { get; private set; }

        /// <summary>
        /// Cost + Distance
        /// </summary>
        public int CostDistance
        {
            get { return Cost + Distance; }
            set { }
        }

        /// <summary>
        /// The tile the pathfinding algorithm comes from when reaching this one
        /// </summary>
        public Tile Parent { get; set; }

        public Tile (int x, int y)
        {
            X = x;
            Y = y;

            Cost = 0;
            Distance = 0;
        }

        /// <summary>
        /// Calculates and assignes the absolute value of the distance between this tile and the target indicated
        /// </summary>
        /// <param name="destinationX">The X coordinate of the destination</param>
        /// <param name="destinationY">The Y coordinate of the destination</param>
        public void SetDistance (int destinationX, int destinationY)
        {
            Distance = Math.Abs(destinationX - X) + Math.Abs(destinationY - Y);
        }
    }
}
