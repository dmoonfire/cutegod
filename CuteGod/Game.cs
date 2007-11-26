using C5;
using CuteGod.Play;
using MfGames.Sprite3;
using MfGames.Sprite3.Backends;
using MfGames.Sprite3.Gui;
using MfGames.Sprite3.PlanetCute;
using Nini.Config;
using System;
using System.Drawing;

namespace CuteGod
{
    /// <summary>
    /// The static accessor to the game state, graphics, and everything
    /// else. This organizes both the views and the models of the game.
    /// </summary>
    public static class Game
    {
        #region Graphics
        private static IBackend backend;
        private static BlockDrawableManager drawableManager =
            new BlockDrawableManager();
        private static SpriteManager sprites;
        private static GuiManager gui;

        /// <summary>
        /// Contains the sprite backend used for the actual
        /// rendering of sprites.
        /// </summary>
        public static IBackend Backend
        {
            get { return backend; }
            set
            {
                // Check for null
                if (value == null)
                    throw new Exception("Cannot assign a null backend");

                // Set the backend value for use later
                backend = value;

                // Create a new GUI manager and sprite manager
                sprites = new SpriteManager(backend);
                sprites.ClipContents = true;

                gui = new GuiManager(sprites);
            }
        }

        /// <summary>
        /// Contains the drawable manager used in the game.
        /// </summary>
        public static BlockDrawableManager DrawableManager
        {
            get { return drawableManager; }
        }

        /// <summary>
        /// Contains the GUI manager for this game.
        /// </summary>
        public static GuiManager GuiManager
        {
            get { return gui; }
        }

        /// <summary>
        /// Sets the size of the window.
        /// </summary>
        public static SizeF Size
        {
            get { return sprites.Size; }
            set
            {
                // See if we need to pass on the size
                bool sendChange = sprites.Size == value;

                // Set this viewport
                sprites.Size = value;

                // Tell the game mode about the size also
                if (sendChange && gameMode != null)
                    gameMode.Size = value;
            }
        }

        /// <summary>
        /// Contains the loaded sprite manager.
        /// </summary>
        public static SpriteManager SpriteManager
        {
            get { return sprites; }
        }
        #endregion

        #region Sounds
        private static SoundManager sound = new SoundManager();

        /// <summary>
        /// Gains access to the sound manager.
        /// </summary>
        public static SoundManager Sound
        {
            get { return sound; }
        }
        #endregion

        #region Fonts
        private static HashDictionary<float,IBackendFont> fonts =
            new HashDictionary<float,IBackendFont>();

        /// <summary>
        /// Returns a font of a specific size from the system.
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public static IBackendFont GetFont(float size)
        {
            // See if we already have the font
            if (fonts.Contains(size))
                return fonts[size];

            // Generate the font
            IBackendFont font = Game.Backend
                .LoadFont("Fonts/kinkee__.ttf", size, FontStyle.Bold);
			//.LoadFont(FontFamily.GenericSansSerif, size, FontStyle.Bold);

            // Cache it and return the results
            fonts[size] = font;
            return font;
        }
        #endregion

        #region Game Modes
        private static IGameMode gameMode;
		private static PlayMode playMode;

        /// <summary>
        /// Switches the game mode currently in use. This also triggers
        /// the needed events for starting up and shutting down a game.
        /// </summary>
        public static IGameMode GameMode
        {
            get { return gameMode; }
            set
            {
                // Check for nulls
                if (value == null)
                    throw new Exception("Cannot have a null game mode.");

                // If we already have a mode, deactivate it
                if (gameMode != null)
                    gameMode.Deactivate();

                // Set the value
                gameMode = value;

				// If we are a play mode, throw it in there
				if (gameMode is PlayMode)
					playMode = (PlayMode) gameMode;

                // Activate the new mode. There are some modes that
                // will switch the game mode as part of their activation
                // so we cannot do anything after this point.
                gameMode.Activate();
            }
        }

		/// <summary>
		/// Returns the mode as a PlayMode or null.
		/// </summary>
		public static PlayMode PlayMode
		{
			get { return playMode; }
			set { playMode = value; }
		}
        #endregion

        #region Game State
        private static State state;
		private static IniConfigSource config;

		/// <summary>
		/// Contains the CuteGod configuration section.
		/// </summary>
		public static IConfig Config
		{
			get { return config.Configs["CuteGod"]; }
		}

		/// <summary>
		/// Contains the configuration for the game.
		/// </summary>
		public static IniConfigSource ConfigSource
		{
			get { return config; }
			set { config = value; }
		}

        /// <summary>
        /// Contains the current game state, which may be null if there
        /// is no game in progress.
        /// </summary>
        public static State State
        {
            get { return state; }
            set { state = value; }
        }

        /// <summary>
        /// Indicates to the loaded game mode that it should
        /// perform any drawing routines.
        /// </summary>
        /// <param name="args"></param>
        public static void Draw()
        {
            // Set the backend
            DrawingArgs args = new DrawingArgs();
            args.Backend = backend;

            // Check for game mode
            if (gameMode != null)
                gameMode.Draw(args);
        }

        /// <summary>
        /// Triggers an update into the system, updating the state.
        /// In most cases, this passes it to the game mode.
        /// </summary>
        /// <param name="args"></param>
        public static void Update(UpdateArgs args)
        {
            // Check for game mode
            if (gameMode != null)
                gameMode.Update(args);

            // Update the sprites
            sprites.Update(args);

            // Update the GUI
            gui.Update(args);
        }
        #endregion

        #region Prayers
        private static PrayerFactory prayerFactory = new PrayerFactory();

        /// <summary>
        /// Returns the prayer factory for the game.
        /// </summary>
        public static PrayerFactory PrayerFactory
        {
            get { return prayerFactory; }
        }
        #endregion
    }
}
