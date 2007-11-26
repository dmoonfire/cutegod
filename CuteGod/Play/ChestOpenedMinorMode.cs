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
	/// Shows the results of a chest being opened.
    /// </summary>
    public class ChestOpenedMinorMode
	: DisplayStatusMinorMode
    {
        #region Constructors
        /// <summary>
        /// Constructs the basic minor mode and sets up the internal
        /// structures.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="chest"></param>
        public ChestOpenedMinorMode(int number)
			: base()
        {
			// Save the values
			ChestCounter = number;

			// Debugging
			Subtitle = "Opening Chests";
        }
        #endregion

        #region Activation and Deactivation
        /// <summary>
        /// Indicates to the game mode that it is activated.
        /// </summary>
        public override void Activate()
        {
            // Change the background music
            Game.Sound.ChestOpened();
        }

        /// <summary>
        /// Triggered when the game mode is deactivated.
        /// </summary>
        public override void Deactivate()
        {
        }
        #endregion

        #region Drawing
		protected override string ChestDrawableName
		{
			get { return "Chest Open"; }
		}
		#endregion

        #region Updating
		private ChestBonusInfo chestInfo = null;
		private DrawableSprite bonusSprite;
		private double chestSeconds;
		private bool fadeIn;
		private bool pause;
		private Color tint = Color.White;

        /// <summary>
        /// Triggers an update of the mode's state.
        /// </summary>
        /// <param name="args"></param>
        public override void Update(UpdateArgs args)
        {
			// See if we have any chests left
			if (chestInfo == null && ChestCounter == 0)
			{
				// We don't, so finish up
				Timeout();
				return;
			}

			// If the chest is null, we need to start up a new chest
			if (chestInfo == null)
			{
				// Create a new chest
				ChestCounter--;
				chestInfo = ChestBonusInfo.Random;

				// Give it a time to live
				chestSeconds = Constants.ChestOpenFadeSpeed;

				// Create the sprites and direction
				bonusSprite = new DrawableSprite(
					Game.DrawableManager[chestInfo.BlockKey]);
				bonusSprite.Point = ChestSprite.Point;
				fadeIn = true;
				pause = false;

				// Set up the text
				Title = chestInfo.Title;
				Text = String.Format(chestInfo.Description,
					Game.State.SecondsPerTurn,
					Game.State.BugCount,
					Game.State.GrabCost,
					Game.State.Board.Columns);

				// Add it to the sprites
				Sprites.Add(bonusSprite);
			}
			else
			{
				// Increment the pointer
				chestSeconds -= args.SecondsSinceLastUpdate;

				// If we are negative, then we change mode
				if (chestSeconds < 0)
				{
					// Switch mode
					if (pause)
					{
						// Fade out
						pause = false;
						fadeIn = false;
					}
					else if (fadeIn)
					{
						// Pause it
						pause = true;

						// Apply the value
						chestInfo.Apply(bonusSprite.Point);
					}
					else
					{
						// Remove it
						Sprites.Remove(bonusSprite);
						chestInfo = null;
						bonusSprite = null;
						return;
					}

					// Update the counter
					chestSeconds = Constants.ChestOpenFadeSpeed;
				}
			}

			// Figure out the ratios (1 is just start, 0 is done)
			double timeRatio = chestSeconds / Constants.ChestOpenFadeSpeed;
			float positionRatio = (float) (timeRatio * BlockHeight);
			int alpha = (int) (255 * timeRatio);

			// Figure out the chest position
			if (pause)
			{
				// We aren't doing anything until the seconds change
				bonusSprite.Point = new PointF(
					ChestSprite.Point.X,
					ChestSprite.Point.Y - BlockHeight);
				tint = Color.White;
			}
			else if (fadeIn)
			{
				// Change the position based on the ratio
				bonusSprite.Point = new PointF(
					ChestSprite.Point.X,
					ChestSprite.Point.Y - BlockHeight + positionRatio);

				// Change the alpha based on the ratio
				tint = Color.FromArgb(255 - alpha, Color.White);
			}
			else
			{
				// Change the position based on the ratio
				bonusSprite.Point = new PointF(
					ChestSprite.Point.X,
					ChestSprite.Point.Y - 2 * BlockHeight + positionRatio);

				// Change the alpha based on the ratio
				tint = Color.FromArgb(alpha, Color.White);
			}

			// Move the sprite
			bonusSprite.Tint = tint;

			// Call the base
			base.Update(args);
        }
		
		/// <summary>
		/// Triggered when the mode times out. 
		/// </summary>
		protected override void Timeout()
		{
			// See if we need to finish up anything
			if (chestInfo != null)
			{
				// Just apply it where ever it is
				chestInfo.Apply(bonusSprite.Point);
			}

			// Loop through any remaining chests
			while (ChestCounter > 0)
			{
				chestInfo = ChestBonusInfo.Random;
				chestInfo.Apply(ChestSprite.Point);
				ChestCounter--;
			}

			// Change the mini mode to the new stage
			(Game.GameMode as PlayMode).MinorMode =
				new NewStageMinorMode();
		}
        #endregion
    }
}
