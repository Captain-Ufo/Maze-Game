using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Text.Json;
using static System.Console;

namespace MazeGame
{
    class Game
    {
        private List<World> worlds;

        private Player player;
        public Player Player { get => player; }

        private bool playerHasBeenCaught;
        private bool hasDrawnBackground;
        private int timesCaught;

        private int totalGold;

        private int currentRoom;

        private string levelFilesPath;
        private string saveGamesPath;

        private string gameVersion = "1.5";

        private Menu mainMenu;
        private Menu bribeMenu;

        public Difficulty DifficultyLevel { get; private set; }
        public Stopwatch MyStopwatch { get; private set; }



        public void Start()
        {
            MyStopwatch = new Stopwatch();
            playerHasBeenCaught = false;
            timesCaught = 0;
            totalGold = 0;

            levelFilesPath = Directory.GetCurrentDirectory() + "\\Levels";
            saveGamesPath = Directory.GetCurrentDirectory() + "\\Saves";

            SetupConsole();
            CreateMainMenu();
            CreateBribeMenu();
            RunMainMenu();
        }



        #region SetUp
        private void DisplayLoading()
        {
            string loadingText = "...Loading...";
            int posY = WindowHeight / 2;
            int halfX = WindowWidth / 2;
            int textOffset = loadingText.Length / 2;
            int posX = halfX - textOffset;

            SetCursorPosition(posX, posY);
            WriteLine(loadingText);
        }



        private void InstantiateEntities(int startBooty, int startLevel)
        {
            worlds = new List<World>();

            string[] levelFiles = File.ReadAllLines(levelFilesPath + "\\FloorsConfig.txt");

            foreach (string levelFile in levelFiles)
            {
                string levelFilePath = levelFilesPath + "\\" + levelFile + ".txt";

                LevelInfo levelInfo = LevelParser.ParseFileToLevelInfo(levelFilePath);

                worlds.Add(new World(levelInfo.Grid, levelInfo.HasKey, levelInfo.PlayerStartX, levelInfo.PlayerStartY, 
                                     levelInfo.LeversDictionary, levelInfo.Guards, MyStopwatch));

                totalGold += levelInfo.TotalGold;
            }

            player = new Player(worlds[startLevel].PlayerStartX, worlds[startLevel].PlayerStartY);
            player.Booty = startBooty;
        }



        private void SetupConsole()
        {
            Title = "Escape From The Dungeon";

            try
            {
                SetWindowSize(180, 56);
            }
            catch (ArgumentOutOfRangeException)
            {
                DisplayConsoleSizeWarning();
            }
        }



        private void DisplayConsoleSizeWarning()
        {
            WriteLine("Error setting the preferred console size.");
            WriteLine("You can continue using the program, but glitches may occour, and it will probably not be displayed correctly.");
            WriteLine("To fix this error, please try changing the character size of the Console.");

            WriteLine("\n\nPress any key to continue...");
            ReadKey(true);
        }
        #endregion



        #region Game
        private void PlayGame(int startRoom, int startBooty = 0)
        {
            Clear();
            DisplayLoading();
            InstantiateEntities(startBooty, startRoom);
            RunGameLoop(startRoom);
        }



        private void RunGameLoop(int startRoom)
        {
            MyStopwatch.Start();
            long timeAtPreviousFrame = MyStopwatch.ElapsedMilliseconds;

            Clear();

            currentRoom = startRoom;
            hasDrawnBackground = false;

            while (true)
            {
                player.HasPlayerMoved = false;

                if (playerHasBeenCaught)
                {
                    break;
                }

                int deltaTimeMS = (int)(MyStopwatch.ElapsedMilliseconds - timeAtPreviousFrame);
                timeAtPreviousFrame = MyStopwatch.ElapsedMilliseconds;

                if (!HandlePlayerInputs(currentRoom))
                {
                    return;
                }

                worlds[currentRoom].UpdateGuards(deltaTimeMS, this);

                hasDrawnBackground = DrawFrame(currentRoom, hasDrawnBackground);

                string elementAtPlayerPosition = worlds[currentRoom].GetElementAt(player.X, player.Y);

                if (elementAtPlayerPosition == SymbolsConfig.TreasureChar.ToString())
                {  
                    worlds[currentRoom].ChangeElementAt(player.X, player.Y, SymbolsConfig.EmptySpace.ToString());
                    Beep(1000, 80);
                    player.Booty += 100;
                }
                else if (elementAtPlayerPosition == SymbolsConfig.KeyChar.ToString())
                {
                    worlds[currentRoom].IsLocked = false;
                    Beep(800, 90);
                    worlds[currentRoom].ChangeElementAt(player.X, player.Y, SymbolsConfig.EmptySpace.ToString());
                }
                else if ((elementAtPlayerPosition == SymbolsConfig.LeverOffChar.ToString()
                    || elementAtPlayerPosition == SymbolsConfig.LeverOnChar.ToString())
                    && player.HasPlayerMoved)
                {
                    worlds[currentRoom].ToggleLevers(player.X, player.Y);
                }
                else if (elementAtPlayerPosition == SymbolsConfig.ExitChar.ToString() && !worlds[currentRoom].IsLocked)
                {
                    if (worlds.Count > currentRoom + 1)
                    {
                        currentRoom++;
                        player.X = worlds[currentRoom].PlayerStartX;
                        player.Y = worlds[currentRoom].PlayerStartY;
                        hasDrawnBackground = false;

                        GameData data = new GameData(player.Booty, currentRoom, timesCaught, DifficultyLevel);
                        SaveGame(data);
                        Clear();
                    }
                    else
                    {
                        DeleteSaveGame();
                        break;
                    }
                }

                Thread.Sleep(20);
            }

            if (playerHasBeenCaught)
            {
                GameOver();
                return;
            }

            DisplayOutro();
        }



