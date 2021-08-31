using System;
using System.Collections.Generic;
using System.Linq;
using static System.Console;

namespace MazeGame
{
    /// <summary>
    /// A gameplay element that (optionally) patrols the level and catches the player if moved within range.
    /// </summary>
    class Guard
    {
        private Directions direction = Directions.down;
        private Coordinates[] patrolPath;
        private int nextPatrolPoint;
        private int verticalAggroDistance = 5;
        private int horizontalAggroDistance = 10;

        private int bribeTimer;
        private bool hasBeenBribed;

        private int alertTimer;
        private bool isAlerted;

        private bool isReturning;

        /// <summary>
        /// To be set depending on difficulty level. If true, it will prevent being bribed a second time
        /// </summary>
        public bool HasBeenBribedBefore { get; private set; }

        private bool easyGame;

        private string[] guardMarkersTable = new string[] {"^", ">", "V", "<" };
        private string guardMarker;
        private ConsoleColor guardSymbolColor = ConsoleColor.Black;
        private ConsoleColor guardTileColor = ConsoleColor.DarkRed;

        private int walkingSpeed = 150;
        private int runningSpeed = 120;
        private int timeBetweenMoves;
        private int timeSinceLastMove = 0;

        private Coordinates originPoint;

        private Coordinates lastKnownPlayerPosition;

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
            isAlerted = false;
            isReturning = false;
            HasBeenBribedBefore = false;
            bribeTimer = 0;
            alertTimer = 0;
            timeBetweenMoves = walkingSpeed;
            direction = Directions.up;
            guardMarker = guardMarkersTable[(int)direction];

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

            UpdateBribe();

            if (SpotPlayer(game, world))
            {
                if (!isAlerted)
                {
                    game.TimesSpotted ++ ;
                }

                guardTileColor = ConsoleColor.Red;
                lastKnownPlayerPosition = new Coordinates(game.MyPlayer.X, game.MyPlayer.Y);
                isAlerted = true;
                isReturning = false;
                timeBetweenMoves = runningSpeed;
                MoveTowards(lastKnownPlayerPosition, world);
            }
            else if (isAlerted)
            {
                guardTileColor = ConsoleColor.Red;
                AlertedBehavior(world);
            }
            else if (isReturning)
            {
                guardTileColor = ConsoleColor.DarkRed;
                ReturnToPatrol(world);
            }
            else
            {
                guardTileColor = ConsoleColor.DarkRed;
                timeBetweenMoves = walkingSpeed;
                Move(world, Patrol());
            }

            CatchPlayer(game);

            timeSinceLastMove -= timeBetweenMoves;
        }

