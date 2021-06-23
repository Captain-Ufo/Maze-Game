using System;
using System.Collections.Generic;
using System.Text;
using static System.Console;

namespace MazeGame
{
    class Guard
    {
        private Coordinates[] patrolPath;
        private int nextPatrolPoint;

        private int bribeTimer;
        private bool hasBeenBribed;
        public bool HasBeenBribedBefore { get; private set; }
        private bool easyGame;
        
        private string guardMarker = "@";
        private ConsoleColor guardColor = ConsoleColor.DarkRed;

        private int timeBetweenMoves = 150;
        private int timeSinceLastMove = 0;

        private Coordinates originPoint;

        public int X { get; private set; }
        public int Y { get; private set; }

        public Guard()
        {
            nextPatrolPoint = 0;
            hasBeenBribed = false;
            HasBeenBribedBefore = false;
            bribeTimer = 0;

            easyGame = false;
        }

        public void AssignOriginPoint(int x, int y)
        {
            X = x;
            Y = y;

            originPoint = new Coordinates(X, Y);
        }

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

        public void AssignPatrol(Coordinates[] path)
        {
            patrolPath = path;
        }

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

                this.Clear(world.GetElementAt(X, Y));

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
                world.ToggleLevers(X, Y);
            }

            timeSinceLastMove -= timeBetweenMoves;
        }

        public void BribeGuard(bool IsGameEasy)
        {
            hasBeenBribed = true;
            easyGame = IsGameEasy;
        }

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

        public void Draw()
        {
            ConsoleColor previousColor = ForegroundColor;
            ForegroundColor = guardColor;
            SetCursorPosition(X, Y);
            Write(guardMarker);
            ForegroundColor = previousColor;
        }

        public void Clear(string symbol)
        {
            SetCursorPosition(X, Y);
            Write(symbol);
        }
    }
}
