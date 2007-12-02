using System;
using System.Drawing;

namespace CuteGod
{
    /// <summary>
    /// Contains all the static readonlyants used throughout the game.
    /// </summary>
    public static class Constants
    {
        #region Scoring
        /// <summary>
        /// Contains the number of hearts to accept a prayer.
        /// </summary>
        public static readonly int HeartsPerPrayerAcceptance = 5;

        /// <summary>
        /// Contains the number of hearts per prayer block.
        /// </summary>
        public static readonly int HeartsPerPrayerBlock = 1;

        /// <summary>
        /// Contains the number of stars per prayer block.
        /// </summary>
        public static readonly int StarsPerPrayerBlock = 1;
        #endregion

        #region Positions
        /// <summary>
        /// Contains the offset from the top block that a grab hovers at.
        /// </summary>
        public static readonly float GrabStackOffset = 4f;

        /// <summary>
        /// Contains the minimum height of the grab stack.
        /// </summary>
        public static readonly float MinimumGrabHeight = 8f;

        /// <summary>
        /// Contains the highest position that the base can be.
        /// </summary>
        public static readonly float TopImmobilePosition = 1f;

        /// <summary>
        /// Contains the highest position that a ground block may be.
        /// </summary>
        public static readonly float TopGroundPosition = 4f;

        /// <summary>
        /// The position that a block should be assigned to for dropping
        /// for a prayer, to show the various hearts and stars.
        /// </summary>
        public static readonly float PrayerDroppingMultiplier = 2f;
        #endregion

        #region Board Layout
        /// <summary>
        /// Contains the number of rows on the board.
        /// </summary>
        public static readonly int BoardRows = 5;

        /// <summary>
        /// Contains the number of immobile blocks on the board.
        /// </summary>
        public static readonly int ImmobilePositions = 1;

        /// <summary>
        /// Contains the number of movable blocks (i.e. the game) that
        /// may be stacked on top of each other.
        /// </summary>
        public static readonly int MaximumBlockPositions = 3;

        /// <summary>
        /// Stacks with this many items may not have any more.
        /// </summary>
        public static readonly int MaximumPlacementCount =
            MaximumBlockPositions + ImmobilePositions;

        /// <summary>
        /// Identifies the highest construction possible in the game.
        /// </summary>
        public static readonly int MaximumConstructionHeight = 4;

        /// <summary>
        /// Dictates the largest number of swapable blocks a character
        /// may have.
        /// </summary>
        public static readonly int MaximumGrabCount = 4;

        /// <summary>
        /// Contains the bottom of the swap position.
        /// </summary>
        public static readonly float BottomGrabPosition =
            MaximumConstructionHeight + MaximumBlockPositions
			+ ImmobilePositions;

        /// <summary>
        /// Contains the highest possible position in the board. This is
        /// the top position, not bottom.
        /// </summary>
        public static readonly float MaximumPosition =
            BottomGrabPosition + MaximumGrabCount + 1;

        /// <summary>
        /// Defines the focus point for the block viewport.
        /// </summary>
        public static readonly float FocusPosition =
            BottomGrabPosition;

        /// <summary>
        /// Contains the spacing between blocks when they are dropped.
        /// </summary>
        public static readonly float PlacementSpacing = 1.5f;

        /// <summary>
        /// Identifies the size of each stage.
        /// </summary>
        public static readonly int StageWidth = 5;

        /// <summary>
        /// How many initial stages in the game?
        /// </summary>
        public static readonly int InitialStages = 3;

        /// <summary>
        /// Contains the maximum number of blocks in a single stage. This
        /// must show up in the file after the referenced to variables.
        /// </summary>
        public static readonly int MaximumBlocksPerStage =
            MaximumBlockPositions * BoardRows * StageWidth;

		/// <summary>
		/// Contains the maximum blocks changed with the
		/// conversion. This should be a factor of three.
		/// </summary>
		public static readonly int MaximumBlocksChanged = 3;
        #endregion

        #region Block
        public static readonly string ImmobileBlockName = "Immobile Block";
        public static readonly string DirtBlockName = "Dirt Block";
        public static readonly string GrassBlockName = "Grass Block";
        public static readonly string WaterBlockName = "Water Block";
        public static readonly string DeadDirtBlockName = "Dead Dirt Block";
        public static readonly string DeadGrassBlockName = "Dead Grass Block";
        public static readonly string DeadWaterBlockName = "Dead Water Block";
        public static readonly string SelectorName = "Selector";
        public static readonly string InvalidSelectorName = "Invalid Selector";
		public static readonly string BugBlockName = "Enemy Bug";

        public static readonly string MiniHeartName = "Mini Heart";
        public static readonly string MiniStarName = "Mini Star";
        public static readonly string MiniClockName = "Mini Clock";
        public static readonly string MiniBugName = "Mini Bug";

        /// <summary>
        /// Contains the mass of the blocks, which dictates their drop speed.
        /// </summary>
        public static readonly float BlockMass = 5f;

        /// <summary>
        /// Contains the mass of the prayer completed blocks.
        /// </summary>
        public static readonly float PrayerBlockMass = 0.5f;

        /// <summary>
        /// Contains the maximum number of blocks that can be placed at one
        /// time using the random block layout.
        /// </summary>
        public static readonly int MaximumBlocksPlaced = 10;

        /// <summary>
        /// Contains the initial dropped vector.
        /// </summary>
        public static readonly float DroppedVector = 10f;

        /// <summary>
        /// Contains the prayer dropped vector.
        /// </summary>
        public static readonly float PrayerDroppedVector = 0.5f;

