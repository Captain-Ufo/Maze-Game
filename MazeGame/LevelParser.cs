using System;
using System.Collections.Generic;
using System.IO;

namespace MazeGame
{
    /// <summary>
    /// Contains the public static method that parses the level from the text file provided
    /// </summary>
    class LevelParser
    {
        /// <summary>
        /// Reads a text file and interprets all the informations required to create a level. It's a static method, so there's no need to instantiate
        /// a LevelParser to use it.
        /// </summary>
        /// <param name="filePath">The path of the file to parse the level from</param>
        /// <returns></returns>
        public static LevelInfo ParseFileToLevelInfo(string filePath)
        {
            //first, creating a whole bunch of variables that will hold the informations for the creation of the level

            string[] lines = File.ReadAllLines(filePath);
            string firstLine = lines[0];

            int rows = lines.Length;
            int columns = firstLine.Length;

            string[,] grid = new string[rows, columns];

            //bool hasKey = false;
            int playerStartX = 0;
            int playerStartY = 0;
            int totalGold = 0;

            LevelLock levLock = new LevelLock();

            Dictionary<Coordinates, Lever> leversDictionary = new Dictionary<Coordinates, Lever>();
            
            Lever leverA = new Lever();
            Lever leverE = new Lever();
            Lever leverI = new Lever();
            Lever leverO = new Lever();
            Lever leverU = new Lever();
            Lever leverY = new Lever();

            //these LUT dictionaries serve the sole purpose of making the massive switch block that parses the level
            //more succint and readable
            Dictionary<char, Lever> leversLUT = new Dictionary<char, Lever>
            {
                ['A'] = leverA,
                ['E'] = leverE,
                ['I'] = leverI,
                ['O'] = leverO,
                ['U'] = leverU,
                ['Y'] = leverY
            };

            List<Coordinates> leverAGates = new List<Coordinates>();
            List<Coordinates> leverEGates = new List<Coordinates>();
            List<Coordinates> leverIGates = new List<Coordinates>();
            List<Coordinates> leverOGates = new List<Coordinates>();
            List<Coordinates> leverUGates = new List<Coordinates>();
            List<Coordinates> leverYGates = new List<Coordinates>();

            Dictionary<char, List<Coordinates>> leverGatesLUT = new Dictionary<char, List<Coordinates>>
            {
                ['a'] = leverAGates,
                ['à'] = leverAGates,
                ['e'] = leverEGates,
                ['è'] = leverEGates,
                ['i'] = leverIGates,
                ['ì'] = leverIGates,
                ['o'] = leverOGates,
                ['ò'] = leverOGates,
                ['u'] = leverUGates,
                ['ù'] = leverUGates,
                ['y'] = leverYGates,
                ['ÿ'] = leverYGates
            };

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

            Dictionary<char, Guard> guardsLUT = new Dictionary<char, Guard>
            {
                ['B'] = guard1,
                ['C'] = guard2,
                ['D'] = guard3,
                ['F'] = guard4,
                ['G'] = guard5,
                ['J'] = guard6,
                ['K'] = guard7,
                ['L'] = guard8,
                ['M'] = guard9,
                ['N'] = guard10
            };

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

            Dictionary<char, List<Coordinates>> guardPatrolsLUT = new Dictionary<char, List<Coordinates>>
            {
                ['b'] = guard1Patrol,
                ['c'] = guard2Patrol,
                ['d'] = guard3Patrol,
                ['f'] = guard4Patrol,
                ['g'] = guard5Patrol,
                ['j'] = guard6Patrol,
                ['k'] = guard7Patrol,
                ['l'] = guard8Patrol,
                ['m'] = guard9Patrol,
                ['n'] = guard10Patrol
            };

            //Looping through every single character in the grid to find special characters for special gameplay elements 
            //(keys, treasures, levers, guards), and in the end create a bidimensional string array that will be the grid
            //used by the game to display the level.
            //When the switch catches a special characters, it replaces it in the grid with the appropriate representation

            for (int y = 0; y < rows; y++)
            {
                string line = lines[y];
                for (int x = 0; x < columns; x++)
                {
                    char currentChar = line[x];

                    Coordinates leverGate;
                    Coordinates patrolPoint;

                    switch (currentChar)
                    {
                        //player spawn point
                        case SymbolsConfig.SpawnChar:
                            playerStartX = x;
                            playerStartY = y;
                            currentChar = SymbolsConfig.EmptySpace;
                            break;
                        //keys
                        case SymbolsConfig.KeyChar:
                            levLock.AddRevealedKeyPiece();
                            break;
                        case '1':
                        case '2':
                        case '3':
                            currentChar = SymbolsConfig.KeyChar;
                            levLock.AddRevealedKeyPiece();
                            break;
                        case '¹':
                            currentChar = SymbolsConfig.EmptySpace;
                            levLock.AddHiddenKeyPiece(x, y, 0);
                            break;
                        case '²':
                            currentChar = SymbolsConfig.EmptySpace;
                            levLock.AddHiddenKeyPiece(x, y, 1);
                            break;
                        case '³':
                            currentChar = SymbolsConfig.EmptySpace;
                            levLock.AddHiddenKeyPiece(x, y, 2);
                            break;
                        //treasures
                        case '$':
                            totalGold += 100;
                            break;
                        //levers
                        case 'A':
                        case 'E':
                        case 'I':
                        case 'O':
                        case 'U':
                        case 'Y':
                            leversLUT[currentChar].SetLeverCoordinates(x, y);
                            Coordinates leverCoord = new Coordinates(x, y);
                            leversDictionary[leverCoord] = leversLUT[currentChar];
                            currentChar = SymbolsConfig.LeverOffChar;
                            break;
                        //lever gates that are closed when the game begins
                        case 'a':
                        case 'e':
                        case 'i':
                        case 'o':
                        case 'u':
                        case 'y':
                            leverGate = new Coordinates(x, y);
                            leverGatesLUT[currentChar].Add(leverGate);
                            currentChar = SymbolsConfig.GateChar;
                            break;
                        //lever gates that are open when the game begins
                        case 'à':
                        case 'è':
                        case 'ì':
                        case 'ò':
                        case 'ù':
                        case 'ÿ':
                            leverGate = new Coordinates(x, y);
                            leverGatesLUT[currentChar].Add(leverGate);
                            currentChar = SymbolsConfig.EmptySpace;
                            break;
                        //guards
                        case 'B':
                        case 'C':
                        case 'D':
                        case 'F':
                        case 'G':
                        case 'J':
                        case 'K':
                        case 'L':
                        case 'M':
                        case 'N':
                            guardsLUT[currentChar].AssignOriginPoint(x, y);
                            levelGuards.Add(guardsLUT[currentChar]);
                            currentChar = SymbolsConfig.EmptySpace;
                            break;
                        //guard patrols
                        case 'b':
                        case 'c':
                        case 'd':
                        case 'f':
                        case 'g':
                        case 'j':
                        case 'k':
                        case 'l':
                        case 'm':
                        case 'n':
                            patrolPoint = new Coordinates(x, y);
                            guardPatrolsLUT[currentChar].Add(patrolPoint);
                            currentChar = SymbolsConfig.EmptySpace;
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

            LevelInfo levelInfo = new LevelInfo(grid, playerStartX, playerStartY, totalGold, levLock, leversDictionary, levelGuards.ToArray());

            return levelInfo;
        }

        /// <summary>
        /// This rearranges the patrol points provided so that they are in the correct sequence from the closest to the farthest
        /// </summary>
        /// <param name="guard">The guard that follows the patrol</param>
        /// <param name="guardPatrol">The list of patrol points</param>
        /// <returns></returns>
        public static List<Coordinates> ArrangePatrolPoints(Guard guard, List<Coordinates> guardPatrol)
        {
            //I'm pretty sure the rearrange happens in the least effcient way possible (but that's what I could come up with)
            //by finding the closest one in an orthogonal direction compared to a starting point (which, at the beginning is the starting
            //position of the guard  - not included in the patrol points, so if the designer wants, the guard can start in a different position
            //than the path it will follow for the rest of the level, as long as it's orthogonal to at least one of the patrol points - and in
            //each iteration becomes the previously found patrol point.
            //the loop continues until the newly created lis includes as much entries as the original one.

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

    /// <summary>
    /// Just a helper class that holds all the information required to create a level
    /// </summary>
    class LevelInfo
    {
        public string[,] Grid { get; }
        public LevelLock LevLock { get; }
        public int PlayerStartX { get; }
        public int PlayerStartY { get; }
        public int TotalGold { get; }
        public Dictionary<Coordinates, Lever> LeversDictionary { get; }
        public Guard[] Guards { get; }

        public LevelInfo(string[,] grid, int playerStartX, int playerStartY, int totalGold, LevelLock levelLock, Dictionary<Coordinates, Lever> leversDictionary, Guard[] guards)
        {
            Grid = grid;
            LevLock = levelLock;
            PlayerStartX = playerStartX;
            PlayerStartY = playerStartY;
            TotalGold = totalGold;
            LeversDictionary = leversDictionary;
            Guards = guards;
        }
    }
}