        private bool HandlePlayerInputs(int currentLevel)
        {
            ConsoleKey key;

            if (KeyAvailable)
            {
                ConsoleKeyInfo keyInfo = ReadKey(true);
                key = keyInfo.Key;

                switch (key)
                {
                    case ConsoleKey.UpArrow:
                    case ConsoleKey.W:
                    case ConsoleKey.NumPad8:
                        if (worlds[currentLevel].IsPositionWalkable(player.X, player.Y - 1))
                        {
                            player.ClearPlayer(worlds[currentLevel].GetElementAt(player.X, player.Y));
                            player.Y--;
                            player.HasPlayerMoved = true;
                        }
                        return true;
                    case ConsoleKey.DownArrow:
                    case ConsoleKey.S:
                    case ConsoleKey.NumPad2:
                        if (worlds[currentLevel].IsPositionWalkable(player.X, player.Y + 1))
                        {
                            player.ClearPlayer(worlds[currentLevel].GetElementAt(player.X, player.Y));
                            player.Y++;
                            player.HasPlayerMoved = true;
                        }
                        return true;
                    case ConsoleKey.LeftArrow:
                    case ConsoleKey.A:
                    case ConsoleKey.NumPad4:
                        if (worlds[currentLevel].IsPositionWalkable(player.X - 1, player.Y))
                        {
                            player.ClearPlayer(worlds[currentLevel].GetElementAt(player.X, player.Y));
                            player.X--;
                            player.HasPlayerMoved = true;
                        }
                        return true;
                    case ConsoleKey.RightArrow:
                    case ConsoleKey.D:
                    case ConsoleKey.NumPad6:
                        if (worlds[currentLevel].IsPositionWalkable(player.X + 1, player.Y))
                        {
                            player.ClearPlayer(worlds[currentLevel].GetElementAt(player.X, player.Y));
                            player.X++;
                            player.HasPlayerMoved = true;
                        }
                        return true;
                    case ConsoleKey.Escape:
                        MyStopwatch.Stop();
                        if (QuitGame())
                        {
                            return false;
                        }
                        else
                        {
                            Clear();
                            MyStopwatch.Start();
                            worlds[currentLevel].Draw();
                            return true;
                        }
                    default:
                        return true;
                }
            }
            return true;
        }



        private bool DrawFrame(int currentRoom, bool hasDrawnBackground)
        {
            if (!hasDrawnBackground)
            {
                worlds[currentRoom].Draw();
                hasDrawnBackground = true;
            }
            worlds[currentRoom].DrawGuards();
            player.Draw();
            DrawUI(currentRoom);
            CursorVisible = false;
            return hasDrawnBackground;
        }



        private void DrawUI(int currentLevel)
        {
            int uiPosition = WindowHeight - 4;

            SetCursorPosition(0, uiPosition);

            WriteLine("___________________________________________________________________________________________________________________________________________________________________________________");
            WriteLine("");
            Write($"  Tresure collected: $ {player.Booty}");
            SetCursorPosition(32, CursorTop);
            Write($"Floor {currentLevel + 1}");
            SetCursorPosition(45, CursorTop);
            Write($"Difficulty Level: {DifficultyLevel}");
            string quitInfo = "Press Escape to quit.";
            SetCursorPosition(WindowWidth - quitInfo.Length - 3, WindowHeight - 2);
            Write(quitInfo);
        }



        public void CapturePlayer(Guard guard)
        {
            MyStopwatch.Stop();

            if (DifficultyLevel == Difficulty.VeryHard || DifficultyLevel == Difficulty.Ironman || guard.HasBeenBribedBefore || !AttemptBribe())
            {
                playerHasBeenCaught = true;
                return;
            }

            guard.BribeGuard(DifficultyLevel == Difficulty.Easy || DifficultyLevel == Difficulty.VeryEasy);

            timesCaught++;
            Clear();
            hasDrawnBackground = false;

            MyStopwatch.Start();
        }



