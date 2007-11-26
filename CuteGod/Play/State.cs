using MfGames.Sprite3;
using System;

namespace CuteGod.Play
{
    /// <summary>
    /// The "model" of the game, this represents a game in progress
    /// and also handles the serialization needed to save the game.
    /// </summary>
    public class State
    {
        #region Blocks
        private int grassCount = Constants.MaximumBlocksPerStage / 4;
        private int dirtCount = Constants.MaximumBlocksPerStage / 4;
        private int waterCount = Constants.MaximumBlocksPerStage / 4;
        private int deadGrassCount = 0;
        private int deadDirtCount = 0;
        private int deadWaterCount = 0;
        private int immobileCount = 0;
        private int grabCount = 1;
        private int grabCost = 1;

        /// <summary>
        /// Contains the number of dead dirt (sand) tiles on a stage.
        /// </summary>
        public int DeadDirtCount
        {
            get { return deadDirtCount; }
            set
            {
                if (deadDirtCount < 0)
                    deadDirtCount = 0;
                else
                    deadDirtCount = value;
            }
        }

        /// <summary>
        /// Contains the number of dead grass tiles on a stage.
        /// </summary>
        public int DeadGrassCount
        {
            get { return deadGrassCount; }
            set
            {
                if (deadGrassCount < 0)
                    deadGrassCount = 0;
                else
                    deadGrassCount = value;
            }
        }

        /// <summary>
        /// Contains the number of dead water (brackish) tiles on a stage.
        /// </summary>
        public int DeadWaterCount
        {
            get { return deadWaterCount; }
            set
            {
                if (deadWaterCount < 0)
                    deadWaterCount = 0;
                else
                    deadWaterCount = value;
            }
        }

        /// <summary>
        /// Contains the number of dirt tiles on a stage.
        /// </summary>
        public int DirtCount
        {
            get { return dirtCount; }
            set
            {
                if (dirtCount < 0)
                    dirtCount = 0;
                else
                    dirtCount = value;
            }
        }

        /// <summary>
        /// Indicates the cost in hearts to grab a tile.
        /// </summary>
        public int GrabCost
        {
            get { return grabCost; }
            set { grabCost = value; }
        }

        /// <summary>
        /// Contains the maximum number of grabbed blocks we can have.
        /// </summary>
        public int GrabCount
        {
            get { return grabCount; }
            set { grabCount = value; }
        }

        /// <summary>
        /// Contains the number of grass tiles on a stage.
        /// </summary>
        public int GrassCount
        {
            get { return grassCount; }
            set
            {
                if (grassCount < 0)
                    grassCount = 0;
                else
                    grassCount = value;
            }
        }

        /// <summary>
        /// The number of additional immobile blocks to place.
        /// </summary>
        public int ImmobileCount
        {
            get { return immobileCount; }
            set
            {
                if (immobileCount < 0)
                    immobileCount = 0;
                else
                    immobileCount = value;
            }
        }

        /// <summary>
        /// Contains the number of water tiles on a stage.
        /// </summary>
        public int WaterCount
        {
            get { return waterCount; }
            set
            {
                if (waterCount < 0)
                    waterCount = 0;
                else
                    waterCount = value;
            }
        }
        #endregion

        #region Boards
        private Board board = new Board();

        /// <summary>
        /// Contains the game board.
        /// </summary>
        public Board Board
        {
            get { return board; }
        }

        /// <summary>
        /// Contains the number of stages started by the player.
        /// </summary>
        public int StagesStarted
        {
            get { return Board.Columns / Constants.StageWidth; }
        }
        #endregion

        #region Prayers
        private PrayerList prayers = new PrayerList();

        public int PrayersCompleted;
        public int PrayersOffered;

        /// <summary>
        /// Contains a list of prayers currently accepted.
        /// </summary>
        public PrayerList Prayers
        {
            get { return prayers; }
        }
        #endregion

		#region Bugs
		private BugList bugs = new BugList();
		private int bugRound = 1;

		/// <summary>
		/// Contains the current number of bugs in play.
		/// </summary>
		public int BugCount
		{
			get { return bugs.Count; }
		}

		/// <summary>
		/// Contains the round of bugs, inclusive.
		/// </summary>
		public int BugRound
		{
			get { return bugRound; }
			set { bugRound = value; }
		}

		/// <summary>
		/// Contains a list of bugs.
		/// </summary>
		public BugList Bugs
		{
			get { return bugs; }
		}
		#endregion

        #region Hearts and Stars
        private int hearts = 30;
        private int stars = 0;

        /// <summary>
        /// Contains the number of hearts the player has remaining.
        /// </summary>
        public int Hearts
        {
            get { return hearts; }
            set { hearts = value; }
        }

        /// <summary>
        /// Contains the number of stars the player has earned.
        /// </summary>
        public int Stars
        {
            get { return stars; }
            set { stars = value; }
        }
        #endregion

		#region Countdown and Timers
		private double secondsRemaining = -1;
		private double secondsPerTurn = -1;

		/// <summary>
		/// Contains the number of seconds left in this turn
		/// before the player loses a heart.
		/// </summary>
		public double SecondsRemaining
		{
			get { return secondsRemaining; }
			set { secondsRemaining = value; }
		}

		/// <summary>
		/// Sets the number of seconds per turn.
		/// </summary>
		public double SecondsPerTurn
		{
			get { return secondsPerTurn; }
			set { secondsPerTurn = value; }
		}
		#endregion

        #region Turns
        /// <summary>
        /// Indicates end of turn processing.
        /// </summary>
        public void EndTurn()
        {
            // See if we have negative hearts, if so, end the game
            if (Game.State.Hearts < 0)
            {
                Game.State.Hearts = 0;
                (Game.GameMode as PlayMode).MinorMode =
                    new EndOfGameMinorMode();
            }

			// Update the time
			secondsRemaining = secondsPerTurn;
        }

		/// <summary>
		/// Updates the counters and elements for the state.
		/// </summary>
		public void Update(UpdateArgs args)
		{
			// Don't bother if we don't have a timeout
			if (secondsPerTurn > 0)
			{
				// Reduce the time in the round
				secondsRemaining -= args.SecondsSinceLastUpdate;

				// See if we passed the line
				if (secondsRemaining < 0)
				{
					// Lose a heart and end the turn
					Game.State.Hearts--;
					EndTurn();

					// Update the tick counter
					secondsRemaining = secondsPerTurn;
				}
			}
		}
        #endregion
    }
}