        /// <summary>
        /// Contains the position that the blocks are dropped from.
        /// </summary>
        public static readonly float DroppedPosition = 5f;

        /// <summary>
        /// Contains the highest placed position on a stack.
        /// </summary>
        public static readonly float TopPlacedPosition = MaximumPlacementCount;

		/// <summary>
		/// How hard does the prayer jump when it is time.
		/// </summary>
		public static readonly float PrayerBounceVector = -2;

		/// <summary>
		/// How much extra high should they jump if they are jumping
		/// north.
		/// </summary>
		public static readonly float PrayerBounceNorthVector = -2;

		/// <summary>
		/// How much extra should they jump up a block?
		/// </summary>
		public static readonly float PrayerBounceUpVector = -1f;

		/// <summary>
		/// The rate for moving up and down to make it a smooth jump.
		/// </summary>
		public static readonly float PrayerNorthSouthAdjust = 1.0f;

		/// <summary>
		/// The rate for moving right and left for a smooth jump.
		/// </summary>
		public static readonly float PrayerEastWestAdjust = 65.0f;

		/// <summary>
		/// How hard does the bug jump when it is time.
		/// </summary>
		public static readonly float BugBounceVector = -1;
        #endregion

        #region Colors
        /// <summary>
        /// Contains the common color for the heart.
        /// </summary>
        public static readonly Color HeartColor =
            Color.FromArgb(231, 68, 0);

        /// <summary>
        /// Contains the common color for stars.
        /// </summary>
        public static readonly Color StarColor =
            Color.FromArgb(238, 238, 88);

        /// <summary>
        /// Contains the common color for chests.
        /// </summary>
        public static readonly Color ChestColor =
            Color.FromArgb(219, 163, 114);

		/// <summary>
		/// The common color for bug-colored text.
		/// </summary>
		public static readonly Color BugColor =
			Color.FromArgb(215, 129, 69);

		/// <summary>
		/// Color for anything related to clocks.
		/// </summary>
		public static readonly Color ClockColor =
			Color.FromArgb(192, 227, 224);
        #endregion

		#region Timing
		/// <summary>
		/// The number of seconds to fade in the main menu windows.
		/// </summary>
		public static readonly double MainMenuFadeIn = 0.5;

		/// <summary>
		/// The number of seconds to show a new stage display.
		/// </summary>
		public static readonly double NewStageDisplay = 3;

		/// <summary>
		/// The number of seconds to show end of game.
		/// </summary>
		public static readonly double EndOfGameDisplay = 3;

		/// <summary>
		/// The number of seconds for the prayer speeches dialogs to
		/// fade.
		/// </summary>
		public static readonly double PrayerSpeechFade = 2;

		/// <summary>
		/// How fast the prayer speech rises
		/// </summary>
		public static readonly double PrayerSpeechRise = 50;

		/// <summary>
		/// How long to show the prayer screen.
		/// </summary>
		public static readonly double PrayerCompletedTimeout = 5;

		/// <summary>
		/// The minimum amount of time before a figure bounces.
		/// </summary>
		public static readonly double MinimumCharacterBounce = 1;

		/// <summary>
		/// The maximum amount of time before a figure bounces.
		/// </summary>
		public static readonly double MaximumCharacterBounce = 5;

		/// <summary>
		/// The minimum amount of time before a bug bounces.
		/// </summary>
		public static readonly double MinimumBugBounce = 1;

		/// <summary>
		/// The maximum amount of time before a figure bounces.
		/// </summary>
		public static readonly double MaximumBugBounce = 2;

		/// <summary>
		/// The number of seconds from the initial countdown.
		/// </summary>
		public static readonly double StartingCountdown = 60;

		/// <summary>
		/// How much to reduce the countdown each time it
		/// comes up.
		/// </summary>
		public static readonly double LessTimeSeconds = 5;

		/// <summary>
		/// Contains the minimum number of seconds per turn.
		/// </summary>
		public static readonly double MinimumSecondsPerTurn = 10;
		#endregion

		#region Swarm 
		/// <summary>
		/// Contains the initial speed of the swarm particle.
		/// </summary>
		public static readonly float InitialSwarmSpeed = 200f;

		/// <summary>
		/// Contains the gravity of the swarm, used to accelerate the swarm
		/// </summary>
		public static readonly float SwarmTargetGravity = 200f;

		/// <summary>
		/// Identifies how close it has to be before it is considered
		/// on target.
		/// </summary>
		public static readonly double SwarmTargetRadius = 10;

		/// <summary>
		/// Defines the maximum life of a particle, in seconds.
		/// </summary>
		public static readonly double MaxSwarmParticleLife = 5;
		#endregion

		#region Other
		/// <summary>
		/// A random chance of getting a chest.
		/// </summary>
		public static readonly int ChestChance = 100;

		/// <summary>
		/// Minimum stage that the bugs can show up.
		/// </summary>
		public static readonly int MinimumStageForBugs = 10;

		/// <summary>
		/// The earliest stage that clock changes can be made.
		/// </summary>
		public static readonly int MinimumStageForClock = 10;
		#endregion

		public static readonly double ChestOpenFadeSpeed = 1;
		public static readonly double ExtraHeartsStageMultiplier = 1;

		#region Configuration Keys
		public static readonly string ConfigMusicVolume = "Music Volume";
		public static readonly string ConfigWindowedWidth = "Windowed Width";
		public static readonly string ConfigWindowedHeight = "Windowed Height";
		public static readonly string ConfigFullScreen = "Full Screen";
		public static readonly string ConfigFullScreenWidth =
			"Full Screen Width";
		public static readonly string ConfigFullScreenHeight =
			"Full Screen Height";
		#endregion
    }
}