        private bool AttemptBribe()
        {
            Clear();
            SetCursorPosition(0, 3);

            int bribeCostIncrease = 50;

            if (DifficultyLevel == Difficulty.Normal || DifficultyLevel == Difficulty.Hard)
            {
                bribeCostIncrease = 100;
            }

            //No setting for Very Hard or Ironman because in the current iteration of the design, at those difficulty levels 
            //being caught means instant game over

            int bribeCost = 100 + (bribeCostIncrease * timesCaught);

            string[] guardArt =
            {
                @"                           __.--|~|--.__                               ,,;/;  ",
                @"                         /~     | |    ;~\                          ,;;;/;;'  ",
                @"                        /|      | |    ;~\\                      ,;;;;/;;;'   ",
                @"                       |/|      \_/   ;;;|\                    ,;;;;/;;;;'    ",
                @"                       |/ \          ;;;/  )                 ,;;;;/;;;;;'     ",
                @"                   ___ | ______     ;_____ |___....__      ,;;;;/;;;;;'       ",
                @"             ___.-~ \\(| \  \.\ \__/ /./ /:|)~   ~   \   ,;;;;/;;;;;'         ",
                @"         /~~~    ~\    |  ~-.     |   .-~: |//  _.-~~--,;;;;/;;;;;'           ",
                @"        (.-~___     \.'|    | /-.__.-\|::::| //~     ,;;;;/;;;;;'             ",
                @"        /      ~~--._ \|   /   ______ `\:: |/      ,;;;;/;;;;;'               ",
                @"     .-|             ~~|   |  /''''''\ |:  |     ,;;;;/;;;;;' \               ",
                @"    /                   \  |  ~`'~~''~ |  /    ,;;;;/;;;;;'--__;              ",
                @"    /                   \  |  ~`'~~''~ |  /    ,;;;;/;;;;;'--__;              ",
                @"   (        \             \|`\._____./'|/    ,;;;;/;;;;;'      '\             ",
                @"  / \        \              \888888888/    ,;;;;/;;;;;'     /    |            ",
                @" |      ___--'|              \8888888/   ,;;;;/;;;;;'      |     |            ",
                @"|`-._---       |               \888/   ,;;;;/;;;;;'              \            ",
                @"|             /                  °   ,;;;;/;;;;;'  \              \__________ ",
                @"(             )                 |  ,;;;;/;;;;;'      |        _.--~           ",
                @" \          \/ \              ,  ;;;;;/;;;;;'       /(     .-~_..--~~~~~~~~~~ ",
                @"  \__         '  `       ,     ,;;;;;/;;;;;'    .   /  \   / /~               ",
                @" /          \'  |`._______ ,;;;;;;/;;;;;;'    /   :    \/'/'       /|_/|   ``|",
                @"| _.-~~~~-._ |   \ __   .,;;;;;;/;;;;;;' ~~~~'   .'    | |       /~ (/\/    ||",
                @"/~ _.-~~~-._\    /~/   ;;;;;;;/;;;;;;;'          |    | |       / ~/_-'|-   /|",
                @"(/~         \| /' |   ;;;;;;/;;;;;;;;            ;   | |       (.-~;  /-   / |",
                @"|            /___ `-,;;;;;/;;;;;;;;'            |   | |      ,/)  /  /-   /  |",
                @" \            \  `-.`---/;;;;;;;;;' |          _'   |T|    /'('  /  /|- _/  //",
                @"   \           /~~/ `-. |;;;;;''    ______.--~~ ~\  |u|  ,~)')  /   | \~-==// ",
                @"     \      /~(   `-\  `-.`-;   /|    ))   __-####\ |a|   (,   /|    |  \     ",
                @"       \  /~.  `-.   `-.( `-.`~~ /##############'~~)| |   '   / |    |   ~\   ",
                @"        \(   \    `-._ /~)_/|  /############'       |X|      /  \     \_\  `\ ",
                @"        ,~`\  `-._  / )#####|/############'   /     |i|  _--~ _/ | .-~~____--'",
                @"       ,'\  `-._  ~)~~ `################'           |o| ((~>/~   \ (((' -_    ",
                @"     ,'   `-.___)~~      `#############             |n|           ~-_     ~\_ ",
                @" _.,'        ,'           `###########              |g|            _-~-__    (",
                @"|  `-.     ,'              `#########       \       | |          ((.-~~~-~_--~",
                @"`\    `-.;'                  `#####' | | '    ((.-~~                          ",
                @"  `-._   )               \     |   |        .       |  \                 '    ",
                @"      `~~_ /                  |    \               |   `--------------------- ",
                @"                                                                              ",
                @"       |/ ~                `.  |     \        .     | O    __.-------------- -",
                @"                                                                              ",
                @"        |                   \ ;      \             | _.- ~                    ",
                @"                                                                              ",
                @"        |                    |        |            |  /  |                    ",
                @"                                                                              ",
                @"         |                   |         |           |/ '  |  RN TX             ",
            };

            foreach(string s in guardArt)
            {
                SetCursorPosition((WindowWidth / 3) - (s.Length / 2), CursorTop);
                WriteLine(s);
            }

            int xPos = (WindowWidth / 4) * 3;

            string[] prompt =
            {
                "A guard caught you! Quick, maybe you can bribe them.",
                $"You have collected ${player.Booty} so far.",
            };

            string[] options =
            {
                $"Bribe ($ {bribeCost})",
                "Surrender"
            };

            bribeMenu.UpdateMenuItems(prompt, options);

            int selectedIndex = bribeMenu.Run(xPos);

            switch (selectedIndex)
            {
                case 0:

                    string message;

                    if (player.Booty >= bribeCost) 
                    {
                        message = "The guard pockets your money and grumbles";
                        SetCursorPosition(xPos - message.Length/2, CursorTop + 4);
                        WriteLine(message);
                        message = "'I don't want to see your face around here again.'";
                        SetCursorPosition(xPos - message.Length / 2, CursorTop);
                        WriteLine(message);
                        message = "'I won't be so kind next time.'";
                        SetCursorPosition(xPos - message.Length / 2, CursorTop);
                        WriteLine(message);
                        player.Booty -= bribeCost;
                        ReadKey(true);
                        return true;
                    }

                    message = "The guard's request are too high for your pockets.";
                    SetCursorPosition(xPos - message.Length / 2, CursorTop + 4);
                    WriteLine(message);
                    ReadKey(true);
                    return false;

                case 1:
                    return false;

                default:
                    return false;
            }
        }
        #endregion



