using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using static System.Console;

namespace MazeGame
{
    /// <summary>
    /// The game itself. Contains the logic, the navigation menues, and the visuals
    /// </summary>
    class Game
    {
        private List<Level> levels;
        private bool playerHasBeenCaught;
        private bool hasDrawnBackground;
        private int totalGold;
        private string levelFilesPath;
        private string gameVersion = "1.5";
        private Menu mainMenu;
        private Menu bribeMenu;
        private SaveSystem saveSystem;
        
        public ChiptunePlayer TunePlayer { get; private set; }
        public Player MyPlayer { get; private set; }
        public Difficulty DifficultyLevel { get; private set; }
        public int CurrentRoom { get; private set; }
        public int TimesSpotted { get; set; }
        public int TimesCaught { get; private set; }
        public Stopwatch MyStopwatch { get; private set; }


        /// <summary>
        /// Initializes all the required elements and run the game
        /// </summary>
        public void Start()
        {
            saveSystem = new SaveSystem();
            TunePlayer = new ChiptunePlayer();
            MyStopwatch = new Stopwatch();

            playerHasBeenCaught = false;
            TimesCaught = 0;
            totalGold = 0;

            levelFilesPath = Directory.GetCurrentDirectory() + "/Levels";

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



        private void InstantiateGameEntities(string configFileDir, int startBooty, int startLevel)
        {
            levels = new List<Level>();

            string configFilePath = configFileDir +"/FloorsConfig.txt";

            string[] levelFiles = File.ReadAllLines(levelFilesPath + configFilePath);

            foreach (string levelFile in levelFiles)
            {
                string levelFilePath = levelFilesPath + "/" + configFileDir + "/" + levelFile + ".txt";

                string[] levelMap = File.ReadAllLines(levelFilePath);

                LevelInfo levelInfo = LevelParser.ParseFileToLevelInfo(levelMap, DifficultyLevel);

                levels.Add(new Level(levelFile, levelInfo.Grid, levelInfo.PlayerStartX, levelInfo.PlayerStartY, levelInfo.LevLock, levelInfo.Exit, 
                                     levelInfo.Treasures, levelInfo.LeversDictionary, levelInfo.Guards, MyStopwatch));

                totalGold += levelInfo.TotalGold;
            }

            MyPlayer = new Player(levels[startLevel].PlayerStartX, levels[startLevel].PlayerStartY);
            MyPlayer.Booty = startBooty;
        }



        private void InstantiateTutorialEntities(Tutorial tutorial)
        {
            levels = new List<Level>();

            for (int i = 0; i < tutorial.TutorialLevels.Length; i++)
            {
                LevelInfo levelInfo = LevelParser.ParseFileToLevelInfo(tutorial.TutorialLevels[i], DifficultyLevel);

                levels.Add(new Level("Tutorial " + (i + 1), levelInfo.Grid, levelInfo.PlayerStartX, levelInfo.PlayerStartY, levelInfo.LevLock, levelInfo.Exit,
                                     levelInfo.Treasures, levelInfo.LeversDictionary, levelInfo.Guards, MyStopwatch));
            }

            MyPlayer = new Player(levels[0].PlayerStartX, levels[0].PlayerStartY);
            MyPlayer.Booty = 0;
        }



        private void SetupConsole()
        {
            Title = "Heist!";

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
        private void PlayGame(int startRoom = 0, int startBooty = 0)
        {
            Clear();
            DisplayLoading();
            InstantiateGameEntities("/Baron's Jails", startBooty, startRoom);
            RunGameLoop(startRoom);
            WinGame();
        }



        private void PlayTutorial()
        {            
            Tutorial tutorial = new Tutorial();

            Clear();
            DisplayLoading();
            DifficultyLevel = Difficulty.VeryEasy;
            InstantiateTutorialEntities(tutorial);
            RunGameLoop(0, tutorial);
            tutorial.DisplayEndTutorial();
            RunMainMenu();
        }



        private void RunGameLoop(int startRoom, Tutorial tutorial = null)
        {
            MyStopwatch.Start();
            long timeAtPreviousFrame = MyStopwatch.ElapsedMilliseconds;

            Clear();

            CurrentRoom = startRoom;
            hasDrawnBackground = false;

            while (true)
            {
                MyPlayer.HasMoved = false;

                if (playerHasBeenCaught)
                {
                    break;
                }

                int deltaTimeMS = (int)(MyStopwatch.ElapsedMilliseconds - timeAtPreviousFrame);
                timeAtPreviousFrame = MyStopwatch.ElapsedMilliseconds;

                if (!HandleInputs(CurrentRoom, deltaTimeMS))
                {
                    return;
                }

                levels[CurrentRoom].UpdateGuards(deltaTimeMS, this);

                
                DrawFrame(CurrentRoom);

                if (tutorial != null)
                {
                    tutorial.DisplayTutorialInstructions(CurrentRoom);
                }

                string elementAtPlayerPosition = levels[CurrentRoom].GetElementAt(MyPlayer.X, MyPlayer.Y);

                if (elementAtPlayerPosition == SymbolsConfig.TreasureChar.ToString())
                {
                    TunePlayer.PlaySFX(1000, 100);
                    levels[CurrentRoom].ChangeElementAt(MyPlayer.X, MyPlayer.Y, SymbolsConfig.EmptySpace.ToString());
                    MyPlayer.Draw();
                    MyPlayer.Booty += 100;
                }
                else if (elementAtPlayerPosition == SymbolsConfig.KeyChar.ToString())
                {
                    TunePlayer.PlaySFX(800, 100);
                    levels[CurrentRoom].CollectKeyPiece(MyPlayer.X, MyPlayer.Y);
                    MyPlayer.Draw();
                }
                else if ((elementAtPlayerPosition == SymbolsConfig.LeverOffChar.ToString()
                    || elementAtPlayerPosition == SymbolsConfig.LeverOnChar.ToString())
                    && MyPlayer.HasMoved)
                {
                    TunePlayer.PlaySFX(100, 100);
                    levels[CurrentRoom].ToggleLever(MyPlayer.X, MyPlayer.Y);
                    MyPlayer.Draw();
                }
                else if (elementAtPlayerPosition == SymbolsConfig.ExitChar.ToString() && !levels[CurrentRoom].IsLocked)
                {
                    if (levels.Count > CurrentRoom + 1)
                    {
                        CurrentRoom++;
                        MyPlayer.SetStartingPosition(levels[CurrentRoom].PlayerStartX, levels[CurrentRoom].PlayerStartY);
                        hasDrawnBackground = false;

                        if (tutorial == null)
                        {
                            saveSystem.SaveGame(this);
                        }
                        Clear();
                    }
                    else
                    {
                        if (tutorial == null)
                        {
                            saveSystem.DeleteSaveGame(this);
                        }
                        break;
                    }
                }

                Thread.Sleep(20);
            }

            MyStopwatch.Stop();

            if (playerHasBeenCaught)
            {
                if (tutorial != null)
                {
                    tutorial.DisplayTutorialFail();
                    hasDrawnBackground = false;
                    playerHasBeenCaught = false;
                    TimesCaught = 0;
                    MyPlayer.SetStartingPosition(levels[CurrentRoom].PlayerStartX, levels[CurrentRoom].PlayerStartY);
                    levels[CurrentRoom].Reset();
                    RunGameLoop(CurrentRoom, tutorial);
                }
                else
                {
                    GameOver();
                }
                return;
            }
        }



        private bool HandleInputs(int currentLevel, int deltaTimeMS)
        {
            if (!MyPlayer.HandlePlayerControls(levels[currentLevel], deltaTimeMS))
            { 
                MyStopwatch.Stop();
                if (QuitGame())
                {
                    return false;
                }
                else
                {
                    Clear();
                    MyStopwatch.Start();
                    levels[currentLevel].Draw();
                    return true;
                }
            }
            return true;
        }



        private void DrawFrame(int currentRoom)
        {
            if (!hasDrawnBackground)
            {
                levels[currentRoom].Draw();
                MyPlayer.Draw();
                hasDrawnBackground = true;
            }
            levels[currentRoom].DrawGuards();
            DrawUI(currentRoom);
            CursorVisible = false;
        }



        private void DrawUI(int currentLevel)
        {
            int uiPosition = WindowHeight - 4;

            SetCursorPosition(0, uiPosition);

            WriteLine("___________________________________________________________________________________________________________________________________________________________________________________");
            WriteLine("");
            Write($"   {levels[currentLevel].Name}");
            SetCursorPosition(35, CursorTop);
            Write($"Difficulty Level: {DifficultyLevel}");
            SetCursorPosition(70, CursorTop);
            Write($"Tresure collected: $ {MyPlayer.Booty}");
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

            TimesCaught++;
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

            // No setting for Very Hard or Ironman because in the current iteration of the design, at those difficulty levels 
            // being caught means instant game over

            int bribeCost = 100 + (bribeCostIncrease * TimesCaught);

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
                @"        (.-~___     \.'|    | /-.__.-\|::::| //~      ,;;;;/;;;;;'            ",
                @"        /      ~~--._ \|   /   ______ `\:: |/       ,;;;;/;;;;;'              ",
                @"     .-|             ~~|   |  /''''''\ |:  |      ,;;;;/;;;;;' \              ",
                @"    /                   \  |  ~`'~~''~ |  /     ,;;;;/;;;;;'--__;             ",
                @"    /                   \  |  ~`'~~''~ |  /    ,;;;;/;;;;;'--__;              ",
                @"   (        \             \| `\.____./'|/    ,;;;;/;;;;;'      '\             ",
                @"  / \        \!             \888888888/    ,;;;;/;;;;;'     /    |            ",
                @" |      ___--'|              \8888888/   ,;;;;/;;;;;'      |     |            ",
                @"|`-._---       |               \888/   ,;;;;/;;;;;'              \            ",
                @"|             /                  °   ,;;;;/;;;;;'  \              \__________ ",
                @"(             )                 |  ,;;;;/;;;;;'      |        _.--~           ",
                @" \          \/ \              ,  ;;;;;/;;;;;'       /(     .-~_..--~~~~~~~~~~ ",
                @"  \__         '  `       ,     ,;;;;;/;;;;;'    .   /  \   / /~               ",
                @" /          \'  |`._______ ,;;;;;;/;;;;;;'     /   :   \/'/'       /|_/|   ``|",
                @"| _.-~~~~-._ |   \ __   .,;;;;;;/;;;;;;' ~~~~~'  .'    | |       /~ (/\/    ||",
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
                @"      `~~_ /                  |    \                |  `--------------------- ",
                @"                                                                              ",
                @"       |/ ~                `.  |    \         .     | O    __.-------------- -",
                @"                                                                              ",
                @"        |                   \ ;      \              | _.- ~                   ",
                @"                                                                              ",
                @"        |                    |        |             |  /  |                   ",
                @"                                                                              ",
                @"         |                   |         |            |/ '  |  RN TX            ",
            };

            foreach(string s in guardArt)
            {
                SetCursorPosition((WindowWidth / 3) - (s.Length / 2), CursorTop);
                WriteLine(s);
            }

            int xPos = (WindowWidth / 4) * 3;

            string[] prompt =
            {
                "'HALT!'",
                " ",
                "A guard caught you! Quick, maybe you can bribe them.",
                $"You have collected ${MyPlayer.Booty} so far.",
            };

            string[] options =
            {
                $"Bribe ($ {bribeCost})",
                "Surrender"
            };

            bribeMenu.UpdateMenuItems(prompt, options);

            int selectedIndex = bribeMenu.Run(xPos, 5, 2);

            switch (selectedIndex)
            {
                case 0:

                    string message;

                    if (MyPlayer.Booty >= bribeCost) 
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
                        MyPlayer.Booty -= bribeCost;
                        ReadKey(true);
                        return true;
                    }

                    if (MyPlayer.Booty > 0)
                    {
                        message = "The guard won't be swayed by the paltry sum you can offer.";
                    }
                    else
                    {
                        message = "You pockets are empty. The guard won't be swayed by words alone.";
                    }

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



        private void GameOver()
        {
            Clear();

            string[] gameOverArt =
            {
                @"                                                                            ",
                @"                                                                            ",
                @"       ____ __|__   ____    _ ________|__ _________ ______ _____            ",
                @"       |           |o  o|      |           |  \__      |                    ",
                @"                   | c)%,      |                 \     |                    ",
                @"                   |o__o'%                 |                       |        ",
                @"    ___|______________ _  %  ,mM  _________|__ ________|__ ____ ___|__      ",
                @"             |            %  |  n    |           |           |              ",
                @"             |      -__/   %  Y /    |           |   ____    |              ",
                @"             |      /       %J_]     |           |  |o  o|   |              ",
                @"   _ _____ __|__ ________|  / /  ____|__ _______    ,%(C | __|__  __ __     ",
                @"       |           |       / /             |        %o__o|         |        ",
                @"                          / /     ,-~~,      Mm,   %               |        ",
                @"       |          ____   / /    ,r/^V\,\    n  |  %    |           |        ",
                @"   ____|_______  |o  o|  \ \    ('_ ~ ( )   \ Y  %  ___|_______ ___|__ _    ",
                @"             |   | c)%|   \/\   ()--()-))    [_t%            |              ",
                @"             |   |o__%|   /  \   \ _(x)88     \ \            |              ",
                @"             |        %   \  |`-. \ _/|8       \ \           |   _/         ",
                @"   _ _____ __|__ ____  %   \ !  ,%J___]>---.____\ \  ________|___\_____     ",
                @"       |  \_       |    %,  \ `,% \  /   /'    (__/    |           |        ",
                @"       |    \      |     `%-%-%/|  \/   /  / =\|88                 |        ",
                @"                   |          | \      /   /888         |                    ",
                @"    ___|________ __|_______   |  '----'   |8  _________|_______ ___|__      ",
                @"             |           |     \          /8     |           |              ",
                @"             |           |     |         |8      |           |              ",
                @"             |           |     |         |8                  |              ",
                @"   _ _____ __|___ _______|____ /          \_ _    ________ __|__  ___ _     ",
                @"       |           |           J\:______/ \            |\__        |        ",
                @"                   |           |           |           | | \       |        ",
                @"       |           |          /            \           |           |        ",
                @"    ___|__ ________|_______  /     \_       \ _____ ___|__ ____ ___|__ _    ",
                @"             |           |  /      /88\      \   |           |              ",
                @"                         | /      /8   \      \  |           |              ",
                @"             |            /      /8  |  \      \                            ",
                @"   _ _____ __|__ ______  /      /8___|__ \      \  _ ________|__ _____ _    ",
                @"       |           |    /     .'8         '.     \     |           |        ",
                @"       | _         |   /     /8|            \     \    |                    ",
                @"       |  \_       |  /__ __/8 |           | \_____\   |           |        ",
                @"   ____|__/________  /   |888__|__ ____  __|__ 8|   \ _|_______ ___|__ _    ",
                @"             |      /   /8           |           \   \       |              ",
                @"                   /  .'8            |            '.  \      |              ",
                @"             |    /__/8  |           |             8\__\     |              ",
                @"     ____ __ |_  |  /8___|__ _____ __|__ ________|_ 8\  l __|__ _____ _     ",
                @"      |         /> /8          |           |         8\ <\         |        ",
                @" ____          />_/8           |   y       |          8\_<\         ____    ",
                @"|o  o|       ,%J__]            |   \       |           [__t %,     | o  o | ",
                @"| c)%,      ,%> )(8__ ___ ___  |___/___ ___| ______ ___8)(  <%,    _,% (c | ",
                @"| o__o`%-%-%' __ ]8                                     [ __  '%-%-%`o__o | ",
            };

            SetCursorPosition(0, 2);

            foreach (string s in gameOverArt)
            {
                SetCursorPosition(WindowWidth - s.Length - 6, CursorTop);
                WriteLine(s);
            }

            string[] gameOverOutro =
            {
                "Oh, no!",
                "You have been caught and brought back to your cell!",
                "  ",
                $"The guards searched you and sequestered $ {MyPlayer.Booty} in treasures.",
                "  ",
                "Thank you for playing.",
                "Try Again!",
                "  ",
                "  ",
                "÷ GAME OVER ÷",
            };

            SetCursorPosition(0, (WindowHeight / 3) - (gameOverOutro.Length / 2));

            for (int i = 0; i < gameOverOutro.Length; i++)
            {
                SetCursorPosition((WindowWidth / 4) - (gameOverOutro[i].Length / 2), CursorTop);
                if (i == gameOverOutro.Length - 1) { ForegroundColor = ConsoleColor.Red; }
                WriteLine(gameOverOutro[i]);
                ResetColor();
            }

            TunePlayer.PlayGameOverTune();

            if (CurrentRoom == 0 || DifficultyLevel == Difficulty.Easy || DifficultyLevel == Difficulty.Hard || DifficultyLevel == Difficulty.Ironman)
            {
                RestartMenu();
                ResetGame(true);
            }
            else if (DifficultyLevel == Difficulty.VeryEasy || DifficultyLevel == Difficulty.Normal || DifficultyLevel == Difficulty.VeryHard)
            {
                RetryMenu();
                ResetGame(false);
            }

            SetCursorPosition(0, WindowHeight - 2);
            Write("Press any key to continue...");
            ReadKey(true);
            TunePlayer.StopTune();
            DisplayAboutInfo();
        }



        private void WinGame()
        {
            TunePlayer.PlayGameWinTune();

            string[] outro =
            {
                "~·~ CONGRATULATIONS! ~·~",
                "  ",
                "  ",
                "You escaped the Baron's dungeon!",
                "  ",
                $"You collected $ {MyPlayer.Booty} in treasures, out of a total of $ {totalGold}.",
                $"You have been spotted {TimesSpotted} times, and caught {TimesCaught} times.",
                "  ",
                "  ",
                "  ",
                "  ",
                "Thank you for playing!"
            };

            if (TimesCaught > 0)
            {
                outro[7] = "Nevertheless, you always persuaded the guards to look the other way.";
            }
            else
            {
                outro[7] = "You fled the prison and the Baron's guards are none the wiser!";
                if (MyPlayer.Booty == totalGold)
                {
                    outro[8] = "You really are the best of all thieves in the city.";
                }
            }

            Clear();

            WriteLine(SymbolsConfig.OutroArt);

            SetCursorPosition(0, (WindowHeight / 3) - (outro.Length / 2) + 5);

            for (int i = 0; i < outro.Length; i++)
            {
                SetCursorPosition(((WindowWidth / 3) * 2) - (outro[i].Length / 2), CursorTop);
                if (i == 0) { ForegroundColor = ConsoleColor.Green; }
                WriteLine(outro[i]);
                ResetColor();
            }

            SetCursorPosition(0, WindowHeight - 2);
            WriteLine("Press any key to continue...");
            ReadKey(true);
            ResetGame(true);
            TunePlayer.StopTune();
            DisplayAboutInfo();
        }



        private void ResetGame(bool deleteSave)
        {
            playerHasBeenCaught = false;
            TimesCaught = 0;
            MyPlayer.Booty = 0;
            MyPlayer.SetStartingPosition(levels[0].PlayerStartX, levels[0].PlayerStartY);
            CurrentRoom = 0;
            totalGold = 0;
            foreach (Level level in levels)
            {
                level.Reset();
            }

            if (deleteSave)
            {
                saveSystem.DeleteSaveGame(this);
            }
        }
        #endregion



        #region Menus
        private void CreateMainMenu()
        {
            string[] prompt = {
                "                                                              $$$$$                                  ",
                "                                                              $:::$                                  ",
                "HHHHHHHHH     HHHHHHHHHEEEEEEEEEEEEEEEEEEEEEEIIIIIIIIII   $$$$$:::$$$$$$ TTTTTTTTTTTTTTTTTTTTTTT !!! ",
                "H:::::::H     H:::::::HE::::::::::::::::::::EI::::::::I $$::::::::::::::$T:::::::::::::::::::::T!!:!!",
                "H:::::::H     H:::::::HE::::::::::::::::::::EI::::::::I$:::::$$$$$$$::::$T:::::::::::::::::::::T!:::!",
                "HH::::::H     H::::::HHEE::::::EEEEEEEEE::::EII::::::II$::::$       $$$$$T:::::TT:::::::TT:::::T!:::!",
                "  H:::::H     H:::::H    E:::::E       EEEEEE  I::::I  $::::$            TTTTTT  T:::::T  TTTTTT!:::!",
                "  H:::::H     H:::::H    E:::::E               I::::I  $::::$                    T:::::T        !:::!",
                "  H::::::HHHHH::::::H    E::::::EEEEEEEEEE     I::::I  $:::::$$$$$$$$$           T:::::T        !:::!",
                "  H:::::::::::::::::H    E:::::::::::::::E     I::::I   $$::::::::::::$$         T:::::T        !:::!",
                "  H:::::::::::::::::H    E:::::::::::::::E     I::::I     $$$$$$$$$:::::$        T:::::T        !:::!",
                "  H::::::HHHHH::::::H    E::::::EEEEEEEEEE     I::::I              $::::$        T:::::T        !:::!",
                "  H:::::H     H:::::H    E:::::E               I::::I              $::::$        T:::::T        !!:!!",
                "  H:::::H     H:::::H    E:::::E       EEEEEE  I::::I  $$$$$       $::::$        T:::::T         !!! ",
                "HH::::::H     H::::::HHEE::::::EEEEEEEE:::::EII::::::II$::::$$$$$$$:::::$      TT:::::::TT           ",
                "H:::::::H     H:::::::HE::::::::::::::::::::EI::::::::I$::::::::::::::$$       T:::::::::T       !!! ",
                "H:::::::H     H:::::::HE::::::::::::::::::::EI::::::::I $$$$$$:::$$$$$         T:::::::::T      !!:!!",
                "HHHHHHHHH     HHHHHHHHHEEEEEEEEEEEEEEEEEEEEEEIIIIIIIIII      $:::$             TTTTTTTTTTT       !!! ",
                "                                                             $$$$$                                   "
            };

            string[] options = { "New Game", "Tutorial", "Credits ", "Quit" };

            mainMenu = new Menu(prompt, options);
        }



        private void CreateBribeMenu()
        {
            string[] prompt =
            {
                "A guard caught you! Quick, maybe you can bribe them.",
                "You have collected $0 so far.",
            };

            string[] options =
            {
                "Bribe ($ 0)",
                "Surrender"
            };

            bribeMenu = new Menu(prompt, options);
        }



        private void RunMainMenu()
        {
            Clear();
            string[] saveFiles = saveSystem.CheckForOngoingGames();

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
            string[] options = { "Continue", "New Game", "Tutorial", "Credits ", "Quit" };

            mainMenu.UpdateMenuOptions(options);

            int selectedIndex = mainMenu.Run(WindowWidth / 2, 10, 5);

            switch (selectedIndex)
            {
                case 0:
                    LoadSaveMenu(saveFiles);
                    break;
                case 1:
                    SelectDifficulty(true);
                    break;
                case 2:
                    PlayTutorial();
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
            string[] options = { "New Game", "Tutorial", "Credits ", "Quit" };

            mainMenu.UpdateMenuOptions(options);

            int selectedIndex = mainMenu.Run(WindowWidth / 2, 10, 5);

            switch (selectedIndex)
            {
                case 0:
                    SelectDifficulty(false);
                    break;
                case 1:
                    PlayTutorial();
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



        private void LoadSaveMenu(string[] availableSaves)
        {
            Clear();

            string prompt = "~·~ Which game do you want to load? ~·~";

            List<string> options = new List<string>();
            options.Add("Back");

            foreach (string s in availableSaves)
            {
                options.Add(s);
            }

            Menu loadSaveMenu = new Menu(prompt, options.ToArray());

            int selectedIndex = loadSaveMenu.Run(WindowWidth/2, 10, 2);

            switch (selectedIndex)
            {
                case 0:
                    Clear();
                    RunMainMenu();
                    break;
                default:
                    GameData saveGame = saveSystem.LoadGame(availableSaves[selectedIndex - 1]);
                    TimesSpotted = saveGame.TimesSpotted;
                    TimesCaught = saveGame.TimesCaught;
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
                "~·~ Choose your difficulty level ~·~",
                "  ",
                "The game will autosave your progress every time you complete a level. Only one savegame per difficulty level is possible.",
                "  ",
                "  ",
                "  ",
                "  ",
                "  ",
                "  ",
                "  "
            };

            if (IsThereASavegame)
            {
                prompt[3] = "! Warning: if you start a new game with the same difficulty level as an existing save, the save will be overwritten. !";
            }

            string[] options = { "Back", "Very Easy", "Easy", "Normal", "Hard", "Very Hard", "Ironman"};

            string[] vEasyPrompt = new string[prompt.Length];
            Array.Copy(prompt, vEasyPrompt, prompt.Length);
            vEasyPrompt[6] = "VERY EASY: you can bribe guards as many times as you want, if you have collected enough money to do it.";
            vEasyPrompt[7] = "Bribe cost increase by $50 each time. If you game over, you'll be able to reload the last save and retry.";

            string[] easyPrompt = new string[prompt.Length];
            Array.Copy(prompt, easyPrompt, prompt.Length);
            easyPrompt[6] = "EASY: same conditions as very easy, but if you game over, you'll have to start from the first level.";

            string[] normalPrompt = new string[prompt.Length];
            Array.Copy(prompt, normalPrompt, prompt.Length);
            normalPrompt[6] = "NORMAL: you can bribe each guard only once, after which they'll arrest you if they catch you a second time.";
            normalPrompt[7] = "Bribe cost will increase by $100 each time. If you game over, you can reload the last save and retry.";

            string[] hardPrompt = new string[prompt.Length];
            Array.Copy(prompt, hardPrompt, prompt.Length);
            hardPrompt[6] = "HARD: same conditions as normal, but if you game over, you'll have to start from the first level.";

            string[] vHardPrompt = new string[prompt.Length];
            Array.Copy(prompt, vHardPrompt, prompt.Length);
            vHardPrompt[6] = "VERY HARD: you cannot bribe guards at all. They'll arrest you on sight straight from the first time you'll cross their path.";
            vHardPrompt[7] = "You will still be able to load the last save and retry the same level.";

            string[] ironmanPrompt = new string[prompt.Length];
            Array.Copy(prompt, ironmanPrompt, prompt.Length);
            ironmanPrompt[6] = "IRONMAN: You cannot bribe guards at all, and if you get caught you'll have to start from the very beginning.";

            string[] defaultPrompt = prompt;

            string[][] promptsUpdates = new string[][]
            {
                defaultPrompt,
                vEasyPrompt,
                easyPrompt,
                normalPrompt,
                hardPrompt,
                vHardPrompt,
                ironmanPrompt,
            };

            Menu difficultyMenu = new Menu(prompt, options);

            int selectedIndex = difficultyMenu.RunWithUpdatingPrompt(WindowWidth / 2, 8, 0, promptsUpdates);

            switch (selectedIndex)
            {
                case 0:
                    RunMainMenu();
                    return;
                case 1:
                    DifficultyLevel = Difficulty.VeryEasy;
                    break;
                case 2:
                    DifficultyLevel = Difficulty.Easy;
                    break;
                case 3:
                    DifficultyLevel = Difficulty.Normal;
                    break;
                case 4:
                    DifficultyLevel = Difficulty.Hard;
                    break;
                case 5:
                    DifficultyLevel = Difficulty.VeryHard;
                    break;
                case 6:
                    DifficultyLevel = Difficulty.Ironman;
                    break;
            }

            PlayGame();
        }



        private void DisplayInstructions()
        {
            Clear();

            string[] backStory =
            {
                " ",
                "You are Gareth, the non-copyright infringing master thief.",
                "The last job you were tasked to do turned out to be a trap set up by the Watch.",
                "You have been captured and locked deep in the dungeon of the Baron's castle.",
                "They stripped you of all your gear, but they didn't check your clothes in depth. That was a mistake on their part.",
                "Had they searched you more throughly, they would've found the emergency lockpick sown inside the hem of your shirt.",
                "The meager lock of the cell's door is not going to resist it for too long.",
                "Escape the dungeon and, to spite the Watch, try to collect as much treasure as you can!"
            };
        }



        private void DisplayAboutInfo()
        {
            Clear();
            string authorName = "Cristian Baldi";
            string[] credits = new string[]
            {
                " ",
                " ",
                "~·~ CREDITS: ~·~",
                " ",
                " ",
                $"Escape from the Dungeon, a game by {authorName}",
                " ",
                $"Programming: {authorName}",
                "Shoutout to Micheal Hadley's \"Intro To Programming in C#\" course:",
                "https://www.youtube.com/channel/UC_x9TgYAIFHj1ulXjNgZMpQ",
                " ",
                $"Baron's Jail campaign level desing: {authorName}",
                "(I'm not a level designer :P)",
                " ",
                $"Chiptune Music: {authorName}",
                "(I'm not a musician either)",
                " ",
                " ",
                "~·~ ART: ~·~",
                " ",
                "Ascii title from Text To Ascii Art Generator (https://www.patorjk.com/software/taag)",
                " ",
                "Ascii art from Ascii Art Archive (https://www.asciiart.eu/):",
                "Guard art based on 'Orc' by Randall Nortman and Tua Xiong",
                "Win screen art by Henry Segerman",
                "Game over screen art based on art by Jgs"
            };

            foreach (string credit in credits)
            {
                for (int i = 0; i < credits.Length; i++)
                {
                    int cursorXoffset = credits[i].Length / 2;
                    SetCursorPosition((WindowWidth / 2) - cursorXoffset, WindowTop + i + 1);
                    WriteLine(credits[i]);
                }
            }

            SetCursorPosition(0, WindowHeight - 3);
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
                "Are you sure you want to quit?",
                "The game automatically saved the last level you played, but all your progress in the current level will be lost.",
             };
            string[] options = { "Quit to Main Menu", "Quit to desktop", "Return to game" };

            Menu quitMenu = new Menu(quitMenuPrompt, options);
            int selection = quitMenu.Run(WindowWidth/2, 10, 2);
            if (selection == 0)
            {
                RunMainMenu();
                return true;
            }
            if (selection == 1)
            {
                Environment.Exit(0);
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
                "Are you sure you want to quit?",
            };
            string[] options = { "Yes", "No" };

            Menu quitMenu = new Menu(quitMenuPrompt, options);
            int selection = quitMenu.Run(WindowWidth/2, 10, 2);
            if (selection == 0)
            {
                Environment.Exit(0);
                return true;
            }
            else
            {
                return false;
            }
        }



        private void RestartMenu()
        {
            string prompt = "Would you like to restart the game?";

            string[] options =
            {
                "Yes",
                "No",
            };

            Menu retryMenu = new Menu(prompt, options);

            int selectedIndex = retryMenu.Run(WindowWidth / 4, CursorTop + 2, 1);

            if (selectedIndex == 0)
            {
                TunePlayer.StopTune();
                ResetGame(true);
                RunGameLoop(0);
            }
        }

        
        
        private void RetryMenu()
        {
            string prompt = "Would you like to retry the last level?";

            string[] options =
            {
                "Yes",
                "No",
            };

            Menu retryMenu = new Menu(prompt, options);

            int selectedIndex = retryMenu.Run(WindowWidth / 4, CursorTop + 2, 1);

            if (selectedIndex == 0)
            {
                TunePlayer.StopTune();
                Retry();
            }
        }



        private void Retry()
        {
            playerHasBeenCaught = false;

            GameData saveGame = saveSystem.LoadGame(this);
            TimesSpotted = saveGame.TimesSpotted;
            TimesCaught = saveGame.TimesCaught;
            DifficultyLevel = saveGame.DifficultyLevel;
            MyPlayer.Booty = saveGame.Booty;
            MyPlayer.SetStartingPosition(levels[saveGame.CurrentLevel].PlayerStartX, levels[saveGame.CurrentLevel].PlayerStartY);
            levels[saveGame.CurrentLevel].Reset();
            RunGameLoop(saveGame.CurrentLevel);
        }
        #endregion;
    }



    public enum Difficulty { VeryEasy, Easy, Normal, Hard, VeryHard, Ironman }
}