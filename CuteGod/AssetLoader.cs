using BooGame.Video;
using C5;
using MfGames.Sprite3;
using MfGames.Sprite3.Backends;
using MfGames.Sprite3.PlanetCute;
using MfGames.Utility;
using System;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;

namespace CuteGod
{
	/// <summary>
	/// Manager for loading assets into memory. This allows for the
	/// textures, sounds, and fonts to be loaded in the background
	/// through various modes with reflection to enable a mode to
	/// query the status.
	/// </summary>
	public class AssetLoader
	: Logable, IBlockFactory
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
			DirectoryInfo sounds = new DirectoryInfo(
				Path.Combine("Assets", "Sounds"));

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

				// Load the type information, if we have one
				if (File.Exists(Path.Combine(di.FullName, "block.xml")))
				{
					// Load the block information, this controls the
					// color of the blocks.
					LoadAssetTypeMeta(spriteType, 
						new FileInfo(Path.Combine(di.FullName, "block.xml")));
				}

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

			// Look for sounds and music
			foreach (DirectoryInfo di in sounds.GetDirectories())
			{
				// Go through each file in this directory. The
				// directory name is the category of music while the
				// individual files are randomly selected from the
				// category.
				// "Vorbis is the new mp3"
				foreach (FileInfo fi in di.GetFiles("*.ogg"))
				{
					// We want to register this file with the sound
					// manager. Since the SoundManager doesn't load
					// things, we are just registering directory with
					// it.
					Game.Sound.Register(di.Name, fi);
				}
			}
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
		/// Constructs a block and assigns the proper colors.
		/// </summary>
		public Block CreateBlock(string key)
		{
			// See if we have the sprite type
			if (!sprites.Contains(key))
				throw new Exception("No such block sprite key: " + key);

			// Create the sprite
			ISprite sprite = CreateSprite(key);

			// Create a block and set it up
			Block block = new Block(sprite);
			OnImportBlock(block);

			// Return the block
			return block;
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

		/// <summary>
		/// Imports the block into the system, assigning any values
		/// that needs to be assigned.
		/// </summary>
		public void OnImportBlock(Block block)
		{
			// Get the asset key
			if (block == null ||
				block.Sprite == null ||
				block.Sprite.ID == null)
				return;

			string key = block.Sprite.ID;

			if (!sprites.Contains(key))
				return;

			AssetSpriteType ast = sprites[key];

			// Assign the colors
			block.Color = ast.Color;

			// Figure out heights
			if (key.Contains("Tall"))
				block.Height = 2;

			// Figure out shadows
			block.CastsShadows = key.Contains("Block") ||
				key.Contains("Wall") ||
				key.Contains("Door") ||
				key.Contains("Window");
		}
		#endregion

		#region Meta Data Processing
		/// <summary>
		/// Reads the asset type information to gather information
		/// such as sprite color or other information.
		/// </summary>
		private void LoadAssetTypeMeta(AssetSpriteType type,
			FileInfo file)
		{
			// Open up the file
			using (StreamReader sr = file.OpenText())
			{
				// Wrap it in an XML reader
				XmlTextReader xml = new XmlTextReader(sr);

				// Loop through it
				while (xml.Read())
				{
					// Check for open elements
					if (xml.NodeType == XmlNodeType.Element)
					{
						switch (xml.LocalName)
						{
							case "color":
								// Assign a color to this block
								type.Color = Color.FromArgb(
									Int32.Parse(xml["r"]),
									Int32.Parse(xml["g"]),
									Int32.Parse(xml["b"]));
								break;
						}
					}
				}
			}
		}
		#endregion
	}
}