        #region Menus
        private void CreateMainMenu()
        {
            string[] prompt = {
                "                                                                  ",
                "       ▓█████   ██████  ▄████▄   ▄▄▄       ██▓███  ▓█████         ",
                "       ▓█   ▀ ▒██    ▒ ▒██▀ ▀█  ▒████▄    ▓██░  ██▒▓█   ▀         ",
                "       ▒███   ░ ▓██▄   ▒▓█    ▄ ▒██  ▀█▄  ▓██░ ██▓▒▒███           ",
                "       ▒▓█  ▄   ▒   ██▒▒▓▓▄ ▄██▒░██▄▄▄▄██ ▒██▄█▓▒ ▒▒▓█  ▄         ",
                "       ░▒████▒▒██████▒▒▒ ▓███▀ ░ ▓█   ▓██▒▒██▒ ░  ░░▒████▒        ",
                "       ░░ ▒░ ░▒ ▒▓▒ ▒ ░░ ░▒ ▒  ░ ▒▒   ▓▒█░▒▓▒░ ░  ░░░ ▒░ ░        ",
                "        ░ ░  ░░ ░▒  ░ ░  ░  ▒     ▒   ▒▒ ░░▒ ░      ░ ░  ░        ",
                "          ░   ░  ░  ░  ░          ░   ▒   ░░          ░           ",
                "          ░  ░      ░  ░ ░            ░  ░            ░  ░        ",
                "                    ░                                             ",
                "   █████▒██▀███   ▒█████   ███▄ ▄███▓    ▄▄▄█████▓ ██░ ██ ▓█████  ",
                " ▓██   ▒▓██ ▒ ██▒▒██▒  ██▒▓██▒▀█▀ ██▒    ▓  ██▒ ▓▒▓██░ ██▒▓█   ▀  ",
                " ▒████ ░▓██ ░▄█ ▒▒██░  ██▒▓██    ▓██░    ▒ ▓██░ ▒░▒██▀▀██░▒███    ",
                " ░▓█▒  ░▒██▀▀█▄  ▒██   ██░▒██    ▒██     ░ ▓██▓ ░ ░▓█ ░██ ▒▓█  ▄  ",
                " ░▒█░   ░██▓ ▒██▒░ ████▓▒░▒██▒   ░██▒      ▒██▒ ░ ░▓█▒░██▓░▒████▒ ",
                "  ▒ ░   ░ ▒▓ ░▒▓░░ ▒░▒░▒░ ░ ▒░   ░  ░      ▒ ░░    ▒ ░░▒░▒░░ ▒░ ░ ",
                "  ░       ░▒ ░ ▒░  ░ ▒ ▒░ ░  ░      ░        ░     ▒ ░▒░ ░ ░ ░  ░ ",
                "  ░ ░     ░░   ░ ░ ░ ░ ▒  ░      ░         ░       ░  ░░ ░   ░    ",
                "           ░         ░ ░         ░                 ░  ░  ░   ░  ░ ",
                "                     ░                                            ",
                "  ▓█████▄  █    ██  ███▄    █   ▄████ ▓█████  ▒█████   ███▄    █  ",
                "  ▒██▀ ██▌ ██  ▓██▒ ██ ▀█   █  ██▒ ▀█▒▓█   ▀ ▒██▒  ██▒ ██ ▀█   █  ",
                "  ░██   █▌▓██  ▒██░▓██  ▀█ ██▒▒██░▄▄▄░▒███   ▒██░  ██▒▓██  ▀█ ██▒ ",
                "  ░▓█▄   ▌▓▓█  ░██░▓██▒  ▐▌██▒░▓█  ██▓▒▓█  ▄ ▒██   ██░▓██▒  ▐▌██▒ ",
                "  ░▒████▓ ▒▒█████▓ ▒██░   ▓██░░▒▓███▀▒░▒████▒░ ████▓▒░▒██░   ▓██░ ",
                "   ▒▒▓  ▒ ░▒▓▒ ▒ ▒ ░ ▒░   ▒ ▒  ░▒   ▒ ░░ ▒░ ░░ ▒░▒░▒░ ░ ▒░   ▒ ▒  ",
                "   ░ ▒  ▒ ░░▒░ ░ ░ ░ ░░   ░ ▒░  ░   ░  ░ ░  ░  ░ ▒ ▒░ ░ ░░   ░ ▒░ ",
                "   ░ ░  ░  ░░░ ░ ░    ░   ░ ░ ░ ░   ░    ░   ░ ░ ░ ▒     ░   ░ ░  ",
                "     ░       ░              ░       ░    ░  ░    ░ ░           ░  ",
                "   ░                                                              ",
                "                                                                  ",
            };

            string[] options = { "New Game", "Instructions", "Credits", "Quit" };

            mainMenu = new Menu(prompt, options);
        }



