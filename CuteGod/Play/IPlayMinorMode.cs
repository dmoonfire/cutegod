using MfGames.Sprite3.Gui;
using System;
using System.Drawing;

namespace CuteGod.Play
{
    /// <summary>
    /// Represents a minor mode during the play mode.
    /// </summary>
    public interface IPlayMinorMode
	: IGameMode, IGuiMouseListener
    {
        #region Properties
        Color BackgroundColor { get; }
        #endregion
    }
}
