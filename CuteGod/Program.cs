using BooGame;
using BooGame.Input;
using BooGame.Video;
using CuteGod.Play;
using MfGames.Sprite3;
using MfGames.Sprite3.Backends;
using MfGames.Sprite3.BooGameBE;
using MfGames.Sprite3.Gui;
using MfGames.Utility;
using Nini.Config;
using System;
using System.Drawing;
using System.IO;
using Tao.OpenGl;

namespace CuteGod
{
    /// <summary>
    /// Primary entry into the program. This sets up the graphics
    /// and sounds, then initiates the game using the appropriate game mode.
    /// </summary>
    public class Program
        : BooGame.Game
    {
        #region Constructors and Entry
        /// <summary>
        /// Entry into the application.
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            try
            {
                // Load the configuration
                IniConfigSource config = new IniConfigSource();

                if (File.Exists(ConfigFile))
                {
                    config = new IniConfigSource(ConfigFile);
                }
                else if (!config.Configs.Contains(""))
                {
                    config.AddConfig("CuteGod");
                }

                Game.ConfigSource = config;

                // Set up the window size
                int wWidth = Game.Config
                    .GetInt(Constants.ConfigWindowedWidth, 1024);
                int wHeight = Game.Config
                    .GetInt(Constants.ConfigWindowedHeight, 768);

                Core.WindowSize = new Size(wWidth, wHeight);
                Core.ScreenSize = Core.WindowSize;
                Core.Backend = new Graphics().Backend;
                Core.ClearColor = new ColorF(Color.Black);
                Core.Title = "Moonfire Games' CuteGod";
                FullScreen = Game.Config
                    .GetBoolean(Constants.ConfigFullScreen, false);

                // Enable the mouse events
                Mouse.Enabled = true;

                // Start the application
                Core.Run(new Program());
            }
            catch (Exception e)
            {
                // Fallback error
                System.Windows.Forms.MessageBox.Show("There was an error: "
                    + e.Message);
				throw e;
            }
        }

        /// <summary>
        /// Creates the basic application.
        /// </summary>
        public Program()
        {
            // Set up the game and backend
			Game.Backend = new BooGameBackend();

			// Load the initial assets into memory
			AssetLoader.Instance.Queue();

            // Initialize the sound system
            Game.Sound.Initialize();

            // Start up the initial game state
            Game.GameMode = new SplashMode(
				"Assets/mfgames-splash.png",
				5.0,
				new AssetLoadingMode());
        }
        #endregion

        #region Drawing
		/// <summary>
		/// Changes the full screen mode for the game.
		/// </summary>
		public static bool FullScreen
		{
			get { return Core.FullScreen; }
			set
			{
				// Use this to restore the window size when full screen
				// is turned off.
				if (!value)
				{
					// Get the sizes
					int wWidth = Game.Config
						.GetInt(Constants.ConfigWindowedWidth, 1024);
					int wHeight = Game.Config
						.GetInt(Constants.ConfigWindowedHeight, 768);
					
					// Set up the fields
					Logger.Info(typeof(Program),
						"Switching to windowed: {0}", Core.WindowSize);
					Core.WindowSize = new Size(wWidth, wHeight);
					Core.FullScreen = false;
					
					// Save the settings
					Game.Config.Set(Constants.ConfigFullScreen, false);
				}
				else
				{
					// Get the sizes
					int fWidth = Game.Config
						.GetInt(Constants.ConfigFullScreenWidth, 1024);
					int fHeight = Game.Config
						.GetInt(Constants.ConfigFullScreenHeight, 768);
					
					// Set up the fields
					Logger.Info(typeof(Program),
						"Switching to full screen: {0}", Core.WindowSize);
					Core.WindowSize = new Size(fWidth, fHeight);
					Core.FullScreen = true;
					
					// Save the settings
					Game.Config.Set(Constants.ConfigFullScreen, true);
				}
			}
		}

        /// <summary>
        /// Overrides the draw function to process the screen updating
        /// events.
        /// </summary>
        public override void Draw()
        {
            // Make sure the window size and screen match
            if (Core.WindowSize != Core.ScreenSize)
                Core.ScreenSize = Core.WindowSize;

			// Use this size for the layout
			Game.Size = Core.ScreenSize;

            // Set the mouse events
            Game.GuiManager
                .SetMousePosition(new PointF(Mouse.X, Mouse.Y));

            // Draw the game graphics
            Game.Draw();
        }
        #endregion

        #region Updating
        /// <summary>
        /// Overrides the update event to send events into the game
        /// state.
        /// </summary>
        /// <param name="args"></param>
        public override void Update(UpdateEventArgs args)
        {
            UpdateArgs uArgs = new UpdateArgs();
            uArgs.TicksSinceLastUpdate = args.ElapsedMilliseconds * 10000;
            Game.Update(uArgs);
        }
        #endregion

        #region Input Events
        /// <summary>
        /// Processes key down events, used for triggering things within
        /// the game.
        /// </summary>
        /// <param name="args"></param>
        public override void KeyDown(KeyboardEventArgs args)
        {
			// See if we have one of the universal commands
            switch (args.Key)
            {
                case BooGameKeys.F:
					FullScreen = !FullScreen;
					break;

				default:
					// Pass it on to the game mode
					if (Game.GameMode != null)
						Game.GameMode.Key(args);
					break;
            }
        }

        public override void MouseButtonDown(MouseEventArgs args)
        {
            // Call our parents
            base.MouseButtonDown(args);

            // Pass the event to the gui
            GuiMouseEventArgs gargs = new GuiMouseEventArgs();

            // Translate the mouse buttons
            if (args.Button == BooGame.Input.MouseButtons.Left ||
                args.Button == BooGame.Input.MouseButtons.None)
                gargs.Button = MfGames.Sprite3.Gui.MouseButtons.Left;
            if (args.Button == BooGame.Input.MouseButtons.Right)
                gargs.Button = MfGames.Sprite3.Gui.MouseButtons.Right;

            // Fire the event
            Game.GuiManager.MouseDown(gargs);
        }

        public override void MouseButtonUp(MouseEventArgs args)
        {
            // Call our parent
            base.MouseButtonUp(args);

            // Process our events
            GuiMouseEventArgs gargs = new GuiMouseEventArgs();

            // Translate the mouse buttons
            if (args.Button == BooGame.Input.MouseButtons.Left ||
                args.Button == BooGame.Input.MouseButtons.None)
                gargs.Button = MfGames.Sprite3.Gui.MouseButtons.Left;
            if (args.Button == BooGame.Input.MouseButtons.Right)
                gargs.Button = MfGames.Sprite3.Gui.MouseButtons.Right;

            // Fire the event
            Game.GuiManager.MouseUp(gargs);
        }
        #endregion

		#region Configuration
		/// <summary>
		/// Contains the full path to the configuration file.
		/// </summary>
		public static string ConfigFile
		{
			get
			{
				// Set up the configuration
				if (ConfigStorage.Singleton == null)
					ConfigStorage.Singleton = new ConfigStorage("MfGames");

				// Get the directory
				DirectoryInfo cDir =
					ConfigStorage.Singleton.GetDirectory("CuteGod");
				return cDir.FullName
					+ Path.DirectorySeparatorChar
					+ "CuteGod.ini";
			}
		}
		#endregion
    }
}
