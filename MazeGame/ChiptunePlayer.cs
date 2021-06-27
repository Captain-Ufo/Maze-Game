using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Console;

namespace MazeGame
{
    /// <summary>
    /// Plays some chiptunes using the Console.Beep() method.
    /// </summary>
    class ChiptunePlayer
    {
        CancellationTokenSource cts;

        public ChiptunePlayer()
        {
            cts = new CancellationTokenSource();
        }

        /// <summary>
        /// Stops whichever tune is currently playing
        /// </summary>
        public void StopTune()
        {
            cts.Cancel();
            cts.Dispose();
            cts = new CancellationTokenSource();
        }

        /// <summary>
        /// Plays (asyncronously, so it won't block player inputs and otehr gameplay) the game over tune
        /// </summary>
        public void PlayGameOverTune()
        {
            Task.Run(() => GameOverTune(cts.Token));

        }

        /// <summary>
        /// Plays (asyncronously, so it won't block player inputs and otehr gameplay) the win fanfare at the end of the game
        /// </summary>
        public void PlayGameWinTune()
        {
            Task.Run(() => GameWonTune(cts.Token));
        }

        private void GameWonTune(CancellationToken token)
        {
            Coordinates[] tune =
            {
                new Coordinates(540, 170),
                new Coordinates(590, 170),
                new Coordinates(640, 170),
                new Coordinates(690, 500),
                new Coordinates(640, 500),
                new Coordinates(590, 200),
                new Coordinates(640, 170),
                new Coordinates(690, 170),
                new Coordinates(740, 500),
                new Coordinates(690, 500),
                new Coordinates(640, 200),
                new Coordinates(690, 170),
                new Coordinates(740, 170),
                new Coordinates(790, 500),
                new Coordinates(740, 500),
                new Coordinates(790, 200),
                new Coordinates(840, 170),
                new Coordinates(890, 170),
                new Coordinates(940, 170),
                new Coordinates(990, 2000),
            };

            for (int i = 0; i < tune.Length; i++)
            {
                Beep(tune[i].X, tune[i].Y);

                if (token.IsCancellationRequested)
                {
                    return;
                }
            }
        }

        private void GameOverTune(CancellationToken token)
        {
            Coordinates[] tune =
            {
                new Coordinates(660, 1000),
                new Coordinates(528, 1000),
                new Coordinates(594, 1000),
                new Coordinates(495, 1000),
                new Coordinates(528, 1000),
                new Coordinates(440, 1000),
                new Coordinates(419, 1000),
                new Coordinates(495, 1000),
                new Coordinates(660, 1000),
                new Coordinates(528, 1000),
                new Coordinates(594, 1000),
                new Coordinates(495, 1000),
                new Coordinates(660, 500),
                new Coordinates(528, 500),
                new Coordinates(670, 1000),
                new Coordinates(638, 2000),
            };

            for (int i = 0; i < tune.Length; i++)
            {
                Beep(tune[i].X, tune[i].Y);

                if (token.IsCancellationRequested)
                {
                    return;
                }
            }
        }
    }
}
