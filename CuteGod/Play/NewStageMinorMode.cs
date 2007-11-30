using BooGame.Input;
using MfGames.Sprite3;
using MfGames.Utility;
using System;
using System.Drawing;

namespace CuteGod.Play
{
    /// <summary>
    /// Minor mode to indicate what the new stage is and to apply
    /// any of the difficulties to the new system.
    /// </summary>
    public class NewStageMinorMode
        : DisplayStatusMinorMode
    {
        #region Constructors
        /// <summary>
        /// Constructs the minor mode and sets up the internal
        /// sprites.
        /// </summary>
        public NewStageMinorMode()
			: base()
        {
			// Create the block with a placeholder
			block =	AssetLoader.Instance.CreateSprite("Star");
			Viewport.Add(block);
        }
        #endregion

        #region Activation and Deactivation
        /// <summary>
        /// Indicates to the game mode that it is activated.
        /// </summary>
        public override void Activate()
        {
			// Call the base
			base.Activate();

            // Figure out the penalty
			StagePenaltyInfo info = StagePenaltyInfo.Random;

			// Set the value
			Subtitle = "New Stage";
			Title = info.Title;
			Text = String.Format(info.Description,
				Game.State.SecondsPerTurn,
				Game.State.BugCount,
				Game.State.GrabCost);
			ChestVisible = false;

			// Create the block
			block = AssetLoader.Instance.CreateSprite(info.BlockKey);

			// Set up our timeout
			CountingDown = true;
			SecondsRemaining = Constants.NewStageDisplay;
        }

        /// <summary>
        /// Triggered when the game mode is deactivated.
        /// </summary>
        public override void Deactivate()
        {
            // Add a new stage
			base.Deactivate();
            Game.State.Board.GenerateStage();
        }
        #endregion

		#region Controller
		/// <summary>
		/// Triggered when the user releases or presses a key.
		/// </summary>
		protected override void Timeout()
		{
			Game.PlayMode.MinorMode = null;
		}
		#endregion

        #region Properties
		private ISprite block;

        /// <summary>
        /// Sets the size of the game mode (based on the screen).
        /// </summary>
        public override SizeF Size
        {
            set
            {
				// Call our parent's
				base.Size = value;

				// Center the block (if we loaded it yet)
				float cx = Viewport.Size.Width / 2;
				float bw = block.Size.Width / 2;
				block.Point = new PointF(
					cx - bw,
					Viewport.Size.Height - BlockHeight);
            }
        }
        #endregion
    }
}
