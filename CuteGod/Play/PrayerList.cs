using C5;

using MfGames.Sprite3;

using System;

namespace CuteGod.Play
{
    /// <summary>
    /// Contains the list of prayers currently accepted by the
    /// player.
    /// </summary>
    public class PrayerList
        : LinkedList<Prayer>
    {
        #region Properties
        /// <summary>
        /// Contains the largest number of columns found in one
        /// of the prayers.
        /// </summary>
        public int MaximumColumns
        {
            get
            {
                int max = 0;

                foreach (Prayer prayer in this)
                    max = Math.Max(max, prayer.Board.Columns);

                return max;
            }
        }

        /// <summary>
        /// Contains the largest number of rows found in one of
        /// the prayers.
        /// </summary>
        public int MaximumRows
        {
            get
            {
                int max = 0;

                foreach (Prayer prayer in this)
                    max = Math.Max(max, prayer.Board.Rows);

                return max;
            }
        }
        #endregion

		#region Updating
		public void Update(UpdateArgs args)
		{
			foreach (Prayer p in this)
				p.Update(args);
		}
		#endregion
    }
}
