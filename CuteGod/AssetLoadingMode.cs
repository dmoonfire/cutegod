using BooGame.Input;
using MfGames.Sprite3;
using System;
using System.Drawing;

namespace CuteGod.Play
{
    /// <summary>
	/// Defers the processing until all the elements are properly
	/// loaded into memory. Then it kicks over to the main menu.
    /// </summary>
    public class AssetLoadingMode
        : IGameMode
    {
        #region Constants
        private float FontSize = 200;
        private float SmallFontSize = 70;
        #endregion

        #region Constructors
        /// <summary>
        /// Constructs the minor mode and sets up the internal
        /// sprites.
        /// </summary>
        public AssetLoadingMode()
        {
            // Create the game over text
            text = new TextDrawableSprite(Game.GetFont(FontSize),
				"Loading");
            text.Tint = Color.White;
			text.Alignment = ContentAlignment.MiddleCenter;
            sprites.Add(text);

            text2 = new TextDrawableSprite(Game.GetFont(SmallFontSize),
				"100%");
            text2.Tint = Color.White;
			text2.Alignment = ContentAlignment.BottomLeft;
            sprites.Add(text2);
        }
        #endregion

        #region Activation and Deactivation
        /// <summary>
        /// Indicates to the game mode that it is activated.
        /// </summary>
        public void Activate()
        {
			// Cut the load rate to as fast as possible
			AssetLoader.Instance.LoadRate = 0;
        }

        /// <summary>
        /// Triggered when the game mode is deactivated.
        /// </summary>
        public void Deactivate()
        {
        }
        #endregion

        #region Drawing and Activation
        public void Draw(DrawingArgs args)
        {
            sprites.Draw(args);
        }

        public void Update(UpdateArgs args)
        {
			// See if we are done
			if (AssetLoader.Instance.IsFinishedLoading)
			{
				//Game.GameMode = new NewGameMode();
				Game.GameMode = new MainMenuMode();
				return;
			}

			// Load some assets
			AssetLoader.Instance.Update(args.SecondsSinceLastUpdate);

			// Set the color
			int value = (int) (255.0 * AssetLoader.Instance.Pending);
			text.Tint = Color.FromArgb(value, value, value);
			text2.Tint = text.Tint;

			// Update the percentage
			text2.Text = String
				.Format("{0:N0}%", AssetLoader.Instance.Pending * 100);

			// Update the sprites
            sprites.Update(args);
        }
        #endregion

		#region Controller
		/// <summary>
		/// Triggered when the user releases or presses a key.
		/// </summary>
		public void Key(KeyboardEventArgs args)
		{
		}
		#endregion

        #region Properties
        private SpriteViewport sprites = new SpriteViewport();
        private TextDrawableSprite text, text2;

        /// <summary>
        /// Sets the size of the game mode (based on the screen).
        /// </summary>
        public SizeF Size
        {
            set
            {
                // Set the sprite
                sprites.Size = value;

                // Center the window
                float cx = value.Width / 2;
                float cy = value.Height / 2;

                // Center the game over
                text.Point = new PointF(cx, cy * 0.75f);
                text2.Point = new PointF(20, value.Height - 30);
            }
        }
        #endregion
    }
}
