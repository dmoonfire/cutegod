using BooGame.Input;
using C5;
using MfGames.Sprite3;
using MfGames.Sprite3.Gui;
using MfGames.Sprite3.PlanetCute;
using MfGames.Utility;
using System;
using System.Drawing;

namespace CuteGod.Play
{
	/// <summary>
	/// Common full-screen display mode for showing statistics about
	/// the game. This is used by a few modes to show status.
	/// </summary>
	public abstract class DisplayStatusMinorMode
	: Logable, IPlayMinorMode
	{
        #region Constants
        protected const float NewFontSize = 50;
        protected const float TitleFontSize = 80;
        protected const float DescriptionFontSize = 50;
        protected const int BlockHeight = 171;
        protected const int BlockWidth = 101;
        protected const int BlockOffsetX = 101;
        protected const int BlockOffsetY = 80;
        protected const int BlockOffsetZ = 40;
        protected const float WindowPadding = 20;
        protected const float FullTextXOffset = 120;
        protected const float FullTextYOffset = 80;
        protected const float FullTextSize = 80;
        #endregion

		#region Constructors
		protected DisplayStatusMinorMode()
		{
            // Create a sprite viewport to handle everything
            sprites = new SpriteViewport();

            // Create the heart and star icons
            heartSprite = AssetLoader.Instance.CreateSprite("Heart");
            sprites.Add(heartSprite);

            heartText = new TextDrawableSprite(Game.GetFont(FullTextSize));
            heartText.Tint = Constants.HeartColor;
            heartText.Text = Game.State.Hearts.ToString();
			heartText.Alignment = ContentAlignment.MiddleLeft;
            sprites.Add(heartText);

            starSprite = AssetLoader.Instance.CreateSprite("Star");
            sprites.Add(starSprite);

            starText = new TextDrawableSprite(Game.GetFont(FullTextSize));
            starText.Tint = Constants.StarColor;
            starText.Text = Game.State.Stars.ToString();
			starText.Alignment = ContentAlignment.MiddleLeft;
            sprites.Add(starText);

            chestSprite = AssetLoader.Instance.CreateSprite(ChestDrawableName);
            sprites.Add(chestSprite);

            chestText = new TextDrawableSprite(Game.GetFont(FullTextSize));
            chestText.Tint = Constants.ChestColor;
            chestText.Text = "0";
			chestText.Alignment = ContentAlignment.MiddleLeft;
            sprites.Add(chestText);

			// Create the viewport
			viewport = new SpriteViewport();
			viewport.ClipContents = true;
			sprites.Add(viewport);

			// Add the text elements
            subtitle = new TextDrawableSprite(
				Game.GetFont(NewFontSize), "");
            subtitle.Tint = Color.FromArgb(128, Color.White);
			subtitle.Alignment = ContentAlignment.TopCenter;
			subtitle.Visible = false;
            viewport.Add(subtitle);

			// Create the title text
            title = new TextDrawableSprite(
				Game.GetFont(TitleFontSize), "");
            title.Tint = Color.White;
			title.Alignment = ContentAlignment.TopCenter;
			title.Visible = false;
            viewport.Add(title);

			// Create the title text
            text = new TextLayoutDrawableSprite(
				Game.GetFont(DescriptionFontSize), "");
            text.Tint = Color.White;
			text.Alignment = ContentAlignment.TopCenter;
            viewport.Add(text);
		}
		#endregion

        #region Activation and Deactivation
        /// <summary>
        /// Sets the size of the game mode (based on the screen).
        /// </summary>
        public virtual SizeF Size
        {
            set
            {
                // Set the sprite size
                sprites.Size = value; 

                // Move the hearts and stars to the bottom
                heartSprite.Point = new PointF(
                    WindowPadding,
                    WindowPadding);
                heartText.Point = new PointF(
                    heartSprite.Point.X + FullTextXOffset,
                    heartSprite.Point.Y + FullTextYOffset);

                starSprite.Point = new PointF(
					WindowPadding,
					heartSprite.Point.Y + BlockHeight);
                starText.Point = new PointF(
                    starSprite.Point.X + FullTextXOffset,
                    starSprite.Point.Y + FullTextYOffset + 10);

                chestSprite.Point = new PointF(
					WindowPadding,
					value.Height - WindowPadding - BlockHeight);
                chestText.Point = new PointF(
                    chestSprite.Point.X + FullTextXOffset,
                    chestSprite.Point.Y + FullTextYOffset + 10);

				// Change the viewport
				float x = WindowPadding
					+ BlockWidth
					+ FullTextSize * 2f;
				viewport.Point = new PointF(x, WindowPadding);
				viewport.Size = new SizeF(
					value.Width - x - WindowPadding,
					value.Height - WindowPadding * 2);

				// Change the text
				float cx = viewport.Size.Width / 2;
				subtitle.Point = new PointF(cx, 20);
				title.Point = new PointF(cx, 80);
				text.Point = new PointF(cx, 200);

				// Figure out the message display size. This is used
				// for the word-wrapping processing.
				textRegion = new RectangleF(
					0, 200,
					viewport.Size.Width,
					viewport.Size.Height - 200);
				text.Region = textRegion;
            }
        }

        /// <summary>
        /// Indicates to the game mode that it is activated.
        /// </summary>
        public virtual void Activate()
        {
        }

        /// <summary>
        /// Triggered when the game mode is deactivated.
        /// </summary>
        public virtual void Deactivate()
        {
        }
        #endregion

		#region Properties
        private SpriteViewport sprites, viewport;
        private ISprite heartSprite, starSprite, chestSprite;
        private TextDrawableSprite heartText, starText, chestText;
		private TextDrawableSprite subtitle, title;
		private TextLayoutDrawableSprite text;
		private int chestCounter;
		private RectangleF textRegion;

		public int ChestCounter
		{
			get { return chestCounter; }
			set { chestCounter = value; }
		}

		protected virtual string ChestDrawableName
		{
			get { return "Chest Closed"; }
		}

		protected ISprite ChestSprite
		{
			get { return chestSprite; }
		}

		protected bool ChestVisible
		{
			get { return chestSprite.Visible; }
			set
			{
				chestSprite.Visible = value;
				chestText.Visible = value;
			}
		}

		/// <summary>
		/// Contains the text of the subtitle sprite.
		/// </summary>
		protected string Subtitle
		{
			get { return subtitle.Text; }
			set
			{
				subtitle.Text = value ?? "";
				subtitle.Visible = subtitle.Text.Length > 0;
			}
		}

		/// <summary>
		/// Contains the top-level sprite element, including all the
		/// text.
		/// </summary>
		protected SpriteViewport Sprites
		{
			get { return sprites; }
		}

		/// <summary>
		/// Contains the text of the text sprite.
		/// </summary>
		protected string Text
		{
			get { return text.Text; }
			set
			{
				text.Text = value ?? "";
				text.Visible = title.Text.Length > 0;
			}
		}

		/// <summary>
		/// Contains the text of the title sprite.
		/// </summary>
		protected string Title
		{
			get { return title.Text; }
			set
			{
				title.Text = value ?? "";
				title.Visible = title.Text.Length > 0;
			}
		}

		/// <summary>
		/// This is the clipped viewport for the screen that allows
		/// any other sprites to be added.
		/// </summary>
		protected SpriteViewport Viewport
		{
			get { return viewport; }
		}
		#endregion

		#region Drawing
        /// <summary>
        /// Contains the background color for the mode.
        /// </summary>
        public Color BackgroundColor
        {
            get { return Color.FromArgb(222, Color.Black); }
        }

        /// <summary>
        /// Triggers a rendering of the game mode.
        /// </summary>
        /// <param name="args"></param>
        public virtual void Draw(DrawingArgs args)
        {
			// Draw the sprites
            sprites.Draw(args);
        }
        #endregion

        #region Updating
		private double secondsRemaining;
		private bool countingDown = false;

		protected bool CountingDown
		{
			get { return countingDown; }
			set { countingDown = value; }
		}

		protected double SecondsRemaining
		{
			get { return secondsRemaining; }
			set { secondsRemaining = value; }
		}

        /// <summary>
        /// Triggers an update of the mode's state.
        /// </summary>
        /// <param name="args"></param>
        public virtual void Update(UpdateArgs args)
        {
            // Update the sprites
            sprites.Update(args);

            // Update the text
            heartText.Text = Game.State.Hearts.ToString();
            starText.Text = Game.State.Stars.ToString();
			chestText.Text = chestCounter.ToString();

			// Figure out the pause after everything is done
			if (countingDown)
			{
				// Reduce the time
				secondsRemaining -= args.SecondsSinceLastUpdate;

				if (secondsRemaining < 0)
				{
					// Change the mini mode to the new stage
					Timeout();
				}
			}
        }

		/// <summary>
		/// Triggered when the mode times out. 
		/// </summary>
		protected virtual void Timeout()
		{
		}
        #endregion

		#region Controller
		/// <summary>
		/// Triggered when the user releases or presses a key.
		/// </summary>
		public virtual void Key(KeyboardEventArgs args)
		{
			// No timeout if (countingDown)
			if (args.Pressed)
				Timeout();
		}
		#endregion

		#region Mouse Events
        /// <summary>
        /// Contains the priority of the mouse listener. This is typically
        /// the Z-order for most sprites, but can be altered.
        /// </summary>
        public float MouseListenerPriority { get { return 1; } }

        /// <summary>
        /// Triggered when an object extending this class is placed
        /// in GuiManager.MouseDownListeners and the mouse down event
        /// is fired.
        /// </summary>
        /// <param name="args"></param>
        public void MouseDown(GuiMouseEventArgs args)
		{
		}

        /// <summary>
        /// Triggered when an object extending this class is placed
        /// in GuiManager.MouseDownListeners and the mouse motion event
        /// is fired.
        /// </summary>
        /// <param name="args"></param>
        public void MouseMotion(GuiMouseEventArgs args)
		{
		}

        /// <summary>
        /// Triggered when an object extending this class is placed
        /// in GuiManager.MouseDownListeners and the mouse up event
        /// is fired.
        /// </summary>
        /// <param name="args"></param>
        public void MouseUp(GuiMouseEventArgs args)
		{
			Timeout();
		}
		#endregion
	}
}
