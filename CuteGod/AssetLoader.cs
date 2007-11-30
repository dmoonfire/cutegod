using BooGame.Video;
using C5;
using MfGames.Sprite3;
using MfGames.Sprite3.Backends;
using MfGames.Utility;
using System;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;

namespace CuteGod
{
	/// <summary>
	/// Manager for loading assets into memory. This allows for the
	/// textures, sounds, and fonts to be loaded in the background
	/// through various modes with reflection to enable a mode to
	/// query the status.
	/// </summary>
	public class AssetLoader
	: Logable, ISpriteFactory
	{
		#region Singleton
		private static AssetLoader instance;

		/// <summary>
		/// Contains the singleton instance for this loader.
		/// </summary>
		public static AssetLoader Instance
		{
			get
			{
				// Make sure we are created
				if (instance == null)
					instance = new AssetLoader();

				return instance;
			}
		}
		#endregion

		#region Default Assets
		/// <summary>
		/// Function to queue up all the assets required for the
		/// functioning of the system.
		/// </summary>
		public void Queue()
		{
			// Scan the Assets/Images directory for drawables
			DirectoryInfo images = new DirectoryInfo(
				Path.Combine("Assets", "Images"));

			// Load the individual images into memory
			foreach (FileInfo fi in images.GetFiles("*.png"))
			{
				// Load these images
				QueueDrawable("Assets/Images/" + fi.Name);
			}

			// Load the individual directories
			Regex regex = new Regex(@"(\s+\d+)?\.png");

			foreach (DirectoryInfo di in images.GetDirectories())
			{
				// Ignore some common ones
				if (di.Name.StartsWith("."))
					continue;

				// Get the asset sprite type for this directory
				string key = di.Name;

				if (!sprites.Contains(key))
					sprites[key] = new AssetSpriteType(key);

				AssetSpriteType spriteType = sprites[key];

				// Go through each file in this directory
				foreach (FileInfo fi in di.GetFiles("*.png"))
				{
					// Figure out the components
					string path = String.Format("Assets/Images/{0}/{1}",
						key, fi.Name);
					string key2 = regex.Replace(fi.Name, "");

					// Get the asset sprite
					AssetSprite sprite = spriteType.GetAssetSprite(key2);

					// Queue loading the drawable
					Queue(new AssetLoaderSprite(sprite, path));
				}
			}

#if REMOVED
            // Set up the core blocks
            QueueDrawable("PlanetCute/Custom/Immobile Block.png",
                Color.Black);

            // These are the blocks the user will be moving
            QueueDrawable("PlanetCute/Grass Block.png",
                Color.FromArgb(95, 193, 72));
            QueueDrawable("PlanetCute/Custom/Water Block.png",
                Color.FromArgb(97, 119, 221));
            QueueDrawable("PlanetCute/Dirt Block.png",
                Color.FromArgb(193, 143, 72));
            QueueDrawable("PlanetCute/Custom/Dead Grass Block.png",
                Color.DarkRed);
            QueueDrawable("PlanetCute/Custom/Dead Dirt Block.png",
                Color.DarkRed);
            QueueDrawable("PlanetCute/Custom/Dead Water Block.png",
                Color.DarkRed);

			// Sealed blocks are immobile and white, nor can they be
			// picked up or dropped on.
            QueueDrawable("PlanetCute/Custom/Sealed Water.png",
				Color.White);
            QueueDrawable("PlanetCute/Custom/Sealed Grass.png",
				Color.White);
            QueueDrawable("PlanetCute/Custom/Sealed Dirt.png",
				Color.White);
			QueueDrawable("PlanetCute/Custom/Invisible.png");

            // These are the construction blocks
            QueueDrawable("PlanetCute/Brown Block.png", Color.White);
            QueueDrawable("PlanetCute/Plain Block.png", Color.White);
            QueueDrawable("PlanetCute/Ramp East.png", Color.White);
            QueueDrawable("PlanetCute/Ramp North.png", Color.White);
            QueueDrawable("PlanetCute/Ramp South.png", Color.White);
            QueueDrawable("PlanetCute/Ramp West.png", Color.White);
            QueueDrawable("PlanetCute/Door Tall Closed.png",
				Color.White);
            QueueDrawable("PlanetCute/Door Tall Open.png",
				Color.White);
            QueueDrawable("PlanetCute/Rock.png",
				Color.White);
            QueueDrawable("PlanetCute/Roof East.png",
				Color.White);
            QueueDrawable("PlanetCute/Roof North East.png",
				Color.White);
            QueueDrawable("PlanetCute/Roof North West.png",
				Color.White);
            QueueDrawable("PlanetCute/Roof North.png",
				Color.White);
            QueueDrawable("PlanetCute/Roof South East.png",
				Color.White);
            QueueDrawable("PlanetCute/Roof South West.png",
				Color.White);
            QueueDrawable("PlanetCute/Roof South.png",
				Color.White);
            QueueDrawable("PlanetCute/Roof West.png",
				Color.White);
            QueueDrawable("PlanetCute/Stone Block Tall.png",
				Color.White);
            QueueDrawable("PlanetCute/Stone Block.png",
				Color.White);
            QueueDrawable("PlanetCute/Tree Short.png",
				Color.White);
            QueueDrawable("PlanetCute/Tree Tall.png",
				Color.White);
            QueueDrawable("PlanetCute/Tree Ugly.png",
				Color.White);
            QueueDrawable("PlanetCute/Wall Block Tall.png",
				Color.White);
            QueueDrawable("PlanetCute/Wall Block.png",
				Color.White);
            QueueDrawable("PlanetCute/Window Tall.png",
				Color.White);
            QueueDrawable("PlanetCute/Wood Block.png",
				Color.White);

            // Selector
            QueueDrawable("PlanetCute/Selector.png", Color.Yellow);
            QueueDrawable("PlanetCute/Custom/Invalid Selector.png",
                Color.Yellow);
#endif
		}
		#endregion

		#region Queues
		private double secondsRemaining = 0.1;
		private double loadRate = 0.1;
		private int queueCount;
		private LinkedList<IAssetLoader> queue =
			new LinkedList<IAssetLoader>();

		/// <summary>
		/// Returns true if the assets have been loaded.
		/// </summary>
		public bool IsFinishedLoading
		{
			get { return queue.Count == 0; }
		}

		/// <summary>
		/// Contains the rate to load textures.
		/// </summary>
		public double LoadRate
		{
			get { return loadRate; }
			set { loadRate = value; }
		}

		/// <summary>
		/// Returns a double with the amount remaining to be
		/// processed. This will be 0 when it is done.
		/// </summary>
		public double Pending
		{
			get
			{
				return (double) queue.Count / (double) queueCount;
			}
		}

		/// <summary>
		/// Processes one element from the queue.
		/// </summary>
		public void ProcessOne()
		{
			// Ignore empty queues
			if (queue.Count == 0)
				return;

			// Grab the first one
			IAssetLoader loader = queue.RemoveFirst();
			loader.Load();
		}

		/// <summary>
		/// Queues a loader to load as required by the system. Queue
		/// is implemented as a first in/first out approach to
		/// loading.
		/// </summary>
		public void Queue(IAssetLoader loader)
		{
			queue.InsertLast(loader);
			queueCount++;
		}

		/// <summary>
		/// Attempts to load an asset, spreading it out over time to
		/// reduce the load on the server.
		/// </summary>
		public void Update(double secondsSinceLastUpdate)
		{
			// Lower the counter and finish if we are still delaying
			secondsRemaining -= secondsSinceLastUpdate;

			if (secondsRemaining > 0)
				return;

			// Set up the new time
			secondsRemaining = loadRate;
			ProcessOne();
		}
		#endregion

		#region Textures
        /// <summary>
        /// Queues a single drawable into the system.
        /// </summary>
        /// <param name="path"></param>
        private void QueueDrawable(string path)
        {
			Queue(new AssetLoaderTexture(path));
        }
		#endregion

		#region Sprite Creation
		private HashDictionary<string, IDrawable> drawables =
			new HashDictionary<string, IDrawable> ();
		private HashDictionary<string, AssetSpriteType> sprites =
			new HashDictionary<string, AssetSpriteType>();

		/// <summary>
		/// Contains a hash of a string to drawable mapping.
		/// </summary>
		public IDictionary<string, IDrawable> Drawables
		{
			get { return drawables; }
		}

		/// <summary>
		/// Contains a hash tree of the loaded sprites.
		/// </summary>
		public IDictionary<string, AssetSpriteType> Sprites
		{
			get { return sprites; }
		}

		/// <summary>
		/// Loads a single drawable and returns it.
		/// </summary>
		public IDrawable CreateDrawable(string key)
		{
			// See if we have the drawable
			if (drawables.Contains(key))
				return drawables[key];

			// Otherwise, attempt to load it

			// We can't find it
			throw new Exception("Cannot load a drawable: " + key);
		}

		/// <summary>
		/// Constructs a sprite from the given key, caching the
		/// drawable data if needed.
		/// </summary>
		public ISprite CreateSprite(string key)
		{
			// See if we have the sprite type
			if (!sprites.Contains(key))
				throw new Exception("No such sprite key: " + key);

			// Grab the asset type
			AssetSpriteType ast = sprites[key];
			return ast.CreateRandomSprite();
		}
		#endregion
	}
}
