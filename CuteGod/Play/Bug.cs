using MfGames.Sprite3;
using MfGames.Sprite3.PlanetCute;
using MfGames.Utility;
using System;

namespace CuteGod.Play
{
    /// <summary>
    /// Represents a single bug in the system. This includes the bug's
    /// position and the bouncing.
    /// </summary>
    public class Bug
        : Block
    {
        #region Constructors
        /// <summary>
        /// Create an empty bug.
        /// </summary>
        public Bug()
            : base(AssetLoader.Instance.CreateSprite(Constants.BugBlockName))
        {
            // Set the block information
            Height = 0;
            Mass = Constants.BlockMass;
            CastsShadows = false;

			// Set up the bounce
			timeUntilBounce = Entropy.NextDouble(
				Constants.MinimumBugBounce,
				Constants.MaximumBugBounce);
        }

        /// <summary>
        /// Create a bug with a specific board.
        /// </summary>
        /// <param name="board"></param>
        public Bug(Board board)
            : this()
        {
        }
        #endregion

        #region Properties
        public int X;
        public int Y;
		public BlockStack BlockStack;
		private double timeUntilBounce;
        #endregion

		#region Updating
		/// <summary>
		/// Removes a bug from the list and triggers the events.
		/// </summary>
		public void Remove()
		{
			BlockStack.Remove(this);
			Game.State.Bugs.Remove(this);
			// TODO Trigger sound
		}

		/// <summary>
		/// Updates the bug and handles the bouncing, if needed.
		/// </summary>
		public void Update(UpdateArgs args)
		{
			// Decrement the time
			timeUntilBounce -= args.SecondsSinceLastUpdate;

			// If we are negative, bounce
			if (timeUntilBounce < 0)
			{
				// Reset the time
				timeUntilBounce = Entropy.NextDouble(
					Constants.MinimumBugBounce,
					Constants.MaximumBugBounce);

				// Figure out what the top block is. Also move the bug
				// to the bottom as possible.
				Block b = this;
				float position = -1;

				foreach (Block block in BlockStack)
				{
					// We don't bounce ourselves
					if (block == this)
						continue;

					// Check for invalid block
					if (block.Sprite.ID == Constants.ImmobileBlockName &&
						block.TopPosition > position)
					{
						position = block.TopPosition;
					}

					// Check the block
					if (block.BottomPosition > b.BottomPosition)
						b = block;
				}

				// Bounce this one
				b.Vector += Constants.BugBounceVector;
				b.Mass = Constants.BlockMass;
				BottomPosition = position;
			}

			// Set the visbility
			if (this == BlockStack.TopBlock)
			{
				// We are the only one
				//IsVisible = true;
				return;
			}

			// Make ourselves invisible
			//IsVisible = false;
		}
		#endregion
    }
}
