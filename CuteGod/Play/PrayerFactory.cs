using C5;
using MfGames.Sprite3.PlanetCute;
using MfGames.Utility;
using System.IO;
using System.Xml;

namespace CuteGod.Play
{
    /// <summary>
    /// Encapsulates the functionality used to generate random prayers
    /// from the board. This loads files from the embedded resource,
    /// layouts.xml, and keeps them stored in memory.
    /// </summary>
    public class PrayerFactory
    {
        #region Constructors
        /// <summary>
        /// Loads the required data from the embedded resource and
        /// prepares the factory.
        /// </summary>
        public PrayerFactory()
        {
            // Get the manifest string
            Stream stream = GetType().Assembly
                .GetManifestResourceStream("CuteGod.layouts.xml");

            // Create an XML reader out of it
            XmlTextReader xml = new XmlTextReader(stream);

            // Loop through the file
            while (xml.Read())
            {
                // Ignore all but start elements
                if (xml.NodeType != XmlNodeType.Element)
                    continue;

                // Check for name
                if (xml.LocalName == "board")
                {
                    // Load the board
                    Board board = new Board();
                    board.Read(xml);
                    boards.Add(board);
                }
            }
        }
        #endregion

        #region Boards
        private LinkedList<Board> boards = new LinkedList<Board>();

        /// <summary>
        /// Generates a prayer from one randomly selected of the boards.
        /// </summary>
        /// <param name="maxSize"></param>
        /// <returns></returns>
        public Prayer CreatePrayer()
        {
            // Create a new prayer
            Prayer prayer = new Prayer(CreatePrayerBoard());

            // Return the results
            return prayer;
        }

        public Board CreatePrayerBoard()
        {
            // Select a random board
            int index = Entropy.Next(0, boards.Count);
            Board board = boards[index];

            // Clone and return it
            return (Board) board.Clone();
        }
        #endregion
    }
}
