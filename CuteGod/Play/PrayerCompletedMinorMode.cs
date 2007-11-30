using BooGame.Input;
using C5;
using MfGames.Sprite3;
using MfGames.Sprite3.PlanetCute;
using MfGames.Utility;
using System;
using System.Drawing;

namespace CuteGod.Play
{
    /// <summary>
    /// Handles the processing of a prayer minor mode, including
    /// stage progression and processing.
    /// </summary>
    public class PrayerCompletedMinorMode
	: DisplayStatusMinorMode
    {
        #region Constructors
        /// <summary>
        /// Constructs the basic minor mode and sets up the internal
        /// structures.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="prayer"></param>
        public PrayerCompletedMinorMode(int x, int y, Prayer prayer)
			: base()
        {
            // Save the values
            X = x;
            Y = y;

            // Set up the prayer board
            Prayer = prayer;
            Prayer.Board.MaximumPosition = 5;
            Prayer.Board.MinimumPosition = -2;

            // Create the block viewport
            blockViewport =
                new BlockViewport(Prayer.Board, AssetLoader.Instance);
            blockViewport.Size = blockViewport.Extents.Size;
            blockViewport.ClipContents = false;
            Viewport.Add(blockViewport);

            // We modify the prayer to simulate a drop by raising
            // everything but the bottommost one.
            for (int i = 0; i < prayer.Board.Columns; i++)
            {
                // Get the stack
                IList<BlockStack> stacks = prayer.Board[i];

                // Go through the stack
                for (int j = 0; j < stacks.Count; j++)
                {
                    // Get the stack
                    BlockStack stack = stacks[j];
                    stack.IsInInitialPlacement = false;

                    // Go through the blocks
                    foreach (Block block in stack)
                    {
						// Mark this as unusable
						block.Data = false;

						// Figure out the position
                        if (block.BottomPosition != 0)
                        {
                            // For the top blocks
                            block.BottomPosition *=
								Constants.PrayerDroppingMultiplier;
							block.BottomPosition +=
								Entropy.NextFloat(-0.5f, 0.5f);
                            block.IsMoving = true;
                            block.Mass = Constants.PrayerBlockMass;
                            block.Vector = Constants.PrayerDroppedVector;
                        }
                        else
                        {
                            // For the bottom blocks
                            block.IsMoving = false;
                            block.Vector = 0;
                            block.Mass = 0;
                        }
                    }
                }
            }

            // Attack some new events
            prayer.Board.BlockMovingChanged += OnBlockMovingChanged;
            prayer.Board.MovingChanged += OnMovingChanged;
        }
        #endregion

        #region Activation and Deactivation
        /// <summary>
        /// Sets the size of the game mode (based on the screen).
        /// </summary>
        public override SizeF Size
        {
            set
            {
				// Call the base
				base.Size = value;

                // Figure out the total width
                blockViewport.FocusPosition = 4;
                RectangleF rect = blockViewport.Extents;

                // Arrange the block viewport so it is on the left
                // side, middle
                float y = (Viewport.Size.Height - rect.Height) / 2;
                float x = (Viewport.Size.Width - rect.Width) / 2;
                blockViewport.Point = new PointF(x, y);
                blockViewport.Size = rect.Size;
            }
        }

        /// <summary>
        /// Indicates to the game mode that it is activated.
        /// </summary>
        public override void Activate()
        {
            // Change the background music
            Game.Sound.PrayerCompleted(Prayer);
        }

        /// <summary>
        /// Triggered when the game mode is deactivated.
        /// </summary>
        public override void Deactivate()
        {
			// Migrate this board to the game board
			Prayer.Board.MigrateBoard(Game.State.Board, X, Y);

            // Remove the prayer
            Game.State.Prayers.Remove(Prayer);
            Game.State.Board[Prayer.X, Prayer.Y].Remove(Prayer);

			// Update the turn
			Game.State.EndTurn();
        }
        #endregion

        #region Drawing
        private BlockViewport blockViewport;
		private int chestChance = 0;
		#endregion

        #region Updating
        /// <summary>
        /// Triggers an update of the mode's state.
        /// </summary>
        /// <param name="args"></param>
        public override void Update(UpdateArgs args)
        {
			// Call the base
			base.Update(args);

            // Update the prayer
            Prayer.Board.Update(args);
        }
		
		/// <summary>
		/// Triggered when the mode times out. 
		/// </summary>
		protected override void Timeout()
		{
			// See if we have any moving blocks. If we do, we advance
			// a slight period of time and try again.
			while (!CountingDown)
			{
				// Build up the argument
				UpdateArgs args = new UpdateArgs();
				args.TicksSinceLastUpdate = 10000;
				Update(args);
			}

			// Change the mini mode to the new stage
			if (ChestCounter > 0)
				Game.PlayMode.MinorMode =
					new ChestOpenedMinorMode(ChestCounter);
			else
				Game.PlayMode.MinorMode = new NewStageMinorMode();
		}
        #endregion

        #region Events
        private void OnBlockMovingChanged(
			object sender, BlockStackEventArgs args)
        {
			// Figure out the point
			Block b = args.Block;
			BlockStack bs = args.Stack;
			PointF p1 =
				blockViewport.ToPoint(bs.X, bs.Y, b.Height);
			PointF p = new PointF(
				p1.X + blockViewport.Point.X + 31 + Viewport.Point.X,
				p1.Y + blockViewport.Point.Y + 90 + Viewport.Point.Y);

			// Swarm the stars and hearts
			Game.PlayMode
				.SwarmStars(Constants.StarsPerPrayerBlock, p);
			Game.PlayMode
				.SwarmHearts(Constants.HeartsPerPrayerBlock, p);

			// Mark the block as unmovable
			b.Data = false;

			// Add the chance of a chest
			int random = Entropy.Next(Constants.ChestChance);
			chestChance++;

			if (random <= chestChance)
			{
				chestChance = 0;
				ChestCounter++;
			}
        }

        private void OnMovingChanged(object sender, BlockStackEventArgs args)
        {
            // See if we are completely done
            if (!Prayer.Board.IsMoving)
            {
				// Start the countdown timer
				SecondsRemaining = Constants.PrayerCompletedTimeout;
				CountingDown = true;
            }
        }
        #endregion

        #region Properties
        public int X, Y;
        public Prayer Prayer;
        #endregion
    }
}
