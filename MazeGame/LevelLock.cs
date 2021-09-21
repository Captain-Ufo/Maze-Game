using System.Collections.Generic;

namespace MazeGame
{
    /// <summary>
    /// Handles the keys that unlock the level exit
    /// </summary>
    class LevelLock
    {
        private int revealedKeyPieces = 0;
        private List<Vector2> hiddenKeyPieces;
        private List<Vector2> revealedKeyPiecesBackup;
        private List<Vector2> hiddenKeyPiecesBackUp;

        public LevelLock()
        {
            hiddenKeyPieces = new List<Vector2>();

            revealedKeyPiecesBackup = new List<Vector2>();
            hiddenKeyPiecesBackUp = new List<Vector2>();
        }

        /// <summary>
        /// Collects the key piece, by removing it from the map and, if necessary, spawning the next key piece
        /// </summary>
        /// <param name="level">The current level</param>
        /// <param name="x">The X coordinate of the piece the player is collecting</param>
        /// <param name="y">The X coordinate of the piece the player is collecting</param>
        /// <returns>returns whether the level is still locked or not (so true if there are other pieces to collect, false if there are none)</returns>
        public bool CollectKeyPiece(Level level, int x, int y)
        {
            level.ChangeElementAt(x, y, SymbolsConfig.EmptySpace.ToString());

            revealedKeyPieces--;

            if (revealedKeyPieces <= 0)
            {
                if (hiddenKeyPieces.Count > 0)
                {
                    Vector2 nextPiece = hiddenKeyPieces[0];
                    level.ChangeElementAt(nextPiece.X, nextPiece.Y, SymbolsConfig.KeyChar.ToString(), false);
                    revealedKeyPieces++;
                    hiddenKeyPieces.RemoveAt(0);
                    return true;
                }
                return false;
            }

            return true;
        }

        public bool IsLocked()
        {
            return revealedKeyPieces > 0;
        }

        /// <summary>
        /// Increases the revelaed key pieces counter
        /// </summary>
        public void AddRevealedKeyPiece(int x, int y)
        {
            revealedKeyPieces++;

            revealedKeyPiecesBackup.Add(new Vector2(x, y));
        }

        /// <summary>
        /// Adds a key piece in the specified position in the piece list
        /// </summary>
        /// <param name="x">The X coordinate of the piece to add</param>
        /// <param name="y">The Y coordinate of the piece to add</param>
        /// <param name="index">The sequence order of the piece
        /// (use 1 for first position, and so on. The method automatically translates it to a correct list index)</param>
        public void AddHiddenKeyPiece(int x, int y, int index)
        {
            while (hiddenKeyPieces.Count < index + 1)
            {
                hiddenKeyPieces.Add(new Vector2(0, 0));
            }

            hiddenKeyPieces[index] = new Vector2(x, y);

            foreach (Vector2 c in hiddenKeyPieces)
            {
                hiddenKeyPiecesBackUp.Add(c);
            }
        }

        /// <summary>
        /// Resets the lock and all the keys to the state they are at the beginning of the level (uncollects keys, hides hidden keys, re-locks the exit, etc)
        /// </summary>
        /// <param name="level">The level the lock is in</param>
        public void ResetKeys(Level level)
        {
            revealedKeyPieces = revealedKeyPiecesBackup.Count;

            foreach (Vector2 key in revealedKeyPiecesBackup)
            {
                level.ChangeElementAt(key.X, key.Y, SymbolsConfig.KeyChar.ToString(), false, false);
            }

            hiddenKeyPieces = new List<Vector2>();

            foreach (Vector2 key in hiddenKeyPiecesBackUp)
            {
                hiddenKeyPieces.Add(key);
                level.ChangeElementAt(key.X, key.Y, SymbolsConfig.EmptySpace.ToString(), false, false);
            }
        }
    }
}
