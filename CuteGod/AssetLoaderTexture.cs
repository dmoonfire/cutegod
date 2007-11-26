using BooGame;
using BooGame.Video;
using MfGames.Sprite3;
using MfGames.Sprite3.BooGameBE;

namespace CuteGod
{
	/// <summary>
	/// Defers the loading of textures by the use of the AssetLoader
	/// class.
	/// </summary>
	public class AssetLoaderTexture
	: IAssetLoader
	{
		#region Constructors
		public AssetLoaderTexture(string path)
		{
			Filename = path;
		}

        public AssetLoaderTexture(string path, ColorF color)
        {
			Filename = path;
			this.color = color;
			hasColor = true;
        }
		#endregion

		#region Properties
		private string key;
		private string filename;
		private ColorF color;
		private bool hasColor = false;

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
            // Load the drawable using PhysFs
            Texture texture = Core.Backend.CreateTexture(filename);
            TextureDrawable drawable = new TextureDrawable(texture);

            // Load the texture into the drawable manager
			if (hasColor)
				Game.DrawableManager.Register(key, drawable, color);
			else
				Game.DrawableManager.Register(key, drawable);
		}
		#endregion
	}
}
