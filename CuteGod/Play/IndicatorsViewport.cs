using C5;

using MfGames.Sprite3;
using MfGames.Sprite3.Gui;
using MfGames.Sprite3.PlanetCute;

using System;
using System.Drawing;

namespace CuteGod.Play
{
    /// <summary>
    /// Encapsulates the functionality of the indicators including
    /// balancing windows and keeping everything organized on top.
    /// </summary>
    public class IndicatorsViewport
        : SpriteViewport, IGuiMouseSensitive, IGuiMouseListener
    {
        #region Constants
        /// <summary>
        /// Contains the size of the font used in the window.
        /// </summary>
        private static readonly float FontSize = 40;

        /// <summary>
        /// Contains the padding between the viewports.
        /// </summary>
        private static readonly float Padding = 5;

        /// <summary>
        /// Represents the width of the status viewport.
        /// </summary>
        private static readonly float StatusWidth = 300;
        #endregion

        #region Constructors
        /// <summary>
        /// Constructs the viewport and arranges all the needed
        /// elements.
        /// </summary>
        public IndicatorsViewport()
        {
            // Set the default values
            BackgroundColor = Color.FromArgb(128, Color.Black);
            Z = 100;

            // Create the status viewport
            statusViewport = new SpriteViewport();
            statusViewport.ClipContents = true;
            statusViewport.Z = 200;
            Add(statusViewport);

            // Load the mini icons
            heart = new DrawableSprite(
                Game.DrawableManager[Constants.MiniHeartName]);
            heart.Z = 300;
            heart.Point = new PointF(5, 0);
            statusViewport.Add(heart);

            star = new DrawableSprite(
                Game.DrawableManager[Constants.MiniStarName]);
            star.Point = new PointF(5, FontSize + 2);
            star.Z = 300;
            statusViewport.Add(star);

            clock = new DrawableSprite(
                Game.DrawableManager[Constants.MiniClockName]);
            clock.Z = 300;
            clock.Point = new PointF(155, 0);
            statusViewport.Add(clock);

            bug = new DrawableSprite(
                Game.DrawableManager[Constants.MiniBugName]);
            bug.Point = new PointF(155, FontSize + 2);
            bug.Z = 300;
            statusViewport.Add(bug);

            // Create the heart scores
            heartScore =
                new TextDrawableSprite(Game.GetFont(FontSize),
                    Game.State.Hearts.ToString());
            heartScore.Z = 300;
            heartScore.Point = new PointF(35, 15);
            heartScore.Color = Constants.HeartColor;
			heartScore.Alignment = ContentAlignment.MiddleLeft;
            statusViewport.Add(heartScore);

            // Create the star scores
            starScore =
                new TextDrawableSprite(Game.GetFont(FontSize),
                    Game.State.Stars.ToString());
            starScore.Z = 300;
            starScore.Point = new PointF(35, FontSize + 15 + 4);
            starScore.Color = Constants.StarColor;
			starScore.Alignment = ContentAlignment.MiddleLeft;
            statusViewport.Add(starScore);

            // Create the clock scores
            clockScore =
                new TextDrawableSprite(Game.GetFont(FontSize),
                    Game.State.SecondsRemaining.ToString());
            clockScore.Z = 300;
            clockScore.Point = new PointF(185, heartScore.Point.Y);
            clockScore.Color = Constants.ClockColor;
			clockScore.Alignment = ContentAlignment.MiddleLeft;
            statusViewport.Add(clockScore);

            // Create the star scores
            bugScore =
                new TextDrawableSprite(Game.GetFont(FontSize),
                    Game.State.Stars.ToString());
            bugScore.Z = 300;
            bugScore.Point = new PointF(clockScore.Point.X, starScore.Point.Y);
            bugScore.Color = Constants.BugColor;
			bugScore.Alignment = ContentAlignment.MiddleLeft;
            statusViewport.Add(bugScore);

            // Create the prayer viewport
            prayerViewport = new SpriteViewport();
            prayerViewport.ClipContents = true;
            Add(prayerViewport);
        }
        #endregion

        #region Sprites
        private SpriteViewport prayerViewport;
        private SpriteViewport statusViewport;
        private TextDrawableSprite heartScore;
        private TextDrawableSprite starScore;
        private DrawableSprite heart;
        private DrawableSprite star;
        private TextDrawableSprite bugScore;
        private TextDrawableSprite clockScore;
        private DrawableSprite bug;
        private DrawableSprite clock;

		/// <summary>
		/// Contains the position of the heart icon.
		/// </summary>
		public PointF HeartPoint
		{
			get
			{
				return new PointF(
					statusViewport.Point.X + heart.Point.X,
					statusViewport.Point.Y + heart.Point.Y);
			}
		}

		/// <summary>
		/// Contains the position of the star icon.
		/// </summary>
		public PointF StarPoint
		{
			get
			{
				return new PointF(
					statusViewport.Point.X + star.Point.X,
					statusViewport.Point.Y + star.Point.Y);
			}
		}
        #endregion

        #region Viewport
        /// <summary>
        /// Handles the size allocation processing for the viewport.
        /// </summary>
        public override SizeF Size
        {
            get
            {
                return base.Size;
            }
            set
            {
                // Set the base size
                base.Size = value;

                // Set the status viewport
                statusViewport.Size = new SizeF(
                    IndicatorsViewport.StatusWidth,
                    value.Height);
                statusViewport.Point = new PointF(
                    value.Width - StatusWidth, 0);

                // Set the prayer point
                prayerViewport.Size = new SizeF(
                    value.Width - StatusWidth - Padding,
                    value.Height);
            }
        }

        /// <summary>
        /// Overrides the drawing routines.
        /// </summary>
        /// <param name="args"></param>
        public override void Draw(DrawingArgs args)
        {
            // Save our coordinate
            screenPoint = new PointF(
                prayerViewport.Point.X + args.ScreenOffsetX,
                prayerViewport.Point.Y + args.ScreenOffsetY);

            // Update the prayer data
            UpdatePrayers();

            // Perform the basic drawing
            base.Draw(args);
        }

        /// <summary>
        /// Handles the update code for all the elements.
        /// </summary>
        /// <param name="args"></param>
        public override void Update(UpdateArgs args)
        {
            // Update the viewport
            base.Update(args);

            // Update our text
            heartScore.Text = Game.State.Hearts.ToString();
            starScore.Text = Game.State.Stars.ToString();

			// Visbility of the clock
			if (Game.State.SecondsPerTurn > 0)
			{
				clock.Visible = true;
				clockScore.Visible = true;
				clockScore.Text =
					((int) Game.State.SecondsRemaining).ToString();
			}
			else
			{
				clock.Visible = false;
				clockScore.Visible = false;
			}

			// Visible bugs
			if (Game.State.BugCount > 0)
			{
				bug.Visible = true;
				bugScore.Visible = true;
				bugScore.Text = Game.State.BugCount.ToString();
			}
			else
			{
				bug.Visible = false;
				bugScore.Visible = false;
			}
        }
        #endregion

        #region Prayers
        private HashDictionary<Prayer, TopDownViewport> prayerViews =
            new HashDictionary<Prayer, TopDownViewport>();

        /// <summary>
        /// Filters out all but the bottom layer of the block.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static bool FilterConstruction(
			object sender, BlockFilterArgs args)
        {
            return args.Block.Position == 0;
        }

