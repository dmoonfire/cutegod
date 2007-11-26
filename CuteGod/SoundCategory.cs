using C5;
using MfGames.Utility;
using System;
using System.Xml;

namespace CuteGod
{
    /// <summary>
    /// This keeps track of all the sounds of a given category of sound.
    /// It is read from the sounds.xml embedded resource and allows for
    /// a random selection of sounds for any specific event.
    /// </summary>
    public class SoundCategory
        : ArrayList<string>
    {
        #region Properties
        private string name;
        private ArrayList<string> recentlyPlayed = new ArrayList<string>();

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
        public IList<string> RecentlyPlayed
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
        public string GetRandomSound()
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
                string sound = this[index];

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

        #region XML
        /// <summary>
        /// Reads the XML stream and populates the categories.
        /// </summary>
        /// <param name="xml"></param>
        public void Read(XmlTextReader xml)
        {
            // We already have the category tag, so grab the name
            name = xml["title"];

            // Loop forever to get the sounds until we finish
            while (xml.Read())
            {
                switch (xml.NodeType)
                {
                    case XmlNodeType.EndElement:
                        if (xml.LocalName == "category")
                            return;
                        break;
                    case XmlNodeType.Element:
                        if (xml.LocalName == "sound")
                            Add(xml.ReadString());
                        break;
                }
            }
        }
        #endregion
    }
}
