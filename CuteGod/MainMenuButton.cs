using BooGame;
using BooGame.Input;
using MfGames.Sprite3;
using MfGames.Sprite3.Backends;
using MfGames.Sprite3.Gui;
using System;
using System.Drawing;
using System.Text;

namespace CuteGod
{
	/// <summary>
	/// Represents the main menu button which changes color based on
	/// the mouse state.
	/// </summary>
    public class MainMenuButton
	: DrawableSprite, IGuiMouseSensitive, IGuiMouseListener
    {
        #region Constructors
        public MainMenuButton(IDrawable drawable)
            : base(drawable)
        {
        }
        #endregion

		#region Updating
		public event EventHandler Clicked;

		public int Alpha = 0;
		public bool IsActive = false;

		public override void Update(UpdateArgs args)
		{
			// Figure out what color we should be. We change every 10
			// seconds from the heart color to the star color and
			// back.
			double sec = ((double) DateTime.UtcNow.Ticks / 10000000.0) % 2.0;
			double range = Math.Abs(sec - 1.0);
			double ratio = (double) range / 1;
			double inverse = 1 - ratio;

			Color c = Color.FromArgb(
				(int) (Constants.HeartColor.R * ratio
					+ Constants.StarColor.R * inverse),
				(int) (Constants.HeartColor.G * ratio
					+ Constants.StarColor.G * inverse),
				(int) (Constants.HeartColor.B * ratio
					+ Constants.StarColor.B * inverse));

			// We change our state based on our active status
			if (Clicked == null)
			{
				Tint = Color.FromArgb(64, Color.White);
			}
			else if (IsActive)
			{
				Tint = Color.FromArgb(Alpha, c);
			}
			else
			{
				int a = (int) ((float) Alpha * 0.8f);
				Tint = Color.FromArgb(a, c);
			}
		}
		#endregion

        #region IGuiMouseSensitive Members
        private bool isMouseSensitive = true;

        public bool IsMouseSensitive
        {
            get { return isMouseSensitive; }
            set { isMouseSensitive = value; }
        }

        /// <summary>
        /// Contains the priority of the mouse listener. This is typically
        /// the Z-order for most sprites, but can be altered.
        /// </summary>
        public float MouseListenerPriority { get { return 1000; } }

        /// <summary>
        /// Triggered when an object extending this class is placed
        /// in GuiManager.MouseDownListeners and the mouse down event
        /// is fired.
        /// </summary>
        /// <param name="args"></param>
        public void MouseDown(GuiMouseEventArgs args)
		{
		}

        public virtual void MouseEnter(GuiMouseSensitiveArgs args)
        {
			if (Clicked != null)
			{
                // Add ourselves to the listener
                args.GuiManager.MouseUpListeners.Add(this);

				// Mark ourselves as active
				IsActive = true;
			}
        }

        public virtual void MouseExit(GuiMouseSensitiveArgs args)
        {	
			if (Clicked != null)
			{
                // Remove ourselves to the listener
                args.GuiManager.MouseUpListeners.Remove(this);

				// Mark ourselves as active
				IsActive = false;
			}
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
			if (Clicked != null)
			{
				// remove ourselves
                args.GuiManager.MouseUpListeners.Remove(this);
				Clicked(this, new EventArgs());
			}
		}
        #endregion
    }
}
