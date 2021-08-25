using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using static System.Console;

namespace MazeGame
{
    /// <summary>
    /// Holds informations about the current level
    /// </summary>
    class World
    {
        private string[,] grid;

        private int rows;
        private int columns;

        private int xOffset;
        private int yOffset;

        private LevelLock levelLock;

        private Dictionary<Coordinates, Lever> leversDictionary;

        private Guard[] levelGuards;

        private Stopwatch stopwatch;

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
        public World(string[,] grid, int startX, int startY, LevelLock levelLock, Dictionary<Coordinates, Lever> levers, Guard[] guards, Stopwatch stopwatch)
        {
            this.grid = grid;

            rows = this.grid.GetLength(0);
            columns = this.grid.GetLength(1);

            xOffset = (WindowWidth / 2) - (columns / 2);
            yOffset = ((WindowHeight - 5) / 2) - (rows / 2);
            //yOffset = 0;

            leversDictionary = new Dictionary<Coordinates, Lever>();

            this.stopwatch = stopwatch;

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
                    else
                    {
                        ForegroundColor = ConsoleColor.White;
                    }
                    Write(element);
                }
            }
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
                   grid[y, x] == "░" ||
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
        public void ChangeElementAt(int x, int y, string newElement, bool withOffset = true, bool redrawScene = true)
        {
            if (withOffset)
            {
                x -= xOffset;
                y -= yOffset;
            }

            grid[y, x] = newElement;

            if (redrawScene)
            {
                Draw();
            }
        }

        /// <summary>
        /// Pathfinding helper function. Returns the immediately adjecent walkable tiles (north, west, south and east) to the one provided
        /// </summary>
        /// <param name="currentTile">The Tile to find neighbors of</param>
        /// <param name="targetTile">The destination of the pathfinding</param>
        /// <returns></returns>
        public List<Tile> GetWalkableNaighborsOfTile(Tile currentTile, Tile targetTile)
        {
            List<Tile> neighborsList = new List<Tile>()
            {
                new Tile(currentTile.X, currentTile.Y - 1),
                new Tile(currentTile.X, currentTile.Y + 1),
                new Tile(currentTile.X - 1, currentTile.Y),
                new Tile(currentTile.X + 1, currentTile.Y)
            };

            foreach (Tile tile in neighborsList)
            {
                tile.Parent = currentTile;
                tile.Cost = currentTile.Cost + 1;
                tile.SetDistance(targetTile.X, targetTile.Y);
            }

            return neighborsList
                                .Where(tile => tile.X >= 0 && tile.X < columns)
                                .Where(tile => tile.Y >= 0 && tile.Y < rows)
                                .Where(tile => IsPositionWalkable(tile.X, tile.Y))
                                .ToList();
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
            Draw();
        }

        public void CollectKeyPiece(int x, int y)
        {
            IsLocked = levelLock.CollectKeyPiece(this, x, y);
            Draw();
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
                    guard.Update(this, game, deltaDimeMS);
                }
            }
        }

        /// <summary>
        /// Resets all guards to their state at the beginning of the level
        /// </summary>
        public void ResetGuards()
        {
            foreach (Guard guard in levelGuards)
            {
                guard.Reset();
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
        public int CostDistance { get; set; }

        /// <summary>
        /// The tile the pathfinding algorithm comes from when reaching this one
        /// </summary>
        public Tile Parent { get; set; }

        public Tile (int x, int y)
        {
            X = x;
            Y = y;
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
