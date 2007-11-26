using BooGame.Video;
using C5;
using System.Drawing;

namespace CuteGod
{
	/// <summary>
	/// Manager for loading assets into memory. This allows for the
	/// textures, sounds, and fonts to be loaded in the background
	/// through various modes with reflection to enable a mode to
	/// query the status.
	/// </summary>
	public static class AssetLoader
	{
		#region Default Assets
		/// <summary>
		/// Function to queue up all the assets required for the
		/// functioning of the system.
		/// </summary>
		public static void Queue()
		{
			// Main menu elements
			QueueDrawable("Assets/quit.png");
			QueueDrawable("Assets/credits.png");
			QueueDrawable("Assets/help.png");
			QueueDrawable("Assets/high-scores.png");
			QueueDrawable("Assets/new-game.png");
			QueueDrawable("Assets/resume-game.png");
			QueueDrawable("Assets/settings.png");

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
            QueueDrawable("PlanetCute/Door Tall Closed.png", Color.White);
            QueueDrawable("PlanetCute/Door Tall Open.png", Color.White);
            QueueDrawable("PlanetCute/Rock.png", Color.White);
            QueueDrawable("PlanetCute/Roof East.png", Color.White);
            QueueDrawable("PlanetCute/Roof North East.png", Color.White);
            QueueDrawable("PlanetCute/Roof North West.png", Color.White);
            QueueDrawable("PlanetCute/Roof North.png", Color.White);
            QueueDrawable("PlanetCute/Roof South East.png", Color.White);
            QueueDrawable("PlanetCute/Roof South West.png", Color.White);
            QueueDrawable("PlanetCute/Roof South.png", Color.White);
            QueueDrawable("PlanetCute/Roof West.png", Color.White);
            QueueDrawable("PlanetCute/Stone Block Tall.png", Color.White);
            QueueDrawable("PlanetCute/Stone Block.png", Color.White);
            QueueDrawable("PlanetCute/Tree Short.png", Color.White);
            QueueDrawable("PlanetCute/Tree Tall.png", Color.White);
            QueueDrawable("PlanetCute/Tree Ugly.png", Color.White);
            QueueDrawable("PlanetCute/Wall Block Tall.png", Color.White);
            QueueDrawable("PlanetCute/Wall Block.png", Color.White);
            QueueDrawable("PlanetCute/Window Tall.png", Color.White);
            QueueDrawable("PlanetCute/Wood Block.png", Color.White);

            // Characters
            QueueDrawable("PlanetCute/Character Boy.png");
            QueueDrawable("PlanetCute/Character Cat Girl.png");
            QueueDrawable("PlanetCute/Character Horn Girl.png");
            QueueDrawable("PlanetCute/Character Pink Girl.png");
            QueueDrawable("PlanetCute/Character Princess Girl.png");
            QueueDrawable("PlanetCute/Enemy Bug.png");

            QueueDrawable("PlanetCute/SpeechBubble.png");

            // These are the icons
            QueueDrawable("PlanetCute/Gem Blue.png");
            QueueDrawable("PlanetCute/Gem Green.png");
            QueueDrawable("PlanetCute/Gem Orange.png");
            QueueDrawable("PlanetCute/Heart.png");
            QueueDrawable("PlanetCute/Star.png");
            QueueDrawable("PlanetCute/Key.png");
            QueueDrawable("PlanetCute/Chest Closed.png");
            QueueDrawable("PlanetCute/Chest Open.png");
            QueueDrawable("PlanetCute/Chest Lid.png");

            QueueDrawable("PlanetCute/Custom/Half Heart.png");
            QueueDrawable("PlanetCute/Custom/Mini Heart.png");
            QueueDrawable("PlanetCute/Custom/Mini Star.png");
            QueueDrawable("PlanetCute/Custom/Mini Bug.png");
            QueueDrawable("PlanetCute/Custom/Mini Clock.png");

            // These are the shadows
            QueueDrawable("PlanetCute/Shadow East.png");
            QueueDrawable("PlanetCute/Shadow North East.png");
            QueueDrawable("PlanetCute/Shadow North West.png");
            QueueDrawable("PlanetCute/Shadow North.png");
            QueueDrawable("PlanetCute/Shadow Side West.png");
            QueueDrawable("PlanetCute/Shadow South East.png");
            QueueDrawable("PlanetCute/Shadow South West.png");
            QueueDrawable("PlanetCute/Shadow South.png");
            QueueDrawable("PlanetCute/Shadow West.png");

            // Selector
            QueueDrawable("PlanetCute/Selector.png", Color.Yellow);
            QueueDrawable("PlanetCute/Custom/Invalid Selector.png",
                Color.Yellow);
		}
		#endregion

		#region Queues
		private static double secondsRemaining = 0.1;
		private static double loadRate = 0.1;
		private static int queueCount;
		private static LinkedList<IAssetLoader> queue =
			new LinkedList<IAssetLoader>();

		/// <summary>
		/// Returns true if the assets have been loaded.
		/// </summary>
		public static bool IsFinishedLoading
		{
			get { return queue.Count == 0; }
		}

		/// <summary>
		/// Contains the rate to load textures.
		/// </summary>
		public static double LoadRate
		{
			get { return loadRate; }
			set { loadRate = value; }
		}

		/// <summary>
		/// Returns a double with the amount remaining to be
		/// processed. This will be 0 when it is done.
		/// </summary>
		public static double Pending
		{
			get { return (double) queue.Count / (double) queueCount; }
		}

		/// <summary>
		/// Processes one element from the queue.
		/// </summary>
		public static void ProcessOne()
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
		public static void Queue(IAssetLoader loader)
		{
			queue.InsertLast(loader);
			queueCount++;
		}

		/// <summary>
		/// Attempts to load an asset, spreading it out over time to
		/// reduce the load on the server.
		/// </summary>
		public static void Update(double secondsSinceLastUpdate)
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
        private static void QueueDrawable(string path)
        {
			Queue(new AssetLoaderTexture(path));
        }

        /// <summary>
        /// Queues a single drawable into the system, setting up the
        /// internal elements.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="color"></param>
        /// <param name="height"></param>
        private static void QueueDrawable(string path, ColorF color)
        {
			Queue(new AssetLoaderTexture(path, color));
        }
		#endregion
	}
}