        private void CreateBribeMenu()
        {
            string[] prompt =
            {
                "A guard caught you! Quick, maybe you can bribe them.",
                $"You have collected $0 so far.",
            };

            string[] options =
            {
                $"Bribe ($ 0)",
                "Surrender"
            };

            bribeMenu = new Menu(prompt, options);
        }



        private void RunMainMenu()
        {
            Clear();
            string[] saveFiles = CheckForOngoingGames();

            string gameVersionText = "Version " + gameVersion;

            SetCursorPosition(WindowWidth - 5 - gameVersionText.Length, WindowHeight - 2);
            WriteLine(gameVersionText);
            SetCursorPosition(0, 0);

            if (saveFiles.Length > 0)
            {
                MainMenuWithContinue(saveFiles);
            }
            else
            {
                DefaultMainMenu();
            }
        }



        private void MainMenuWithContinue(string[] saveFiles)
        {
            string[] options = { "Continue", "New Game", "Instructions", "Credits", "Quit" };

            mainMenu.UpdateMenuOptions(options);

            int selectedIndex = mainMenu.Run(WindowWidth / 2);

            switch (selectedIndex)
            {
                case 0:
                    LoadSaveMenu(saveFiles);
                    break;
                case 1:
                    SelectDifficulty(true);
                    break;
                case 2:
                    DisplayInstructions();
                    break;
                case 3:
                    DisplayAboutInfo();
                    break;
                case 4:
                    if (!MainMenuQuitGame())
                    {
                        RunMainMenu();
                    }
                    break;
            }
        }



        private void DefaultMainMenu()
        {
            string[] options = { "New Game", "Instructions", "Credits", "Quit" };

            mainMenu.UpdateMenuOptions(options);

            int selectedIndex = mainMenu.Run(WindowWidth / 2);

            switch (selectedIndex)
            {
                case 0:
                    SelectDifficulty(false);
                    break;
                case 1:
                    DisplayInstructions();
                    break;
                case 2:
                    DisplayAboutInfo();
                    break;
                case 3:
                    if (!MainMenuQuitGame())
                    {
                        RunMainMenu();
                    }
                    break;
            }
        }



        private string[] CheckForOngoingGames()
        {
            if (!Directory.Exists(saveGamesPath))
            {
                Directory.CreateDirectory(saveGamesPath);
            }

            string[] files = Directory.GetFiles(saveGamesPath);

            int extra = saveGamesPath.Length + 1; //+1 required to include the "\"

            List<string> saves = new List<string>();

            foreach (string file in files)
            {
                string fileName = file.Remove(0, extra);

                if (!fileName.Contains(".sav"))
                {
                    continue;
                }
                saves.Add(fileName);
            }

            return saves.ToArray();
        }



        private void LoadSaveMenu(string[] availableSaves)
        {
            Clear();

            string[] prompt =
            {
                "  ",
                "  ",
                "  ",
                "  ",
                "  ",
                "  ",
                "  ",
                "  ",
                "  ",
                "  ",
                "~·~ Which game do you want to load? ~·~",
                "  ",
                "  ",
            };

            List<string> options = new List<string>();
            options.Add("Back");

            foreach (string s in availableSaves)
            {
                options.Add(s);
            }

            Menu loadSaveMenu = new Menu(prompt, options.ToArray());

            int selectedIndex = loadSaveMenu.Run(WindowWidth/2);

            switch (selectedIndex)
            {
                case 0:
                    Clear();
                    RunMainMenu();
                    break;
                default:
                    GameData saveGame = LoadGame(availableSaves[selectedIndex - 1]);
                    timesCaught = saveGame.TimesCaught;
                    DifficultyLevel = saveGame.DifficultyLevel;
                    PlayGame(saveGame.CurrentLevel, saveGame.Booty);
                    break;
            }
        }



        private void SelectDifficulty(bool IsThereASavegame)
        {
            Clear();

            string[] prompt = 
            {
                "  ",
                "  ",
                "  ",
                "  ",
                "  ",
                "  ",
                "  ",
                "  ",
                "~·~ Choose your difficulty level ~·~",
                "  ",
                "  ",
                "  "
            };

            if (IsThereASavegame)
            {
                prompt[10] = "! Warning: if you start a new game with the same difficulty level as an existing save, the save will be overwritten. !";
            }

            string[] options = { "Very Easy", "Easy", "Normal", "Hard", "Very Hard", "Ironman", "Back"};

            Menu difficultyMenu = new Menu(prompt, options);

            int selectedIndex = difficultyMenu.Run(WindowWidth / 2);

            switch (selectedIndex)
            {
                case 0:
                    DifficultyLevel = Difficulty.VeryEasy;
                    break;
                case 1:
                    DifficultyLevel = Difficulty.Easy;
                    break;
                case 2:
                    DifficultyLevel = Difficulty.Normal;
                    break;
                case 3:
                    DifficultyLevel = Difficulty.Hard;
                    break;
                case 4:
                    DifficultyLevel = Difficulty.VeryHard;
                    break;
                case 5:
                    DifficultyLevel = Difficulty.Ironman;
                    break;
                case 6:
                    RunMainMenu();
                    return;
            }

            PlayGame(0);
        }



