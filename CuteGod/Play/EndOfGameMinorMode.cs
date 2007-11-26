using BooGame.Input;
using MfGames.Sprite3;
using MfGames.Sprite3.Gui;
using System;
using System.Drawing;

namespace CuteGod.Play
{
    /// <summary>
    /// Handles the functionality for telling the user that
    /// their game is over. This will switch to the EnterHighScoreMinorMode
    /// if the player has a high score.
    /// </summary>
    public class EndOfGameMinorMode
        : IPlayMinorMode
    {
        #region Constants
        private float FontSize = 300;
        #endregion

        #region Constructors
        /// <summary>
        /// Constructs the minor mode and sets up the internal
        /// sprites.
        /// </summary>
        public EndOfGameMinorMode()
        {
            // Create the game over text
            text = new TextDrawableSprite(Game.GetFont(FontSize),
				"Game\nOver");
            text.Color = Color.White;
			text.Alignment = ContentAlignment.MiddleCenter;
            sprites.Add(text);
        }
        #endregion

        #region Activation and Deactivation
        /// <summary>
        /// Indicates to the game mode that it is activated.
        /// </summary>
        public void Activate()
        {
			// Clear out the play mode to prevent returning
			Game.PlayMode = null;
        }

		private void Close()
		{
			Game.GameMode = new MainMenuMode();
		}

        /// <summary>
        /// Triggered when the game mode is deactivated.
        /// </summary>
        public void Deactivate()
        {
        }
        #endregion

		#region Controller
		/// <summary>
		/// Triggered when the user releases or presses a key.
		/// </summary>
		public void Key(KeyboardEventArgs args)
		{
			// We only care about keyboard presses
			if (!args.Pressed)
				return;

			Close();
		}
		#endregion

        #region Drawing and Activation
        public void Draw(DrawingArgs args)
        {
            sprites.Draw(args);
        }

        public void Update(UpdateArgs args)
        {
			// Lower the countdown
			secondsRemaining -= args.SecondsSinceLastUpdate;

			if (secondsRemaining <= 0)
			{
				Close();
				return;
			}

			// Update the sprites otherwise
            sprites.Update(args);
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
			Close();
		}
		#endregion

        #region Properties
        private SpriteViewport sprites = new SpriteViewport();
        private TextDrawableSprite text;
		private double secondsRemaining = Constants.EndOfGameDisplay;

        /// <summary>
        /// Contains the background color for the mode.
        /// </summary>
        public Color BackgroundColor
        {
            get { return Color.FromArgb(222, Color.DarkRed); }
        }

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
                float cy = value.Height / 3;

                // Center the game over
                text.Point = new PointF(cx, cy);
            }
        }
        #endregion
    }
}
