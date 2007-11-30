using MfGames.Sprite3;
using MfGames.Sprite3.PlanetCute;
using MfGames.Utility;
using System;

namespace CuteGod.Play
{
    /// <summary>
    /// Represents a single prayer in the system. This includes the layout
    /// for the prayer. This also includes the block drawable for the
    /// character requesting the figure.
    /// </summary>
    public class Prayer
        : Block
    {
        #region Constructors
        /// <summary>
        /// Create an empty prayer.
        /// </summary>
        public Prayer()
            : base(GetRandomCharacterSprite())
        {
            // Set the block information
            Height = 1;
            CastsShadows = false;

            // TODO Set the mass to half a normal block to make sure
            // they fall properly instead of just getting stuck.
            Mass = Constants.BlockMass / 2;

            // Create a random board and assign it
            this.board = Game.PrayerFactory.CreatePrayerBoard();

			// Set up the bounce
			timeUntilBounce = Entropy.NextDouble(
				Constants.MinimumCharacterBounce,
				Constants.MaximumCharacterBounce);
        }

        /// <summary>
        /// Create a prayer with a specific board.
        /// </summary>
        /// <param name="board"></param>
        public Prayer(Board board)
            : this()
        {
            this.board = board;
        }
        #endregion

        #region Properties
        public int BonusValue = 15;
        public int BonusStep = 10;
        public int BonusCounter = 0;
        public bool IsAccepted = false;
        public int X;
        public int Y;
		private double timeUntilBounce;
        #endregion

        #region Board
        private Board board = new Board();

        /// <summary>
        /// Contains the block board that represents both the layout
        /// needed to create the board and also the constructions that
        /// are placed on top of it.
        /// </summary>
        public Board Board
        {
            get { return board; }
        }
        #endregion

        #region Block
		/// <summary>
		/// Returns a randomly created character sprite.
		/// </summary>
		private static ISprite GetRandomCharacterSprite()
		{
			return AssetLoader.Instance.CreateSprite("Character");
		}
        #endregion

		#region Updating
		public void Update(UpdateArgs args)
		{
			// Decrement the time
			timeUntilBounce -= args.SecondsSinceLastUpdate;

			// If we are negative, bounce
			if (timeUntilBounce < 0)
			{
				// Reset the time
				timeUntilBounce = Entropy.NextDouble(
					Constants.MinimumCharacterBounce,
					Constants.MaximumCharacterBounce);

				// Add a positive vector
				Vector += Constants.PrayerBounceVector;
			}
		}
		#endregion
    }
}
