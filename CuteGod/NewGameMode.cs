using BooGame.Input;
using CuteGod.Play;
using MfGames.Sprite3;
using System;
using System.Drawing;

namespace CuteGod
{
    /// <summary>
    /// Initializes the game state for a new game and prepares the
    /// game to run. Later, this may ask questions of the user to
    /// allow them to customize their game.
    /// </summary>
    public class NewGameMode
        : IGameMode
    {
        #region Activation and Deactivation
        /// <summary>
        /// Sets the size of the game mode (based on the screen).
        /// </summary>
        public SizeF Size { set { } }

        /// <summary>
        /// Indicates to the game mode that it is activated.
        /// </summary>
        public void Activate()
        {
            // Create a new game state and create the initial board
            Game.State = new State();

            for (int i = 0; i < Constants.InitialStages; i++)
                Game.State.Board.GenerateStage();

            // Change the game mode to the play mode
            Game.GameMode = new PlayMode();
        }

        /// <summary>
        /// Triggered when the game mode is deactivated.
        /// </summary>
        public void Deactivate()
        {
        }
        #endregion

        #region Drawing
        /// <summary>
        /// Triggers a rendering of the game mode.
        /// </summary>
        /// <param name="args"></param>
        public void Draw(DrawingArgs args) { }
        #endregion

        #region Updating
        /// <summary>
        /// Triggers an update of the mode's state.
        /// </summary>
        /// <param name="args"></param>
        public void Update(UpdateArgs args) { }
        #endregion

		#region Controller
		/// <summary>
		/// Triggered when the user releases or presses a key.
		/// </summary>
		public void Key(KeyboardEventArgs args)
		{
		}
		#endregion
    }
}
