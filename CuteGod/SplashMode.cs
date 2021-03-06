using BooGame;
using BooGame.Input;
using BooGame.Video;
using CuteGod.Play;
using MfGames.Sprite3;
using MfGames.Sprite3.Gui;
using MfGames.Utility;
using System;
using System.Drawing;

namespace CuteGod
{
    /// <summary>
	/// Implements the basic splash screen module which just loads an
	/// image with a black screen, brings it up, and then brings it
	/// out before switching to the next mode.
    /// </summary>
    public class SplashMode
	: Logable, IGameMode, IGuiMouseListener
    {
		#region Constructors
		public SplashMode(
			string imageFilename,
			double secondsDisplayed,
			IGameMode nextMode)
		{
			// Save the properties
			this.imageFilename = imageFilename;
			this.secondsDisplayed = secondsDisplayed;
			this.secondsRemaining = secondsDisplayed;
			this.nextMode = nextMode;
		}
		#endregion

		#region Properties
		private string imageFilename;
		private double secondsDisplayed;
		private double secondsRemaining;
		private double fade;
		private IGameMode nextMode;
		private SizeF screenSize;
		private Texture texture;
		#endregion

        #region Activation and Deactivation
        /// <summary>
        /// Sets the size of the game mode (based on the screen).
        /// </summary>
        public SizeF Size
		{
			set { screenSize = value; } 
		}

        /// <summary>
        /// Indicates to the game mode that it is activated.
        /// </summary>
        public void Activate()
        {
			// Create the texture
			texture = Core.Backend.CreateTexture(imageFilename);

			// Listen to some events
            Game.GuiManager.MouseUpListeners.Add(this);
        }

        /// <summary>
        /// Triggered when the game mode is deactivated.
        /// </summary>
        public void Deactivate()
        {
			// Trash our texture
			texture.Dispose();
			texture = null;

			// Stop listening
			Game.GuiManager.MouseUpListeners.Remove(this);
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
			// Switch modes
			Debug("Mouse up!");
			Game.GameMode = nextMode;
		}
		#endregion

		#region Controller
		/// <summary>
		/// Triggered when the user releases or presses a key.
		/// </summary>
		public void Key(KeyboardEventArgs args)
		{
			// Just stop the mode immediatelly
			Game.GameMode = nextMode;
		}
		#endregion

        #region Drawing
        /// <summary>
        /// Triggers a rendering of the game mode.
        /// </summary>
        /// <param name="args"></param>
        public void Draw(DrawingArgs args)
		{
			// Center the texture
			float x = (screenSize.Width - texture.Width) / 2;
			float y = (screenSize.Height - texture.Height) / 2;

			// Build up the tint color
			Color c = Color.FromArgb(255,
				(int) (255 * fade),
				(int) (255 * fade),
				(int) (255 * fade));

			// Draw the texture
			texture.Draw(x, y, c);
		}
        #endregion

        #region Updating
        /// <summary>
        /// Triggers an update of the mode's state.
        /// </summary>
        /// <param name="args"></param>
        public void Update(UpdateArgs args)
		{
			// Lower the display amount
			secondsRemaining -= args.SecondsSinceLastUpdate;
			AssetLoader.Instance.Update(args.SecondsSinceLastUpdate);

			// If this is negative switch to the next mode
			if (secondsRemaining < 0)
			{
				Game.GameMode = nextMode;
				return;
			}

			// We want to fade into the screen
			double offset = secondsRemaining - (secondsDisplayed / 2);
			double abs = Math.Abs(offset);
			double quarter = secondsDisplayed / 4;

			// If we are in the middle, just show it
			if (abs < quarter)
			{
				fade = 1; // 100%
				return;
			}

			// We are on the edge, so figure out the fade
			abs -= quarter;
			fade = 1 - abs / quarter;
		}
        #endregion
    }
}
