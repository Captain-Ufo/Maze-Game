using System;
using System.Collections.Generic;
using System.Text;
using static System.Console;

namespace MazeGame
{
    /// <summary>
    /// A gameplay element that (optionally) patrols the level and catches the player if moved within range.
    /// </summary>
    class Guard
    {
        private Coordinates[] patrolPath;
        private int nextPatrolPoint;

        private int bribeTimer;
        private bool hasBeenBribed;
        /// <summary>
        /// To be set depending on difficulty level. If true, it will prevent being bribed a second time
        /// </summary>
        public bool HasBeenBribedBefore { get; private set; }

        private bool easyGame;
        
        private string guardMarker = "@";
        private ConsoleColor guardColor = ConsoleColor.DarkRed;

        private int timeBetweenMoves = 150;
        private int timeSinceLastMove = 0;

        private Coordinates originPoint;

        /// <summary>
        /// The X coordinate of the Guard
        /// </summary>
        public int X { get; private set; }

        /// <summary>
        /// The Y coordinate of the guard
        /// </summary>
        public int Y { get; private set; }

        /// <summary>
        /// Instantiates a Guard Object and sets its parameters
        /// </summary>
        public Guard()
        {
            nextPatrolPoint = 0;
            hasBeenBribed = false;
            HasBeenBribedBefore = false;
            bribeTimer = 0;

            easyGame = false;
        }

        /// <summary>
        /// Assigns the Guard's initial position when the level is first loaded. To be used only when parsing the level file.
        /// </summary>
        /// <param name="x">The X position</param>
        /// <param name="y">The Y position</param>
        public void AssignOriginPoint(int x, int y)
        {
            X = x;
            Y = y;

            originPoint = new Coordinates(X, Y);
        }

        /// <summary>
        /// Assigns the offsets to account for map centering. To be used only at the beginning of the game
        /// </summary>
        /// <param name="xOffset">The X offset (from 0)</param>
        /// <param name="yOffset">The Y offset (from 0)</param>
        public void AssignOffset(int xOffset, int yOffset)
        {
            X += xOffset;
            Y += yOffset;

            originPoint.X = X;
            originPoint.Y = Y;

            for (int i = 0; i < patrolPath.Length; i++)
            {
                patrolPath[i].X += xOffset;
                patrolPath[i].Y += yOffset;
            }
        }

        /// <summary>
        /// Assigns the patrol path
        /// </summary>
        /// <param name="path">An array of patrol path points in the form of a Coordinates objects</param>
        public void AssignPatrol(Coordinates[] path)
        {
            patrolPath = path;
        }

        /// <summary>
        /// Updates the Guard's movement along its patrol and catches the player if within range
        /// </summary>
        /// <param name="world">The level the guard is patrolling</param>
        /// <param name="game">The current game</param>
        /// <param name="deltaTimeMS">Frame timing, to handle movement speeds</param>
        public void Patrol(World world, Game game, int deltaTimeMS)
        {
            timeSinceLastMove += deltaTimeMS;

            if (timeSinceLastMove < timeBetweenMoves)
            {
                return;
            }

            if (hasBeenBribed)
            {
                bribeTimer++;
            }

            if (bribeTimer > 4)
            {
                hasBeenBribed = false;
                if (!easyGame)
                {
                    HasBeenBribedBefore = true;
                }
                bribeTimer = 0;
            }

            if (patrolPath.Length > 0)
            {
                if (patrolPath[nextPatrolPoint].X == 0 && patrolPath[nextPatrolPoint].Y == 0)
                {
                    return;
                }

                this.Clear(world);

                if (X != patrolPath[nextPatrolPoint].X)
                {
                    if (X - patrolPath[nextPatrolPoint].X > 0)
                    {
                        if (world.IsPositionWalkable(X - 1, Y))
                        {
                            X--;
                        }
                    }
                    else
                    {
                        if (world.IsPositionWalkable(X + 1, Y))
                        {
                            X++;
                        }
                    }
                }
                else if (Y != patrolPath[nextPatrolPoint].Y)
                {
                    if (Y - patrolPath[nextPatrolPoint].Y > 0)
                    {
                        if (world.IsPositionWalkable(X, Y - 1))
                        {
                            Y--;
                        }
                    }
                    else
                    {
                        if (world.IsPositionWalkable(X, Y + 1))
                        {
                            Y++;
                        }
                    }
                }
                else
                {
                    if (nextPatrolPoint < patrolPath.Length-1)
                    {
                        nextPatrolPoint++;
                    }
                    else
                    {
                        nextPatrolPoint = 0;
                    }
                }
            }

            if (game.Player.X >= X-1 && game.Player.X <= X+1
                && game.Player.Y >= Y-1 && game.Player.Y <= Y+1)
            {
                if (!hasBeenBribed)
                {
                    game.CapturePlayer(this);
                }
            }

            if (world.GetElementAt(X, Y) == SymbolsConfig.LeverOnChar.ToString())
            {
                world.ToggleLever(X, Y);
            }

            timeSinceLastMove -= timeBetweenMoves;
        }

        /// <summary>
        /// Prevents a Game Over
        /// </summary>
        /// <param name="IsGameEasy">Sets the flag that is used to determine how many times a guard can be bribed</param>
        public void BribeGuard(bool IsGameEasy)
        {
            hasBeenBribed = true;
            easyGame = IsGameEasy;
        }

        /// <summary>
        /// Restores the guard to its conditions at the beginning of the level. To be used only when retrying levels
        /// </summary>
        public void Reset()
        {
            nextPatrolPoint = 0;
            bribeTimer = 0;
            hasBeenBribed = false;
            X = originPoint.X;
            Y = originPoint.Y;
            timeSinceLastMove = 0;
            HasBeenBribedBefore = false;
            easyGame = false;
        }

        /// <summary>
        /// Draws the guard symbol
        /// </summary>
        public void Draw()
        {
            ConsoleColor previousColor = ForegroundColor;
            ForegroundColor = guardColor;
            SetCursorPosition(X, Y);
            Write(guardMarker);
            ForegroundColor = previousColor;
        }

        /// <summary>
        /// Replaces the guard symbol with whatever static tile is in the map grid in the previous position of the guard
        /// </summary>
        /// <param name="world">The level from which to gather the information required (which symbol to use, the state of the exit, etc)</param>
        public void Clear(World world)
        {
            string symbol = world.GetElementAt(X, Y);

            SetCursorPosition(X, Y);

            if (symbol == "$")
            {
                ForegroundColor = ConsoleColor.Yellow;
            }

            if (symbol == SymbolsConfig.ExitChar.ToString())
            {
                if (world.IsLocked)
                {
                    ForegroundColor = ConsoleColor.Red;
                }
                else
                {
                    ForegroundColor = ConsoleColor.Green;
                }
            }
            Write(symbol);
            ResetColor();
        }
    }
}
