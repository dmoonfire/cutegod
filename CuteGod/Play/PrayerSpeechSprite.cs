using MfGames.Sprite3;
using MfGames.Sprite3.Backends;
using MfGames.Sprite3.Gui;
using MfGames.Sprite3.PlanetCute;

using System;
using System.Drawing;

namespace CuteGod.Play
{
    /// <summary>
    /// Handles the semi-specialized processing of prayer speech bubbles.
    /// </summary>
    public class PrayerSpeechSprite
        : DrawableSprite, IGuiMouseSensitive, IGuiMouseListener
    {
        #region Constructors
        public PrayerSpeechSprite(Prayer prayer)
            : base(AssetLoader.Instance.CreateDrawable("Speech Bubble"))
        {
            this.prayer = prayer;
        }
        #endregion

        #region Properties
        private Prayer prayer;

        /// <summary>
        /// Contains the prayer associated with this speed bubble.
        /// </summary>
        public Prayer Prayer
        {
            get { return prayer; }
            set { prayer = value; }
        }
        #endregion

        #region Mouse Events
        bool isMouseOver = false;
        public bool IsMouseSensitive
        {
            get { return true; }
        }

        public float MouseListenerPriority
        {
            get { return 1000; }
        }

        public void MouseDown(GuiMouseEventArgs args)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void MouseEnter(GuiMouseSensitiveArgs args)
        {
            // Mark that the mouse is over this one
            isMouseOver = true;

            // Listen to the mouse up events
            args.GuiManager.MouseUpListeners.Add(this);
        }

        public void MouseExit(GuiMouseSensitiveArgs args)
        {
            // We no longer have a mouseover
            isMouseOver = false;

            // Stop listening to the mouse up events
            args.GuiManager.MouseUpListeners.Remove(this);
        }

        public void MouseMotion(GuiMouseEventArgs args)
        {
            throw new Exception("The method or operation is not implemented.");
        }

		/// <summary>
		/// Triggered when the mouse is released over this sprite.
		/// </summary>
        public void MouseUp(GuiMouseEventArgs args)
        {
            // We only do stuff if we haven't been accepted
            if (Game.PlayMode == null || prayer.IsAccepted)
                return;

			// Create the center point
			PointF p = new PointF(
				Point.X + 31,
				Point.Y + 90);

            // We want to accept this one
            prayer.IsAccepted = true;
			secondsLeft = Constants.PrayerSpeechFade;
			Game.PlayMode
				.SwarmHearts(Constants.HeartsPerPrayerAcceptance, p);
			
            // End the turn
            Game.State.EndTurn();
            args.StopProcessing = true;
        }
        #endregion

        #region Drawing
        private TopDownViewport topDown;

        public override void Draw(DrawingArgs args)
        {
            // Draw the heart if we haven't been accepted
            if (!Prayer.IsAccepted)
            {
				// Start by drawing the speech bubble
				base.Draw(args);

                // Get the heart drawable, large if the mouse is over it
                if (isMouseOver)
                {
                    IDrawable hd = AssetLoader.Instance.CreateDrawable("Heart");
                    PointF point = new PointF(
                        Point.X + args.ScreenOffsetX + 1,
                        Point.Y + args.ScreenOffsetY + 20);
                    hd.Draw(point, Color.White, args.BackendDrawingArgs);
                }
                else
                {
                    // Show the mini heart
                    IDrawable hd = AssetLoader.Instance
						.CreateDrawable("Half Heart");
                    PointF point = new PointF(
                        Point.X + args.ScreenOffsetX + 15,
                        Point.Y + args.ScreenOffsetY + 45);
                    hd.Draw(point, Color.White, args.BackendDrawingArgs);
                }
            }
            else
            {
				// See if we have no more seconds
				if (secondsLeft < 0)
				{
					topDown = null;
					return;
				}
				
				// Figure out the alpha channel stuff
				double delta = secondsLeft / Constants.PrayerSpeechFade;
				Color c = Color.FromArgb((int) (delta * 255), 255, 255, 255);
				
				// Start by drawing the speech bubble
				base.Draw(args, c);
				
                // See if we have a viewport to accept
                if (topDown == null)
                {
					// Create the top-down element
					topDown = new TopDownViewport(Prayer.Board);
					topDown.BlockFilter =
						IndicatorsViewport.FilterConstruction;

					// Set up the initial point
					PointF point = new PointF(
						Point.X + args.ScreenOffsetX + 13,
						Point.Y + args.ScreenOffsetY + 73);
					topDown.Point = point;

					// Scale the viewport to fit
					int cols = prayer.Board.Columns;
					int rows = prayer.Board.Rows;
					float width = 80
						- topDown.Margin * 2 
						- topDown.Padding * (cols - 1);
					float height = 80
						- topDown.Margin * 2
						- topDown.Padding * (rows - 1);
					width = Math.Min(width, height);
					topDown.BlockWidth = width / (float) cols;
					topDown.BlockHeight = width / (float) rows;
				}

                // Perform the drawing
                topDown.Draw(args, delta);
            }
        }
        #endregion

		#region Updating
		private double secondsLeft;

		/// <summary>
		/// Updates the arguments to handle the speech bubble fading.
		/// </summary>
		public override void Update(UpdateArgs args)
		{
			if (secondsLeft >= 0)
			{
				// Keep track of the number of seconds
				secondsLeft -= args.SecondsSinceLastUpdate;

				// Move the top down
				if (topDown != null)
				{
					topDown.Point = new PointF(
						topDown.Point.X,
						topDown.Point.Y -
						(float) (Constants.PrayerSpeechRise
							* args.SecondsSinceLastUpdate));
				}
			}
		}
		#endregion
    }
}
