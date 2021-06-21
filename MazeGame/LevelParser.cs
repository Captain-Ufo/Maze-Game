using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace MazeGame
{
    class LevelParser
    {
        public static LevelInfo ParseFileToLevelInfo(string filePath)
        {
            string[] lines = File.ReadAllLines(filePath);
            string firstLine = lines[0];

            int rows = lines.Length;
            int columns = firstLine.Length;

            string[,] grid = new string[rows, columns];

            bool hasKey = false;
            int playerStartX = 0;
            int playerStartY = 0;
            int totalGold = 0;

            Dictionary<Coordinates, Lever> leversDictionary = new Dictionary<Coordinates, Lever>();
            
            Lever leverA = new Lever();
            Lever leverE = new Lever();
            Lever leverI = new Lever();
            Lever leverO = new Lever();
            Lever leverU = new Lever();
            Lever leverY = new Lever();

            List<Coordinates> leverAGates = new List<Coordinates>();
            List<Coordinates> leverEGates = new List<Coordinates>();
            List<Coordinates> leverIGates = new List<Coordinates>();
            List<Coordinates> leverOGates = new List<Coordinates>();
            List<Coordinates> leverUGates = new List<Coordinates>();
            List<Coordinates> leverYGates = new List<Coordinates>();

            List<Guard> levelGuards = new List<Guard>();

            Guard guard1 = new Guard();
            Guard guard2 = new Guard();
            Guard guard3 = new Guard();
            Guard guard4 = new Guard();
            Guard guard5 = new Guard();
            Guard guard6 = new Guard();
            Guard guard7 = new Guard();
            Guard guard8 = new Guard();
            Guard guard9 = new Guard();
            Guard guard10 = new Guard();

            List<Coordinates> guard1Patrol = new List<Coordinates>();
            List<Coordinates> guard2Patrol = new List<Coordinates>();
            List<Coordinates> guard3Patrol = new List<Coordinates>();
            List<Coordinates> guard4Patrol = new List<Coordinates>();
            List<Coordinates> guard5Patrol = new List<Coordinates>();
            List<Coordinates> guard6Patrol = new List<Coordinates>();
            List<Coordinates> guard7Patrol = new List<Coordinates>();
            List<Coordinates> guard8Patrol = new List<Coordinates>();
            List<Coordinates> guard9Patrol = new List<Coordinates>();
            List<Coordinates> guard10Patrol = new List<Coordinates>();

            for (int y = 0; y < rows; y++)
            {
                string line = lines[y];
                for (int x = 0; x < columns; x++)
                {
                    char currentChar = line[x];

                    Coordinates leverAGate;
                    Coordinates patrolPoint;

                    switch (currentChar)
                    {
                        case SymbolsConfig.KeyChar:
                            hasKey = true;
                            break;
                        case SymbolsConfig.SpawnChar:
                            playerStartX = x;
                            playerStartY = y;
                            currentChar = SymbolsConfig.EmptySpace;
                            break;
                        case 'A':
                            leverA.SetLeverCoordinates(x, y);
                            Coordinates leverACoord = new Coordinates(x, y);
                            leversDictionary[leverACoord] = leverA;
                            currentChar = SymbolsConfig.LeverOffChar;
                            break;
                        case 'a':
                        case 'à':
                            leverAGate = new Coordinates(x, y);
                            leverAGates.Add(leverAGate);
                            if (currentChar == 'à') { currentChar = SymbolsConfig.EmptySpace; }
                            else { currentChar = SymbolsConfig.GateChar; }
                            break;
                        case 'E':
                            leverE.SetLeverCoordinates(x, y);
                            Coordinates leverECoord = new Coordinates(x, y);
                            leversDictionary[leverECoord] = leverE;
                            currentChar = SymbolsConfig.LeverOffChar;
                            break;
                        case 'e':
                        case 'è':
                            Coordinates leverBGate = new Coordinates(x, y);
                            leverEGates.Add(leverBGate);
                            if (currentChar == 'è') { currentChar = SymbolsConfig.EmptySpace; }
                            else { currentChar = SymbolsConfig.GateChar; }
                            break;
                        case 'I':
                            leverI.SetLeverCoordinates(x, y);
                            Coordinates leverICoord = new Coordinates(x, y);
                            leversDictionary[leverICoord] = leverI;
                            currentChar = SymbolsConfig.LeverOffChar;
                            break;
                        case 'i':
                        case 'ì':
                            Coordinates leverIGate = new Coordinates(x, y);
                            leverIGates.Add(leverIGate);
                            if (currentChar == 'ì') { currentChar = SymbolsConfig.EmptySpace; }
                            else { currentChar = SymbolsConfig.GateChar; }
                            break;
                        case 'O':
                            leverO.SetLeverCoordinates(x, y);
                            Coordinates leverOCoord = new Coordinates(x, y);
                            leversDictionary[leverOCoord] = leverO;
                            currentChar = SymbolsConfig.LeverOffChar;
                            break;
                        case 'o':
                        case 'ò':
                            Coordinates leverOGate = new Coordinates(x, y);
                            leverOGates.Add(leverOGate);
                            if (currentChar == 'ò') { currentChar = SymbolsConfig.EmptySpace; }
                            else { currentChar = SymbolsConfig.GateChar; }
                            break;
                        case 'U':
                            leverU.SetLeverCoordinates(x, y);
                            Coordinates leverUCoord = new Coordinates(x, y);
                            leversDictionary[leverUCoord] = leverU;
                            currentChar = SymbolsConfig.LeverOffChar;
                            break;
                        case 'u':
                        case 'ù':
                            Coordinates leverUGate = new Coordinates(x, y);
                            leverUGates.Add(leverUGate);
                            if (currentChar == 'ù') { currentChar = SymbolsConfig.EmptySpace; }
                            else { currentChar = SymbolsConfig.GateChar; }
                            break;
                        case 'Y':
                            leverY.SetLeverCoordinates(x, y);
                            Coordinates leverYCoord = new Coordinates(x, y);
                            leversDictionary[leverYCoord] = leverY;
                            currentChar = SymbolsConfig.LeverOffChar;
                            break;
                        case 'y':
                        case 'ÿ':
                            Coordinates leverYGate = new Coordinates(x, y);
                            leverYGates.Add(leverYGate);
                            if (currentChar == 'ÿ') { currentChar = SymbolsConfig.EmptySpace; }
                            else { currentChar = SymbolsConfig.GateChar; }
                            break;
                        case 'B':
                            guard1.AssignOriginPoint(x, y);
                            levelGuards.Add(guard1);
                            currentChar = SymbolsConfig.EmptySpace;
                            break;
                        case 'b':
                            patrolPoint = new Coordinates(x, y);
                            guard1Patrol.Add(patrolPoint);
                            currentChar = SymbolsConfig.EmptySpace;
                            break;
                        case 'C':
                            guard2.AssignOriginPoint(x, y);
                            levelGuards.Add(guard2);
                            currentChar = SymbolsConfig.EmptySpace;
                            break;
                        case 'c':
                            patrolPoint = new Coordinates(x, y);
                            guard2Patrol.Add(patrolPoint);
                            currentChar = SymbolsConfig.EmptySpace;
                            break;
                        case 'D':
                            guard3.AssignOriginPoint(x, y);
                            levelGuards.Add(guard3);
                            currentChar = SymbolsConfig.EmptySpace;
                            break;
                        case 'd':
                            patrolPoint = new Coordinates(x, y);
                            guard3Patrol.Add(patrolPoint);
                            currentChar = SymbolsConfig.EmptySpace;
                            break;
                        case 'F':
                            guard4.AssignOriginPoint(x, y);
                            levelGuards.Add(guard4);
                            currentChar = SymbolsConfig.EmptySpace;
                            break;
                        case 'f':
                            patrolPoint = new Coordinates(x, y);
                            guard4Patrol.Add(patrolPoint);
                            currentChar = SymbolsConfig.EmptySpace;
                            break;
                        case 'G':
                            guard5.AssignOriginPoint(x, y);
                            levelGuards.Add(guard5);
                            currentChar = SymbolsConfig.EmptySpace;
                            break;
                        case 'g':
                            patrolPoint = new Coordinates(x, y);
                            guard5Patrol.Add(patrolPoint);
                            currentChar = SymbolsConfig.EmptySpace;
                            break;
                        case 'J':
                            guard6.AssignOriginPoint(x, y);
                            levelGuards.Add(guard6);
                            currentChar = SymbolsConfig.EmptySpace;
                            break;
                        case 'j':
                            patrolPoint = new Coordinates(x, y);
                            guard6Patrol.Add(patrolPoint);
                            currentChar = SymbolsConfig.EmptySpace;
                            break;
                        case 'K':
                            guard7.AssignOriginPoint(x, y);
                            levelGuards.Add(guard7);
                            currentChar = SymbolsConfig.EmptySpace;
                            break;
                        case 'k':
                            patrolPoint = new Coordinates(x, y);
                            guard7Patrol.Add(patrolPoint);
                            currentChar = SymbolsConfig.EmptySpace;
                            break;
                        case 'L':
                            guard8.AssignOriginPoint(x, y);
                            levelGuards.Add(guard8);
                            currentChar = SymbolsConfig.EmptySpace;
                            break;
                        case 'l':
                            patrolPoint = new Coordinates(x, y);
                            guard8Patrol.Add(patrolPoint);
                            currentChar = SymbolsConfig.EmptySpace;
                            break;
                        case 'M':
                            guard9.AssignOriginPoint(x, y);
                            levelGuards.Add(guard9);
                            currentChar = SymbolsConfig.EmptySpace;
                            break;
                        case 'm':
                            patrolPoint = new Coordinates(x, y);
                            guard9Patrol.Add(patrolPoint);
                            currentChar = SymbolsConfig.EmptySpace;
                            break;
                        case 'N':
                            guard10.AssignOriginPoint(x, y);
                            levelGuards.Add(guard10);
                            currentChar = SymbolsConfig.EmptySpace;
                            break;
                        case 'n':
                            patrolPoint = new Coordinates(x, y);
                            guard10Patrol.Add(patrolPoint);
                            currentChar = SymbolsConfig.EmptySpace;
                            break;
                        case '$':
                            totalGold += 100;
                            break;
                    }
                    grid[y, x] = currentChar.ToString();
                }
            }

            leverA.AssignGates(leverAGates.ToArray());
            leverE.AssignGates(leverEGates.ToArray());
            leverI.AssignGates(leverIGates.ToArray());
            leverO.AssignGates(leverOGates.ToArray());
            leverU.AssignGates(leverUGates.ToArray());
            leverY.AssignGates(leverYGates.ToArray());

            guard1.AssignPatrol(ArrangePatrolPoints(guard1, guard1Patrol).ToArray());
            guard2.AssignPatrol(ArrangePatrolPoints(guard2, guard2Patrol).ToArray());
            guard3.AssignPatrol(ArrangePatrolPoints(guard3, guard3Patrol).ToArray());
            guard4.AssignPatrol(ArrangePatrolPoints(guard4, guard4Patrol).ToArray());
            guard5.AssignPatrol(ArrangePatrolPoints(guard5, guard5Patrol).ToArray());
            guard6.AssignPatrol(ArrangePatrolPoints(guard6, guard6Patrol).ToArray());
            guard7.AssignPatrol(ArrangePatrolPoints(guard7, guard7Patrol).ToArray());
            guard8.AssignPatrol(ArrangePatrolPoints(guard8, guard8Patrol).ToArray());
            guard9.AssignPatrol(ArrangePatrolPoints(guard9, guard9Patrol).ToArray());
            guard10.AssignPatrol(ArrangePatrolPoints(guard10, guard10Patrol).ToArray());

            LevelInfo levelInfo = new LevelInfo(grid, hasKey, playerStartX, playerStartY, totalGold, leversDictionary, levelGuards.ToArray());

            return levelInfo;
        }

        public static List<Coordinates> ArrangePatrolPoints(Guard guard, List<Coordinates> guardPatrol)
        {
            Coordinates startPoint = new Coordinates(guard.X, guard.Y);

            List<Coordinates> arrangedPatrolPoints = new List<Coordinates>();

            while (arrangedPatrolPoints.Count < guardPatrol.Count)
            {
                int currentMinDistance = 1000;
                int closestPatrolX = 0;
                int closestPatrolY = 0;

                for (int i = 0; i < guardPatrol.Count; i++)
                {
                    bool alreadyAdded = false;

                    foreach (Coordinates c in arrangedPatrolPoints)
                    {
                        if (c.X == guardPatrol[i].X && c.Y == guardPatrol[i].Y)
                        {
                            alreadyAdded = true;
                        }
                    }

                    if (alreadyAdded) { continue; }

                    int xDifference = Math.Abs(guardPatrol[i].X - startPoint.X);
                    int yDifference = Math.Abs(guardPatrol[i].Y - startPoint.Y);

                    if (xDifference > 0 && yDifference > 0) { continue; }

                    if (xDifference == 0 && yDifference == 0) { continue; }

                    if (xDifference == 0)
                    {
                        if (yDifference <= currentMinDistance)
                        {
                            currentMinDistance = yDifference;

                            closestPatrolX = guardPatrol[i].X;
                            closestPatrolY = guardPatrol[i].Y;
                        }
                    }

                    if (yDifference == 0)
                    {
                        if (xDifference <= currentMinDistance)
                        {
                            currentMinDistance = xDifference;

                            closestPatrolX = guardPatrol[i].X;
                            closestPatrolY = guardPatrol[i].Y;
                        }
                    }
                }

                Coordinates closestPatrolPoint = new Coordinates(closestPatrolX, closestPatrolY);
                arrangedPatrolPoints.Add(closestPatrolPoint);
                startPoint = closestPatrolPoint;
            }

            return arrangedPatrolPoints;
        }
    }

    class LevelInfo
    {
        public string[,] Grid { get; }
        public bool HasKey { get; }
        public int PlayerStartX { get; }
        public int PlayerStartY { get; }
        public int TotalGold { get; }
        public Dictionary<Coordinates, Lever> LeversDictionary { get; }
        public Guard[] Guards { get; }

        public LevelInfo(string[,] grid, bool hasKey, int playerStartX, int playerStartY, int totalGold, Dictionary<Coordinates, Lever> leversDictionary, Guard[] guards)
        {
            Grid = grid;
            HasKey = hasKey;
            PlayerStartX = playerStartX;
            PlayerStartY = playerStartY;
            TotalGold = totalGold;
            LeversDictionary = leversDictionary;
            Guards = guards;
        }
    }
}
