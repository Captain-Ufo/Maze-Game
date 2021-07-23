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
        private enum Directions { up, right, down, left }

        private Directions direction = Directions.down;
        private Coordinates[] patrolPath;
        private int nextPatrolPoint;
        private int aggroDistance = 5;

        private int bribeTimer;
        private bool hasBeenBribed;
        /// <summary>
        /// To be set depending on difficulty level. If true, it will prevent being bribed a second time
        /// </summary>
        public bool HasBeenBribedBefore { get; private set; }

        private bool easyGame;
        
        private string guardMarker = "V";
        private ConsoleColor guardSymbolColor = ConsoleColor.Black;
        private ConsoleColor guardTileColor = ConsoleColor.DarkRed;

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
        /// Updates the guard's AI behavior
        /// </summary>
        /// <param name="world">The level the guard is in</param>
        /// <param name="game">The current game</param>
        /// <param name="deltaTimeMS">frame timing, to handle movement speed</param>
        public void Update(World world, Game game, int deltaTimeMS)
        {
            timeSinceLastMove += deltaTimeMS;

            if (timeSinceLastMove < timeBetweenMoves)
            {
                return;
            }

            Patrol(world, deltaTimeMS);
            CatchPlayer(game, world);

            timeSinceLastMove -= timeBetweenMoves;
        }

        private void CatchPlayer(Game game, World world)
        {
            switch (direction)
            {
                case Directions.up:
                    if (game.MyPlayer.X >= X - aggroDistance && game.MyPlayer.X <= X + aggroDistance
                        && game.MyPlayer.Y >= Y - aggroDistance && game.MyPlayer.Y <= Y + 1)
                    {
                        Coordinates[] tilesBetweenGuardAndPlayer = GetTilesBetweenGuardAndPlayer(this.X, this.Y, game.MyPlayer.X, game.MyPlayer.Y);

                        foreach (Coordinates tile in tilesBetweenGuardAndPlayer)
                        {
                            if (!world.IsPositionWalkable(tile.X, tile.Y))
                            {
                                guardTileColor = ConsoleColor.DarkRed;
                                return;
                            }
                        }
                        guardTileColor = ConsoleColor.Red;
                    }
                    else
                    {
                        guardTileColor = ConsoleColor.DarkRed;
                    }
                     break;
                case Directions.right:
                    if (game.MyPlayer.X >= X - 1 && game.MyPlayer.X <= X + aggroDistance
                        && game.MyPlayer.Y >= Y - aggroDistance && game.MyPlayer.Y <= Y + aggroDistance)
                    {
                        Coordinates[] tilesBetweenGuardAndPlayer = GetTilesBetweenGuardAndPlayer(this.X, this.Y, game.MyPlayer.X, game.MyPlayer.Y);

                        foreach (Coordinates tile in tilesBetweenGuardAndPlayer)
                        {
                            if (!world.IsPositionWalkable(tile.X, tile.Y))
                            {
                                guardTileColor = ConsoleColor.DarkRed;
                                return;
                            }
                        }
                        guardTileColor = ConsoleColor.Red;
                    }
                    else
                    {
                        guardTileColor = ConsoleColor.DarkRed;
                    }
                    break;
                 case Directions.down:
                    if (game.MyPlayer.X >= X - aggroDistance && game.MyPlayer.X <= X + aggroDistance
                        && game.MyPlayer.Y >= Y - 1 && game.MyPlayer.Y <= Y + aggroDistance)
                    {
                        Coordinates[] tilesBetweenGuardAndPlayer = GetTilesBetweenGuardAndPlayer(this.X, this.Y, game.MyPlayer.X, game.MyPlayer.Y);

                        foreach (Coordinates tile in tilesBetweenGuardAndPlayer)
                        {
                            if (!world.IsPositionWalkable(tile.X, tile.Y))
                            {
                                guardTileColor = ConsoleColor.DarkRed;
                                return;
                            }
                        }
                        guardTileColor = ConsoleColor.Red; ;
                    }
                    else
                    {
                        guardTileColor = ConsoleColor.DarkRed;
                    }
                    break;
                 case Directions.left:
                    if (game.MyPlayer.X >= X - aggroDistance && game.MyPlayer.X <= X + 1
                        && game.MyPlayer.Y >= Y - aggroDistance && game.MyPlayer.Y <= Y + aggroDistance)
                    {
                        Coordinates[] tilesBetweenGuardAndPlayer = GetTilesBetweenGuardAndPlayer(this.X, this.Y, game.MyPlayer.X, game.MyPlayer.Y);

                        foreach (Coordinates tile in tilesBetweenGuardAndPlayer)
                        {
                            if (!world.IsPositionWalkable(tile.X, tile.Y))
                            {
                                guardTileColor = ConsoleColor.DarkRed;
                                return;
                            }
                        }
                        guardTileColor = ConsoleColor.Red;
                    }
                    else
                    {
                        guardTileColor = ConsoleColor.DarkRed;
                    }
                    break;
            }

            if (game.MyPlayer.X >= X - 1 && game.MyPlayer.X <= X + 1
                && game.MyPlayer.Y >= Y - 1 && game.MyPlayer.Y <= Y + 1)
            {
                if (!hasBeenBribed)
                {
                    game.CapturePlayer(this);
                }
            }
        }

        public Coordinates[] GetTilesBetweenGuardAndPlayer(int guardX, int guardY, int playerX, int playerY)
        {
            bool isLineSteep = Math.Abs(playerY - guardY) > Math.Abs(playerX - guardX);

            if (isLineSteep)
            {
                int temp = guardX;
                guardX = guardY;
                guardY = temp;
                temp = playerX;
                playerX = playerY;
                playerY = temp;
            }

            if (guardX > playerX)
            {
                int temp = guardX;
                guardX = guardY;
                guardY = temp;
                temp = playerX;
                playerX = playerY;
                playerY = temp;
            }

            int deltaX = playerX - guardX;
            int deltaY = Math.Abs(playerY - guardY);
            int error = deltaX / 2;
            int yStep = (guardY < playerY) ? 1 : -1;
            int y = guardY;

            List<Coordinates> tilesBetweenGuardAndPlayer = new List<Coordinates>();

            for (int x = guardX; x <= playerX; x++)
            {
                tilesBetweenGuardAndPlayer.Add(new Coordinates((isLineSteep ? y : x), (isLineSteep ? x : y)));
                error = error - deltaY;
                if (error < 0)
                {
                    y += yStep;
                    error += deltaX;
                }
            }

            return tilesBetweenGuardAndPlayer.ToArray();
        }

        /// <summary>
        /// Updates the Guard's movement along its patrol and catches the player if within range
        /// </summary>
        /// <param name="world">The level the guard is patrolling</param>
        /// <param name="game">The current game</param>
        /// <param name="deltaTimeMS">Frame timing, to handle movement speeds</param>
        private void Patrol(World world, int deltaTimeMS)
        {
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
                            direction = Directions.left;
                            guardMarker = "<";
                        }
                    }
                    else
                    {
                        if (world.IsPositionWalkable(X + 1, Y))
                        {
                            X++;
                            direction = Directions.right;
                            guardMarker = ">";
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
                            direction = Directions.up;
                            guardMarker = "^";
                        }
                    }
                    else
                    {
                        if (world.IsPositionWalkable(X, Y + 1))
                        {
                            Y++;
                            direction = Directions.down;
                            guardMarker = "v";
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

            if (world.GetElementAt(X, Y) == SymbolsConfig.LeverOnChar.ToString())
            {
                world.ToggleLever(X, Y);
            }
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
            ConsoleColor previousFColor = ForegroundColor;
            ConsoleColor previusBGColor = BackgroundColor;
            ForegroundColor = guardSymbolColor;
            BackgroundColor = guardTileColor;
            SetCursorPosition(X, Y);
            Write(guardMarker);
            ForegroundColor = previousFColor;
            BackgroundColor = previusBGColor;
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
