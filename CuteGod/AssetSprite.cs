using C5;
using MfGames.Sprite3.Backends;
using MfGames.Utility;
using System;

namespace CuteGod
{
	/// <summary>
	/// An AssetSprite is a collection of one or more drawables, used
	/// to build up an animated draable with potentially timing
	/// information.
	/// </summary>
	public class AssetSprite
	{
		#region Constructors
		public AssetSprite(string name)
		{
			this.name = name;
		}
		#endregion

		#region Properties
		private string name;
		private ArrayList<IDrawable> drawables = new ArrayList<IDrawable>();

		/// <summary>
		/// Contains a list of all the drawables for this sprite.
		/// </summary>
		public IList<IDrawable> Drawables
		{
			get { return drawables; }
		}

		/// <summary>
		/// Contains the name of this asset sprite.
		/// </summary>
		public string Name
		{
			get { return name; }
		}
		#endregion
	}
}
