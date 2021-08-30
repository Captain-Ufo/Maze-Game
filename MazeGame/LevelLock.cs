using System.Collections.Generic;

namespace MazeGame
{
    /// <summary>
    /// Handles the keys that unlock the level exit
    /// </summary>
    class LevelLock
    {
        private int RevealedKeyPieces = 0;

        private List<Coordinates> HiddenkeyPieces;

        public LevelLock()
        {
            HiddenkeyPieces = new List<Coordinates>();
        }

        /// <summary>
        /// Collects the key piece, by removing it from the map and, if necessary, spawning the next key piece
        /// </summary>
        /// <param name="world">The current level</param>
        /// <param name="x">The X coordinate of the piece the player is collecting</param>
        /// <param name="y">The X coordinate of the piece the player is collecting</param>
        /// <returns>returns whether the level is still locked or not (so true if there are other pieces to collect, false if there are none)</returns>
        public bool CollectKeyPiece(World world, int x, int y)
        {
            world.ChangeElementAt(x, y, SymbolsConfig.EmptySpace.ToString());

            RevealedKeyPieces--;

            if (RevealedKeyPieces <= 0)
            {
                if (HiddenkeyPieces.Count > 0)
                {
                    Coordinates nextPiece = HiddenkeyPieces[0];
                    world.ChangeElementAt(nextPiece.X, nextPiece.Y, SymbolsConfig.KeyChar.ToString(), false);
                    RevealedKeyPieces++;
                    HiddenkeyPieces.RemoveAt(0);
                    return true;
                }
                return false;
            }

            return true;
        }

        public bool IsLocked()
        {
            return RevealedKeyPieces > 0;
        }

        /// <summary>
        /// Increases the revelaed key pieces counter
        /// </summary>
        public void AddRevealedKeyPiece()
        {
            RevealedKeyPieces++;
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
            Coordinates pieceCoordinates = new Coordinates(x, y);
            HiddenkeyPieces.Insert(index, pieceCoordinates);
        }
    }
}
