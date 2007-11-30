using BooGame;
using BooGame.Video;
using MfGames.Sprite3;
using MfGames.Sprite3.BooGameBE;
using System;

namespace CuteGod
{
	/// <summary>
	/// Defers the loading of a single drawable of a sprite until later.
	/// </summary>
	public class AssetLoaderSprite
	: IAssetLoader
	{
		#region Constructors
		public AssetLoaderSprite(AssetSprite sprite, string path)
		{
			this.sprite = sprite;
			Filename = path;
		}
		#endregion

		#region Properties
		private string key;
		private string filename;
		private AssetSprite sprite;

		/// <summary>
		/// Sets the filename for this loader.
		/// </summary>
		public string Filename
		{
			set
			{
				// Strip out the information to get the key
				filename = value;
				key = value;
				key = key.Substring(key.LastIndexOf("/") + 1);
				key = key.Substring(0, key.IndexOf("."));
			}
		}
		#endregion

		#region Loading
		/// <summary>
		/// Implements the actual loading process.
		/// </summary>
		public void Load()
		{
			// See if we already have it
			TextureDrawable drawable = null;

			if (AssetLoader.Instance.Drawables.Contains(key))
			{
				drawable =
					(TextureDrawable) AssetLoader.Instance.Drawables[key];
			}
			else // Create it
			{
				// Load the drawable using PhysFs
				Texture texture = Core.Backend.CreateTexture(filename);
				drawable = new TextureDrawable(texture);
				AssetLoader.Instance.Drawables[key] = drawable;
			}

			// Set the drawable in the asset sprite
			sprite.Drawables.Add(drawable);
		}
		#endregion
	}
}
