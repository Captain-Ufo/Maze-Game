using System.Collections.Generic;

namespace MazeGame
{
    /// <summary>
    /// Handles the keys that unlock the level exit
    /// </summary>
    class LevelLock
    {
        private int revealedKeyPieces = 0;

        private List<Coordinates> hiddenKeyPieces;

        private List<Coordinates> revealedKeyPiecesBackup;
        private List<Coordinates> hiddenKeyPiecesBackUp;

        public LevelLock()
        {
            hiddenKeyPieces = new List<Coordinates>();

            revealedKeyPiecesBackup = new List<Coordinates>();
            hiddenKeyPiecesBackUp = new List<Coordinates>();
        }

        /// <summary>
        /// Collects the key piece, by removing it from the map and, if necessary, spawning the next key piece
        /// </summary>
        /// <param name="floor">The current level</param>
        /// <param name="x">The X coordinate of the piece the player is collecting</param>
        /// <param name="y">The X coordinate of the piece the player is collecting</param>
        /// <returns>returns whether the level is still locked or not (so true if there are other pieces to collect, false if there are none)</returns>
        public bool CollectKeyPiece(Floor floor, int x, int y)
        {
            floor.ChangeElementAt(x, y, SymbolsConfig.EmptySpace.ToString());

            revealedKeyPieces--;

            if (revealedKeyPieces <= 0)
            {
                if (hiddenKeyPieces.Count > 0)
                {
                    Coordinates nextPiece = hiddenKeyPieces[0];
                    floor.ChangeElementAt(nextPiece.X, nextPiece.Y, SymbolsConfig.KeyChar.ToString(), false);
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

            revealedKeyPiecesBackup.Add(new Coordinates(x, y));
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
                hiddenKeyPieces.Add(new Coordinates(0, 0));
            }

            hiddenKeyPieces[index] = new Coordinates(x, y);

            foreach (Coordinates c in hiddenKeyPieces)
            {
                hiddenKeyPiecesBackUp.Add(c);
            }
        }

        public void ResetKeys(Floor floor)
        {
            revealedKeyPieces = revealedKeyPiecesBackup.Count;

            foreach (Coordinates key in revealedKeyPiecesBackup)
            {
                floor.ChangeElementAt(key.X, key.Y, SymbolsConfig.KeyChar.ToString(), false, false);
            }

            hiddenKeyPieces = new List<Coordinates>();

            foreach (Coordinates key in hiddenKeyPiecesBackUp)
            {
                hiddenKeyPieces.Add(key);
                floor.ChangeElementAt(key.X, key.Y, SymbolsConfig.EmptySpace.ToString(), false, false);
            }
        }
    }
}
