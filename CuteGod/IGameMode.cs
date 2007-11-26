using BooGame.Input;
using MfGames.Sprite3;
using System;
using System.Drawing;

namespace CuteGod
{
    /// <summary>
    /// Defines a game mode for the system. A mode is basically
    /// significant functionality, such as new game, playing, end game,
    /// high scores, main menu, etc.
    /// </summary>
    public interface IGameMode
    {
        #region Activation and Deactivation
        /// <summary>
        /// Sets the size of the game mode (based on the screen).
        /// </summary>
        SizeF Size { set; }

        /// <summary>
        /// Indicates to the game mode that it is activated.
        /// </summary>
        void Activate();
        
        /// <summary>
        /// Triggered when the game mode is deactivated.
        /// </summary>
        void Deactivate();
        #endregion

        #region Drawing
        /// <summary>
        /// Triggers a rendering of the game mode.
        /// </summary>
        /// <param name="args"></param>
        void Draw(DrawingArgs args);
        #endregion

        #region Updating
        /// <summary>
        /// Triggers an update of the mode's state.
        /// </summary>
        /// <param name="args"></param>
        void Update(UpdateArgs args);
        #endregion

		#region Controller
		/// <summary>
		/// Triggered when the user releases or presses a key.
		/// </summary>
		void Key(KeyboardEventArgs args);
		#endregion
    }
}