        /// <summary>
        /// Prevents a Game Over
        /// </summary>
        /// <param name="IsGameEasy">Sets the flag that is used to determine how many times a guard can be bribed</param>
        public void BribeGuard(bool IsGameEasy)
        {
            hasBeenBribed = true;
            if (isAlerted)
            {
                isAlerted = false;
                isReturning = true;
            }
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
            alertTimer = 0;
            isAlerted = false;
            isReturning = false;
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

        private void UpdateBribe()
        {
            if (hasBeenBribed)
            {
                bribeTimer++;
            }

            if (bribeTimer > 10)
            {
                hasBeenBribed = false;
                if (!easyGame)
                {
                    HasBeenBribedBefore = true;
                }
                bribeTimer = 0;
            }
        }

        private bool SpotPlayer(Game game, World world)
        {
            if (hasBeenBribed)
            {
                return false;
            }

            switch (direction)
            {
                case Directions.up:
                    if (game.MyPlayer.X >= X - horizontalAggroDistance && game.MyPlayer.X <= X + horizontalAggroDistance
                        && game.MyPlayer.Y >= Y - verticalAggroDistance && game.MyPlayer.Y <= Y + 1)
                    {
                        Coordinates[] tilesBetweenGuardAndPlayer = GetTilesBetweenGuardAndPlayer(this.X, this.Y, game.MyPlayer.X, game.MyPlayer.Y);

                        foreach (Coordinates tile in tilesBetweenGuardAndPlayer)
                        {
                            if (!world.IsPositionWalkable(tile.X, tile.Y))
                            {
                                return false ;
                            }
                        }
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                case Directions.right:
                    if (game.MyPlayer.X >= X - 1 && game.MyPlayer.X <= X + horizontalAggroDistance
                        && game.MyPlayer.Y >= Y - verticalAggroDistance && game.MyPlayer.Y <= Y + verticalAggroDistance)
                    {
                        Coordinates[] tilesBetweenGuardAndPlayer = GetTilesBetweenGuardAndPlayer(this.X, this.Y, game.MyPlayer.X, game.MyPlayer.Y);

                        foreach (Coordinates tile in tilesBetweenGuardAndPlayer)
                        {
                            if (!world.IsPositionWalkable(tile.X, tile.Y))
                            {
                                return false;
                            }
                        }
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                 case Directions.down:
                    if (game.MyPlayer.X >= X - horizontalAggroDistance && game.MyPlayer.X <= X + horizontalAggroDistance
                        && game.MyPlayer.Y >= Y - 1 && game.MyPlayer.Y <= Y + verticalAggroDistance)
                    {
                        Coordinates[] tilesBetweenGuardAndPlayer = GetTilesBetweenGuardAndPlayer(this.X, this.Y, game.MyPlayer.X, game.MyPlayer.Y);

                        foreach (Coordinates tile in tilesBetweenGuardAndPlayer)
                        {
                            if (!world.IsPositionWalkable(tile.X, tile.Y))
                            {
                                return false;
                            }
                        }
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                 case Directions.left:
                    if (game.MyPlayer.X >= X - horizontalAggroDistance && game.MyPlayer.X <= X + 1
                        && game.MyPlayer.Y >= Y - verticalAggroDistance && game.MyPlayer.Y <= Y + verticalAggroDistance)
                    {
                        Coordinates[] tilesBetweenGuardAndPlayer = GetTilesBetweenGuardAndPlayer(this.X, this.Y, game.MyPlayer.X, game.MyPlayer.Y);

                        foreach (Coordinates tile in tilesBetweenGuardAndPlayer)
                        {
                            if (!world.IsPositionWalkable(tile.X, tile.Y))
                            {
                                return false;
                            }
                        }
                        return true;
                    }
                    else
                    {
                        return false;
                    }
            }
            return false;
        }

        private void AlertedBehavior(World world)
        {
            if (X != lastKnownPlayerPosition.X && Y != lastKnownPlayerPosition.Y)
            {
                MoveTowards(lastKnownPlayerPosition, world);
                return;
            }

            alertTimer++;

            if (alertTimer % 10 == 0)
            {
                if (direction == Directions.left)
                {
                    direction = Directions.up;
                }
                else
                {
                    direction += 1;
                }

                guardMarker = guardMarkersTable[(int)direction];
            }

            if (alertTimer > 50) 
            {
                alertTimer = 0;
                isAlerted = false;
                guardTileColor = ConsoleColor.DarkRed;
                timeBetweenMoves = walkingSpeed;
                isReturning = true;
            }
        }

        private void ReturnToPatrol(World world)
        {
            if (X != patrolPath[nextPatrolPoint].X && Y != patrolPath[nextPatrolPoint].Y)
            {
                MoveTowards(new Coordinates(patrolPath[nextPatrolPoint].X, patrolPath[nextPatrolPoint].Y), world);
                return;
            }

            isReturning = false;
        }

        private Tile Pathfind(World world, Tile pathStart, Tile destination)
        {
            pathStart.SetDistance(destination.X, destination.Y);
            List<Tile> activeTiles = new List<Tile>();
            activeTiles.Add(pathStart);
            List<Tile> visitedTiles = new List<Tile>();

            while (activeTiles.Any())
            {
                Tile tileToCheck = activeTiles.OrderBy(tile => tile.CostDistance).First();

                if (tileToCheck.X == destination.X && tileToCheck.Y == destination.Y)
                {
                    return tileToCheck.Parent;
                }

                visitedTiles.Add(tileToCheck);
                activeTiles.Remove(tileToCheck);

                List<Tile> walkableNeighbors = world.GetWalkableNeighborsOfTile(tileToCheck, destination);

                foreach (Tile neighbor in walkableNeighbors)
                {
                    if (visitedTiles.Any(tile => tile.X == neighbor.X && tile.Y == neighbor.Y))
                    {
                        //The tile was already evaluated by the pathfinding, so we skip it
                        continue;
                    }

                    if (activeTiles.Any(tile => tile.X == neighbor.X && tile.Y == neighbor.Y))
                    {
                        //re-check a tile evaluated on a previous cycle to see if now it has a better value
                        Tile existingTile = activeTiles.First(tile => tile.X == neighbor.X && tile.Y == neighbor.Y);

                        if (existingTile.CostDistance > neighbor.CostDistance)
                        {
                            activeTiles.Remove(existingTile);
                            activeTiles.Add(neighbor);
                        }
                    }
                    else
                    {
                        activeTiles.Add(neighbor);
                    }
                }
            }
            return null;
        }

        private void MoveTowards(Coordinates destination, World world)
        {
            Tile guardTile = new Tile(X, Y);
            Tile destinationTile = new Tile(destination.X, destination.Y);
            Tile tileToMoveTo = Pathfind(world, guardTile, destinationTile);

            if (tileToMoveTo != null)
            {
                Coordinates movementCoordinates = new Coordinates(tileToMoveTo.X, tileToMoveTo.Y);

                Move(world, movementCoordinates);
            }
        }

        private void CatchPlayer(Game game)
        {
            if (game.MyPlayer.X >= X - 1 && game.MyPlayer.X <= X + 1
                && game.MyPlayer.Y >= Y - 1 && game.MyPlayer.Y <= Y + 1)
            {
                if (!hasBeenBribed)
                {
                    game.CapturePlayer(this);
                }
            }
        }

        private Coordinates[] GetTilesBetweenGuardAndPlayer(int guardX, int guardY, int playerX, int playerY)
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
                guardX = playerX;
                playerX = temp;
                temp = guardY;
                guardY = playerY;
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

        private Coordinates Patrol()
        {
            if (patrolPath.Length > 0)
            {
                if (patrolPath[nextPatrolPoint].X == 0 && patrolPath[nextPatrolPoint].Y == 0)
                {
                    return new Coordinates(X, Y);
                }

                if (X != patrolPath[nextPatrolPoint].X || Y != patrolPath[nextPatrolPoint].Y)
                {
                    return new Coordinates(patrolPath[nextPatrolPoint].X, patrolPath[nextPatrolPoint].Y);
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

            return new Coordinates(X, Y);
        }

        private void Move(World world, Coordinates tileToMoveTo)
        {
            this.Clear(world);

            if (X != tileToMoveTo.X)
            {
                if (X - tileToMoveTo.X > 0)
                {
                    if (world.IsPositionWalkable(X - 1, Y))
                    {
                        X--;
                        direction = Directions.left;
                    }
                }
                else
                {
                    if (world.IsPositionWalkable(X + 1, Y))
                    {
                        X++;
                        direction = Directions.right;
                    }
                }
            }
            else if (Y != tileToMoveTo.Y)
            {
                if (Y - tileToMoveTo.Y > 0)
                {
                    if (world.IsPositionWalkable(X, Y - 1))
                    {
                        Y--;
                        direction = Directions.up;
                    }
                }
                else
                {
                    if (world.IsPositionWalkable(X, Y + 1))
                    {
                        Y++;
                        direction = Directions.down;
                    }
                }
            }

            guardMarker = guardMarkersTable[(int)direction];

            if (world.GetElementAt(X, Y) == SymbolsConfig.LeverOnChar.ToString())
            {
                world.ToggleLever(X, Y);
            }
        }
    }
}