        private void DisplayInstructions()
        {
            Clear();

            string[] backStory =
            {
                " ",
                "You are Gareth, the master thief.",
                "The last job you were tasked to do turned out to be a trap set up by the Watch.",
                "You have been captured and locked deep in the dungeon of the Baron's castle.",
                "They stripped you of all your gear, but they didn't check your clothes in depth. That was a mistake on their part.",
                "Had they searched you more throughly, they would've found the emergency lockpick sown inside the hem of your shirt.",
                "The meager lock of the cell's door is not going to resist it for too long.",
                "Escape the dungeon and, to spite the Watch, try to collect as much treasure as you can!"
        };

            foreach(string s in backStory)
            {
                SetCursorPosition((WindowWidth / 2) - (s.Length / 2), CursorTop);
                WriteLine(s);
            }

            WriteLine("\n\n\n ~·~ INSTRUCTIONS: ~·~");
            WriteLine("\n > Use the arrow keys to move.");
            Write("\n > Try to reach the TRAPDOOR to the next level, which looks like this: ");
            ForegroundColor = ConsoleColor.Green;
            WriteLine(SymbolsConfig.ExitChar);
            ResetColor();
            Write("\n > Sometimes before you reach the door you need to find a KEY, which looks like this: ");
            ForegroundColor = ConsoleColor.DarkYellow;
            WriteLine(SymbolsConfig.KeyChar);
            ResetColor();
            Write("\n > Avoid at all costs GUARDS, which looks like this: ");
            ForegroundColor = ConsoleColor.DarkRed;
            WriteLine("@");
            ResetColor();
            WriteLine("   Beware, Guards can catch you even if they just walk by your position. They don't need to actually bump against you.");
            WriteLine("   Depending on the difficulty level you chose, you might be able to bribe them to look the other way. It will get more expansive the more you do it!");
            WriteLine("\n > LEVERS, which looks like this: " + SymbolsConfig.LeverOffChar + " or this " + SymbolsConfig.LeverOnChar + 
                      ", open GATES ( " +SymbolsConfig.GateChar + " )  in the current floor.");
            Write("\n > (Optional) Try to collect as much treasure as you can: ");
            ForegroundColor = ConsoleColor.Yellow;
            WriteLine(SymbolsConfig.TreasureChar);
            ResetColor();
            WriteLine("\n The game will autosave your progress every time you complete a level. Only one savegame per difficulty level is possible.");
            WriteLine("\n\n Difficulty levels:");
            WriteLine("\n > VERY EASY: you can bribe guards as many times as you want, if you have collected enough money to do it.");
            WriteLine("   Bribe cost increase by $50 each time. If you game over, you'll be able to reload the last save and retry.");
            WriteLine("\n > EASY: same conditions as very easy, but if you game over, you'll have to start from the first level.");
            WriteLine("\n > NORMAL: you can bribe each guard only once, after which they'll arrest you if they catch you a second time.");
            WriteLine("   Bribe cost will increase by $100 each time. If you game over, you can reload the last save and retry.");
            WriteLine("\n > HARD: same conditions as normal, but if you game over, you'll have to start from the first level.");
            WriteLine("\n > VERY HARD: you cannot bribe guards at all. They'll arrest you on sight straight from the first time you'll cross their path.");
            WriteLine("   You will still be able to load the last save and retry the same level.");
            WriteLine("\n > IRONMAN: You cannot bribe guards at all, and if you get caught you'll have to start from the very beginning.");

            SetCursorPosition(0, WindowHeight - 3);
            WriteLine("Press any key to return to the main menu...");
            ReadKey(true);
            Clear();
            RunMainMenu();
        }



        private void DisplayAboutInfo()
        {
            Clear();
            string authorName = "Cristian Baldi";
            WriteLine($" Escape from the Dungeon, a game by {authorName} expanding on the lessons in Micheal Hadley's \"Intro To Programming in C#\" course:");
            WriteLine(" https://www.youtube.com/channel/UC_x9TgYAIFHj1ulXjNgZMpQ");
            WriteLine($"\n Programming: {authorName}");
            WriteLine($"\n Level desing: {authorName}");
            WriteLine("  (Yes, I know it's bad. I'm not a level designer :P)");
            WriteLine("\n Ascii title from Text To Ascii Art Generator (https://www.patorjk.com/software/taag)");
            WriteLine(" Ascii art from Ascii Art Archive (https://www.asciiart.eu/):");
            WriteLine("\n Guard art by Randall Nortman and Tua Xiong");
            WriteLine("\n Win screen art by Henry Segerman");
            WriteLine("\n Game over screen art based on art by Jgs");
            WriteLine("\n Press any key to return to main menu...");
            ReadKey(true);
            Clear();
            RunMainMenu();
        }



