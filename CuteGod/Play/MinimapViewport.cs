using MfGames.Sprite3.PlanetCute;

using System;
using System.Drawing;

namespace CuteGod.Play
{
    /// <summary>
    /// Encapsulates the specific functionality for a minimap view
    /// of the entire game board.
    /// </summary>
    public class MinimapViewport
        : TopDownViewport
    {
        #region Constructors
        /// <summary>
        /// Constructs the minimap and sets the various paddings and
        /// values.
        /// </summary>
        public MinimapViewport(PlayMode playMode)
            : base(Game.State.Board)
        {
            // Save the value
            this.playMode = playMode;

            // Set the defaults
            Padding = 1;
            Margin = 5;
            BlockHeight = BlockWidth = 8;
            BackgroundColor = Color.FromArgb(128, Color.Black);

            // Set the filter
            BlockFilter = FilterGrabStack;

            // Set the viewport height. PlayMode will handle the changing
            // of the width based on the window size.
            Size = new SizeF(8, Extents.Height);
        }
        #endregion

        #region Properties
        private PlayMode playMode;
        #endregion

        #region Filters
        /// <summary>
        /// Delegate function to filter out the grab stack and only show
        /// the selector.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private bool FilterGrabStack(object sender, BlockFilterArgs args)
        {
            // If we have the selector, return true
            if (playMode.IsSelector(args.Block))
                return true;

            // See if this is in the play mode's grab stack
            if (playMode.IsInGrabStack(args.Block))
                return false;

            // Only care about blocks with height
            return args.Block.Height > 0;
        }
        #endregion
    }
}
