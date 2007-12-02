using C5;
using MfGames.Utility;
using System;
using System.IO;
using System.Xml;

namespace CuteGod
{
    /// <summary>
    /// This keeps track of all the sounds of a given category of sound.
    /// It is read from the sounds.xml embedded resource and allows for
    /// a random selection of sounds for any specific event.
    /// </summary>
    public class SoundCategory
        : ArrayList<FileInfo>
    {
        #region Properties
        private string name;
        private ArrayList<FileInfo> recentlyPlayed = new ArrayList<FileInfo>();

        /// <summary>
        /// Contains the name of the category.
        /// </summary>
        public string Name
        {
            get { return name; }
            set
            {
                if (value == null)
                    throw new Exception("Cannot have null name");

                name = value;
            }
        }

        /// <summary>
        /// Contains the recently played music, this is used to make sure
        /// the same sound isn't played constantly.
        /// </summary>
        public IList<FileInfo> RecentlyPlayed
        {
            get { return recentlyPlayed; }
        }

        /// <summary>
        /// Contains the maximum number of recently played items before
        /// duplication is allowed.
        /// </summary>
        public int RecentlyPlayedSize
        {
            get { return Count / 2; }
        }
        #endregion

        #region Sound Selection
        /// <summary>
        /// Randomly selects a sound from the list and returns the results.
        /// </summary>
        /// <returns></returns>
        public FileInfo GetRandomSound()
        {
            // Ignore empty lists
            if (Count == 0)
                throw new Exception(Name + "No music to play!");

            // See if the recently played list is too big
            if (recentlyPlayed.Count > RecentlyPlayedSize)
                // Remove the last one
                recentlyPlayed.RemoveLast();

            // Randomly select one
            while (true)
            {
                // Select a random sound
                int index = Entropy.Next(0, Count);
                FileInfo sound = this[index];

                // See if we are in the list
                if (!recentlyPlayed.Contains(sound))
                {
                    // We aren't in the recently played
                    recentlyPlayed.InsertFirst(sound);
                    return sound;
                }
            }
        }
        #endregion
    }
}