        private bool QuitGame()
        {
            Clear();
            string[] quitMenuPrompt =
             {
                "  ",
                "  ",
                "  ",
                "  ",
                "  ",
                "  ",
                "  ",
                "  ",
                "  ",
                "  ",
                "Are you sure you want to quit?",
                "The game automatically saved the last level you played, but all your progress in the current level will be lost.",
                "  ",
                "  ",
             };
            string[] options = { "Yes", "No" };

            Menu quitMenu = new Menu(quitMenuPrompt, options);
            int selection = quitMenu.Run(WindowWidth/2);
            if (selection == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }



        private bool MainMenuQuitGame()
        {
            Clear();
            string[] quitMenuPrompt =
            {
                "  ",
                "  ",
                "  ",
                "  ",
                "  ",
                "  ",
                "  ",
                "  ",
                "  ",
                "  ",
                "Are you sure you want to quit?",
                "  ",
                "  ",
            };
            string[] options = { "Yes", "No" };

            Menu quitMenu = new Menu(quitMenuPrompt, options);
            int selection = quitMenu.Run(WindowWidth/2);
            if (selection == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion;



        private void DisplayOutro()
        {
            string[] outro =
            {
                "  ",
                "  ",
                "  ",
                "  ",
                "  ",
                "~·~ CONGRATULATIONS! ~·~",
                "  ",
                "  ",
                "You escaped the Baron's dungeon!",
                "  ",
                $"You collected $ {player.Booty} in treasures, out of a total of $ {totalGold}.",
                $"You have been caught {timesCaught} times, but you always managed to convince the guard to look the other way.",
                "  ",
                "Thank you for playing."
            };

            Clear();

            WriteLine(SymbolsConfig.OutroArt);

            SetCursorPosition(0, (WindowHeight / 3) - (outro.Length / 2));

            for (int i = 0; i < outro.Length; i++)
            {
                SetCursorPosition(((WindowWidth/3)*2) - (outro[i].Length / 2), CursorTop);
                if (i == 0) { ForegroundColor = ConsoleColor.Green; }
                WriteLine(outro[i]);
                ResetColor();
            }

            SetCursorPosition(0, WindowHeight - 2);
            WriteLine("Press any key to continue...");
            ReadKey(true);
            ResetGame(true);

            DisplayAboutInfo();
        }



        private void GameOver()
        {
            Clear();

            string[] gameOverOutro =
            {
                "Oh, no!",
                "You have been caught and brought back to your cell!",
                "  ",
                $"The guards searched you and sequestered $ {player.Booty} in treasures.",
                "  ",
                "Thank you for playing.",
                "Try Again!",
                "  ",
                "  ",
                "÷ GAME OVER ÷",
            };

            SetCursorPosition(0, (WindowHeight / 3) - (gameOverOutro.Length/2));

            for (int i = 0; i < gameOverOutro.Length; i++)
            {
                SetCursorPosition((WindowWidth / 4) - (gameOverOutro[i].Length / 2), CursorTop);
                if (i == gameOverOutro.Length - 1) { ForegroundColor = ConsoleColor.Red; }
                WriteLine(gameOverOutro[i]);
                ResetColor();
            }

            string[] gameOverArt =
            {
                "                                                                            ",
                "                                                                            ",
                "       ____ __|__   ____    _ ________|__ _________ ______ _____            ",
                "       |           |o  o|      |           |  \\__      |                    ",
                "                   | c)%,      |                 \\     |                    ",
                "                   |o__o'%                 |                       |        ",
                "    ___|______________ _  %  ,mM  _________|__ ________|__ ____ ___|__      ",
                "             |            %  |  n    |           |           |              ",
                "             |      -__/   %  Y /    |           |   ____    |              ",
                "             |      /       %J_]     |           |  |o  o|   |              ",
                "   _ _____ __|__ ________|  / /  ____|__ _______    ,%(C | __|__  __ __     ",
                "       |           |       / /             |        %o__o|         |        ",
                "                          / /     ,-~~,      Mm,   %               |        ",
                "       |          ____   / /    ,r/^V\\,\\    n  |  %    |           |        ",
                "   ____|_______  |o  o|  \\ \\    ('_ ~ ( )   \\ Y  %  ___|_______ ___|__ _    ",
                "             |   | c)%|   \\/\\   ()--()-))    [_t%            |              ",
                "             |   |o__%|   /  \\   \\ _(x)88     \\ \\            |              ",
                "             |        %   \\  !`-. \\ _/|8       \\ \\           |   _/         ",
                "   _ _____ __|__ ____  %   \\    ,%J___]>---.____\\ \\  ________|___\\_____     ",
                "       |  \\_       |    %,  \\ `,%         '    (__/    |           |        ",
                "       |    \\      |     `%-%-%/           / =\\|88                 |        ",
                "                   |          |           /888         |                    ",
                "    ___|________ __|_______   |           |8  _________|_______ ___|__      ",
                "             |           |     \\          /8     |           |              ",
                "             |           |     |         |8      |           |              ",
                "             |           |     |         |8                  |              ",
                "   _ _____ __|___ _______|____ /          \\_ _    ________ __|__  ___ _     ",
                "       |           |           J\\:______/ \\            |\\__        |        ",
                "                   |           |           |           | | \\       |        ",
                "       |           |          /            \\           |           |        ",
                "    ___|__ ________|_______  /     \\_       \\ _____ ___|__ ____ ___|__ _    ",
                "             |           |  /      /88\\      \\   |           |              ",
                "                         | /      /8   \\      \\  |           |              ",
                "             |            /      /8  |  \\      \\                            ",
                "   _ _____ __|__ ______  /      /8___|__ \\      \\  _ ________|__ _____ _    ",
                "       |           |    /     .'8         '.     \\     |           |        ",
                "       | _         |   /     /8|            \\     \\    |                    ",
                "       |  \\_       |  /__ __/8 |           | \\_____\\   |           |        ",
                "   ____|__/________  /   |888__|__ ____  __|__ 8|   \\ _|_______ ___|__ _    ",
                "             |      /   /8           |           \\   \\       |              ",
                "                   /  .'8            |            '.  \\      |              ",
                "             |    /__/8  |           |             8\\__\\     |              ",
                "     ____ __ |_  |  /8___|__ _____ __|__ ________|_ 8\\  l __|__ _____ _     ",
                "      |         /> /8          |           |         8\\ <\\         |        ",
                "  ____          />_/8           |   y       |          8\\_<\\         ____    ",
                " |o  o|       ,%J__]            |   \\       |           [__t %,     | o  o | ",
                " | c)%,      ,%> )(8__ ___ ___  |___/___ ___| ______ ___8)(  <%,    _,% (c | ",
                "| o__o`%-%-%' __ ]8                                     [ __  '%-%-%`o__o | ",
            };

            SetCursorPosition(0, 2);

            foreach (string s in gameOverArt)
            {
                SetCursorPosition(WindowWidth - s.Length - 6, CursorTop);
                WriteLine(s);
            }

            GameOverSong();

            if (DifficultyLevel == Difficulty.VeryEasy || DifficultyLevel == Difficulty.Normal || DifficultyLevel == Difficulty.Hard)
            {
                RetryMenu();
                ResetGame(false);
            }
            else
            {
                ResetGame(true);
            }

            SetCursorPosition(0, WindowHeight - 2);
            Write("Press any key to continue...");
            ReadKey(true);

            DisplayAboutInfo();
        }



        private void RetryMenu()
        {
            string[] prompt =
            {
                "  ",
                "  ",
                "Would you like to retry the last level?",
                "  "
            };

            string[] options =
            {
                "Yes",
                "No",
            };

            Menu retryMenu = new Menu(prompt, options);

            int selectedIndex = retryMenu.Run(WindowWidth / 4);

            switch (selectedIndex)
            {
                case 0:
                    Retry();
                    break;
                case 1:
                    break;
            }
        }



        private void Retry()
        {
            playerHasBeenCaught = false;

            string saveGameName = "\\" + DifficultyLevel + "_Game.sav";
            string saveFilePath = saveGamesPath + saveGameName;

            GameData saveGame = LoadGame(saveGameName);
            timesCaught = saveGame.TimesCaught;
            DifficultyLevel = saveGame.DifficultyLevel;
            player.Booty = saveGame.Booty;
            player.X = worlds[saveGame.CurrentLevel].PlayerStartX;
            player.Y = worlds[saveGame.CurrentLevel].PlayerStartY;
            RunGameLoop(saveGame.CurrentLevel);
        }



        private void ResetGame(bool deleteSave)
        {
            playerHasBeenCaught = false;
            timesCaught = 0;
            player.Booty = 0;
            currentRoom = 0;
            totalGold = 0;
            foreach (World world in worlds)
            {
                world.ResetGuards();
            }

            if (deleteSave)
            {
                string saveGameName = "\\" + DifficultyLevel + "_Game.sav";
                string saveFilePath = saveGamesPath + saveGameName;
                File.Delete(saveFilePath);
            }
        }



        private void SaveGame(GameData data)
        {
            string saveGame = JsonSerializer.Serialize(data);
            string saveGameName = "\\" + DifficultyLevel + "_Game.sav";
            if (!Directory.Exists(saveGamesPath))
            {
                Directory.CreateDirectory(saveGamesPath);
            }
            string saveFilePath = saveGamesPath + saveGameName;
            File.WriteAllText(saveFilePath, saveGame);
        }



        private GameData LoadGame(string saveFileToLoad)
        {
            if (!Directory.Exists(saveGamesPath))
            {
                throw new Exception("LoadGame - Save file directory does not exists!");
            }

            string fileToLoadPath = saveGamesPath + "\\" + saveFileToLoad;

            string loadedData = File.ReadAllText(fileToLoadPath);
            GameData data = JsonSerializer.Deserialize<GameData>(loadedData);

            return data;
        }



        private void DeleteSaveGame()
        {
            string saveGameName = "\\" + DifficultyLevel + "Game.sav";
            string saveFilePath = saveGamesPath + saveGamesPath;
            File.Delete(saveFilePath);
        }



        private void GameOverSong()
        {
            Beep(660, 1000);
            Beep(528, 1000);
            Beep(594, 1000);
            Beep(495, 1000);
            Beep(528, 1000);
            Beep(440, 1000);
            Beep(419, 1000);
            Beep(495, 1000);
            Beep(660, 1000);
            Beep(528, 1000);
            Beep(594, 1000);
            Beep(495, 1000);
            Beep(660, 500);
            Beep(528, 500);
            Beep(670, 1000);
            Beep(638, 2000);
        }
    }



    public enum Difficulty { VeryEasy, Easy, Normal, Hard, VeryHard, Ironman }



    public class GameData
    {
        public int Booty { get; set; }
        public int CurrentLevel { get; set; }
        public int TimesCaught { get; set; }
        public Difficulty DifficultyLevel { get; set; }



        public GameData (int booty, int currentLevel, int timesCaught, Difficulty difficultyLevel)
        {
            Booty = booty;
            CurrentLevel = currentLevel;
            TimesCaught = timesCaught;
            DifficultyLevel = difficultyLevel;
        }



        public GameData()
        {
        }
    }
}