        /// <summary>
        /// Updates the prayer viewport to show/include/remove the
        /// top-down views to fit the current view.
        /// </summary>
        private void UpdatePrayers()
        {
            // Build up a list of ones to remove
            LinkedList<Prayer> oldKeys = new LinkedList<Prayer>();
            oldKeys.AddAll(prayerViews.Keys);

            // Loop through the prayers
            float x = Padding;

            foreach (Prayer prayer in Game.State.Prayers)
            {
                // Ignore invisible prayers
                if (!prayer.IsAccepted)
                    continue;

                // See if we have the view
                if (!prayerViews.Contains(prayer))
                {
                    // We need to create the top-down view
                    TopDownViewport tdv = new TopDownViewport(prayer.Board,
                        Game.DrawableManager);

                    tdv.BlockFilter = FilterConstruction;

                    // Set the value
                    prayerViews[prayer] = tdv;
                    prayerViewport.Add(tdv);
                }
                else
                {
                    // Remove this from the to-remove (old) list
                    oldKeys.Remove(prayer);
                }

                // Change the location of the viewport
                TopDownViewport v = prayerViews[prayer];
                v.Point = new PointF(x, Padding);

                // Adjust the size based on how close the mouse is to
                // viewport.
                float size = 8;
                float left = Math.Abs(mousePoint.X - v.Point.X);
                float right =
                    Math.Abs(mousePoint.X - (v.Point.X + v.Size.Width));

                // Average these two to get the midpoint
                float average = (left + right) / 2;

                // Only make a different if we are with 64 of it
                if (average <= 64)
                {
                    float ratio = (64 - average) / 64;
                    size += ratio * 8;
                }

                v.BlockHeight = v.BlockWidth = size;

                // Update the new x coordinate
                v.Size = v.Extents.Size;
                x += Padding + v.Size.Width;
            }

            // Anything left in the old list needs to be removed
            foreach (Prayer prayer in oldKeys)
            {
                // Get the viewport
                TopDownViewport ov = prayerViews[prayer];
                prayerViewport.Remove(ov);
                prayerViews.Remove(prayer);
            }
        }
        #endregion

        #region IGuiMouseSensitive Members
        /// <summary>
        /// Enables collection of mouse events.
        /// </summary>
        /// <param name="args"></param>
        public override void MouseEnter(GuiMouseSensitiveArgs args)
        {
            base.MouseEnter(args);
            args.GuiManager.MouseMotionListeners.Add(this);
        }

        /// <summary>
        /// Stops listening to mouse events.
        /// </summary>
        /// <param name="args"></param>
        public override void MouseExit(GuiMouseSensitiveArgs args)
        {
            base.MouseExit(args);
            args.GuiManager.MouseMotionListeners.Remove(this);
        }
        #endregion

        #region IGuiMouseListener Members
        private PointF screenPoint;
        private PointF mousePoint;

        public float MouseListenerPriority
        {
            get { return this.Z; }
        }

        public void MouseDown(GuiMouseEventArgs args)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void MouseMotion(GuiMouseEventArgs args)
        {
            // Figure out translate point
            float x = args.Point.X
				- this.screenPoint.X - this.ViewportOffset.X;
            float y = args.Point.Y
				- this.screenPoint.Y - this.ViewportOffset.Y;

            // Save the point
            mousePoint = new PointF(x, y);
        }

        public void MouseUp(GuiMouseEventArgs args)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        #endregion
    }
}
