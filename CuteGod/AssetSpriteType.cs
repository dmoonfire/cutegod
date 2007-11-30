using C5;
using MfGames.Sprite3;
using MfGames.Sprite3.Backends;
using MfGames.Utility;
using System;

namespace CuteGod
{
	/// <summary>
	/// This is the top-most of the asset type tree. A single asset
	/// type would be like "Immobile" block where an AssetSprite would
	/// be "Immobile/Immobile Block A".
	/// </summary>
	public class AssetSpriteType
	{
		#region Constructors
		public AssetSpriteType(string name)
		{
			this.name = name;
		}
		#endregion

		#region Properties
		private string name;
		private ArrayList<AssetSprite> sprites =
			new ArrayList<AssetSprite>();

		/// <summary>
		/// Returns a random asset sprite inside this object.
		/// </summary>
		public AssetSprite RandomAssetSprite
		{
			get
			{
				// We can't return anything if there are none
				if (sprites.Count == 0)
					throw new Exception("AssetSpriteType is blank");

				// If we only have one, just return it
				if (sprites.Count == 1)
					return sprites[0];

				// Otherwise, use a random generator to return it.
				return sprites[Entropy.Next(sprites.Count)];
			}
		}

		/// <summary>
		/// This is a list of the asset sprites for this specific
		/// type.
		/// </summary>
		public IList<AssetSprite> AssetSprites
		{
			get { return sprites; }
		}

		/// <summary>
		/// Constructs a random sprite with all the appropriate timing
		/// and color information required.
		/// </summary>
		public ISprite CreateRandomSprite()
		{
			// Get a random type
			AssetSprite ras = RandomAssetSprite;

			if (ras.Drawables.Count == 0)
				throw new Exception("No defined sprites for " + ras.Name);

			// If we have one frame, its simple
			if (ras.Drawables.Count == 1)
			{
				ISprite ds = new DrawableSprite(ras.Drawables[0]);
				ds.ID = name;
			}

			// Otherwise, create a fixed rate (for now)
			FixedRateDrawableSprite fix = new FixedRateDrawableSprite();
			fix.FPS = 3;
			fix.ID = name;
			fix.Drawables.AddAll(ras.Drawables);
			return fix;
		}

		/// <summary>
		/// Loads a specific asset type, either using the data in the
		/// hash or creating a new one and adding it.
		/// </summary>
		public AssetSprite GetAssetSprite(string key)
		{
			// Scan through the list first
			foreach (AssetSprite sprite in sprites)
				if (sprite.Name == key)
					return sprite;

			// Create a new one
			AssetSprite nas = new AssetSprite(key);
			sprites.Add(nas);
			return nas;
		}
		#endregion
	}
}
