using C5;
using BooGame;
using BooGame.Input;
using CuteGod.Play;
using MfGames.Sprite3;
using MfGames.Sprite3.Gui;
using MfGames.Sprite3.PlanetCute;
using MfGames.Utility;
using System;
using System.Drawing;

namespace CuteGod
{
    /// <summary>
	/// Implements the main menu for the entire game.
    /// </summary>
    public class MainMenuMode
	: Logable, IGameMode
    {
        #region Constants
        private float FontSize = 200;
        private float SmallFontSize = 40;
		private int BoardRows = 5;
		private int BoardColumns = 20;
        #endregion

        #region Activation and Deactivation
        /// <summary>
        /// Sets the size of the game mode (based on the screen).
        /// </summary>
        public SizeF Size
		{
			set
			{
				// Don't bother if we already are set
				if (sprites.Size == value)
					return;

				// Set the size
				sprites.Size = value;
				environment.Size = value;

				// Center the viewport on the bottom
				viewport.Size = value;
				viewport.Point = new PointF(0,
					value.Height - viewport.Extents.Height
					- (float) viewport.BlockHeight * 1.2f);

				// Center the title
				titleStart = new PointF(value.Width / 2, 0);
				titleEnd = new PointF(value.Width / 2, 20);

				// Figure out the points
				float x1 = 60;
				float x2 = value.Width - 60 - 340;
				float xm = (value.Width - 340) / 2;
				float y1 = 300;
				float y2 = y1 + 90;
				float y3 = y2 + 90;
				float y4 = y3 + 90;

				// Set up the points
				newGameStart = new PointF(xm, y1);
				newGameEnd = new PointF(x1, y1);
				settingsStart = new PointF(xm, y1);
				settingsEnd = new PointF(x2, y1);
				resumeStart = new PointF(xm, y2);
				resumeEnd = new PointF(x1, y2);
				helpStart = new PointF(xm, y2);
				helpEnd = new PointF(x2, y2);
				scoresStart = new PointF(xm, y3);
				scoresEnd = new PointF(x1, y3);
				creditsStart = new PointF(xm, y3);
				creditsEnd = new PointF(x2, y3);
				quitStart = new PointF(xm, y4);
				quitEnd = new PointF(x1, y4);

				// Add the version point
				version.Point = new PointF(
					value.Width - SmallFontSize / 2,
					value.Height - SmallFontSize / 2);
			}
		}

        /// <summary>
        /// Indicates to the game mode that it is activated.
        /// </summary>
        public void Activate()
		{
			// Set up the sprites
			sprites.IsMouseSensitive = true;
			Game.SpriteManager.Add(sprites);

			// Create the big text
			title = new TextDrawableSprite(Game.GetFont(FontSize),
				"Cute God");
			title.Alignment = ContentAlignment.TopCenter;
			title.Tint = Color.White;
			sprites.Add(title);

			System.Version v = GetType().Assembly.GetName().Version;
			version = new TextDrawableSprite(Game.GetFont(SmallFontSize),
				String.Format("{0}.{1}.{2}",
					v.Major, v.Minor, v.Build));
			version.Alignment = ContentAlignment.BottomRight;
			version.Tint = Color.FromArgb(64, Color.White);
			sprites.Add(version);

			// Create the buttons
			newGame = new MainMenuButton(AssetLoader.Instance
				.CreateDrawable("new-game"));
			newGame.Clicked += NewGameClicked;
			sprites.Add(newGame);

			settings = new MainMenuButton(AssetLoader.Instance
				.CreateDrawable("settings"));
			sprites.Add(settings);

			resume = new MainMenuButton(AssetLoader.Instance
				.CreateDrawable("resume-game"));
			
			if (Game.PlayMode != null)
				resume.Clicked += ResumeClicked;

			sprites.Add(resume);

			help = new MainMenuButton(AssetLoader.Instance
				.CreateDrawable("help"));
			sprites.Add(help);

			scores = new MainMenuButton(AssetLoader.Instance
				.CreateDrawable("high-scores"));
			sprites.Add(scores);

			credits = new MainMenuButton(AssetLoader.Instance
				.CreateDrawable("credits"));
			credits.Clicked += CreditsClicked;
			sprites.Add(credits);

			quit = new MainMenuButton(AssetLoader.Instance
				.CreateDrawable("quit"));
			quit.Clicked += QuitClicked;
			sprites.Add(quit);

			// Create the block viewport
			board = new BlockBoard(BoardRows, BoardColumns);
			board.BlockMovingChanged += OnBlockMovingChanged;

			viewport = new BlockViewport(board, AssetLoader.Instance);
			viewport.FocusPosition = Constants.FocusPosition;
			PopulateBoard();
			// We don't add to the sprites since we are manually
			// drawing it to add a black screen in front of it.
		}
        
        /// <summary>
        /// Triggered when the game mode is deactivated.
        /// </summary>
        public void Deactivate()
		{
			// Remove our sprites
			Game.SpriteManager.Remove(sprites);
		}
        #endregion

		#region Blocks
		private BlockBoard board;
		private BlockViewport viewport;
		private LinkedList<Block> blocks = new LinkedList<Block>();
		private HashDictionary<Block,BlockStack> stacks =
			new HashDictionary<Block,BlockStack>();
		private double secondsUntilChange = -1;

		/// <summary>
		/// Random grabs or drops a block on the board.
		/// </summary>
		private void ChangeBoard(UpdateArgs args)
		{
			// Loop through the blocks to see if they are too high. If
			// they are, remove them.
			LinkedList<Block> tmpBlocks = new LinkedList<Block>();
			tmpBlocks.AddAll(blocks);

			foreach (Block b in tmpBlocks)
			{
				if (b.BottomPosition > 10)
				{
					stacks[b].Remove(b);
					blocks.Remove(b);
					stacks.Remove(b);
				}
			}

			// Decrement the counter for the timeout
			secondsUntilChange -= args.SecondsSinceLastUpdate;

			if (secondsUntilChange > 0)
				// We aren't changing anything
				return;

			// Reset it
			secondsUntilChange = Entropy.NextDouble() * 2;

			// Pick a random coordinate
			int x = Entropy.Next(0, BoardColumns);
			int y = Entropy.Next(0, BoardRows);
			BlockStack stack = board[x, y];
			
			// Make sure we aren't already doing something here
			foreach (Block b in blocks)
				if (stacks[b] == stack)
					// Don't bother this time
					return;

			// We have a stack, decide if we are going to drop or grab
			// something from the stack.
			bool drop = Entropy.Next(0, 2) == 0;

			if (stack.Count > 5)
				// Don't go over 5 high
				drop = false;

			if (stack.Count == 1)
				// Don't go below 1 high
				drop = true;

			// Figure out what to do
			if (drop)
			{
				// Create a new block
				Block nb = new Block(RandomBlock());
				nb.BottomPosition = 10;
				nb.Vector = Constants.DroppedVector;
				nb.Mass = Constants.BlockMass;
				nb.IsMoving = true;
				nb.CastsShadows = true;
				nb.Height = 1;
				stack.Add(nb);
			}
			else
			{
				// Grab the top block
				Block tb = stack.TopBlock;
				tb.Vector = -Constants.DroppedVector;
				tb.Mass = 0;
				tb.IsMoving = true;
			}
		}

		/// <summary>
		/// This is called when a block stops moving, we use that to
		/// determine when to pull out the dropped blocks.
		/// </summary>
		private void OnBlockMovingChanged(
			object sender, BlockStackEventArgs args)
		{
			// Remove the block from our internal lists
			stacks.Remove(args.Block);
			blocks.Remove(args.Block);
		}

		/// <summary>
		/// Internal function to populate the board with random
		/// blocks.
		/// </summary>
		private void PopulateBoard()
		{
			for (int x = 0; x < BoardColumns; x++)
			{
				for (int y = 0; y < BoardRows; y++)
				{
					// Get the stack
					BlockStack stack = board[x, y];

					// Add 1-4 blocks
					int total = Entropy.Next(4) + 1;

					for (int i = 0; i < total; i++)
					{
						Block block = new Block(RandomBlock());
						block.BottomPosition = i;
						block.CastsShadows = true;
						block.Height = 1;
						block.Mass = Constants.BlockMass;
						stack.Add(block);
					}
				}
			}
		}

		/// <summary>
		/// Get a random block name.
		/// </summary>
		private ISprite RandomBlock()
		{
			// Figure out the block type
			string blockKey = null;

			switch (Entropy.Next(3))
			{
				case 0: blockKey = "Grass Block"; break;
				case 1: blockKey = "Dirt Block"; break;
				case 2: blockKey = "Water Block"; break;
				default:
					throw new Exception("Cannot figure out random");
			}

			// Return the block
			return AssetLoader.Instance.CreateSprite(blockKey);
		}
		#endregion

        #region Drawing
		private Environment environment = new Environment();
		private SpriteViewport sprites = new SpriteViewport();
		private int alpha = 0;

		private TextDrawableSprite title, version;
		private PointF titleStart, titleEnd;

		private MainMenuButton newGame, settings, resume;
		private MainMenuButton help, scores, credits, quit;

		private PointF newGameStart, newGameEnd;
		private PointF settingsStart, settingsEnd;
		private PointF resumeStart, resumeEnd;
		private PointF helpStart, helpEnd;
		private PointF scoresStart, scoresEnd;
		private PointF creditsStart, creditsEnd;
		private PointF quitStart, quitEnd;

        /// <summary>
        /// Triggers a rendering of the game mode.
        /// </summary>
        /// <param name="args"></param>
        public void Draw(DrawingArgs args)
		{
			// Draw out the viewport
			environment.DrawBackground(args);
			viewport.Draw(args);
			environment.DrawForeground(args);

			// Add a dark screen to obscure it slightly
			args.Backend.DrawFilledRectangle(
				new RectangleF(sprites.Point, sprites.Size),
				Color.FromArgb(192, Color.Black));

			// Draw out the sprites
			sprites.Draw(args);
		}
        #endregion

        #region Updating
		private double secondsFadeIn = Constants.MainMenuFadeIn;

        /// <summary>
        /// Triggers an update of the mode's state.
        /// </summary>
        /// <param name="args"></param>
        public void Update(UpdateArgs args)
		{
			// Update the environment
			environment.Update(args);

			// If we are fading in, change the position
			if (secondsFadeIn > 0)
			{
				// Decrement the counter
				secondsFadeIn -= args.SecondsSinceLastUpdate;

				if (secondsFadeIn < 0)
					secondsFadeIn = 0;

				// Figure out the ratios to use
				float ratio = (float)
					(secondsFadeIn / Constants.MainMenuFadeIn);
				float inverse = 1 - ratio;
				alpha = (int) (inverse * 255);

				// Figure out the title position and color
				title.Point = new PointF(
					(ratio * titleStart.X + inverse * titleEnd.X),
					(ratio * titleStart.Y + inverse * titleEnd.Y));
				title.Tint = Color.FromArgb(alpha, Color.White);
				version.Tint = Color.FromArgb(alpha / 4, Color.White);

				// New Game
				newGame.Point = new PointF(
					(ratio * newGameStart.X + inverse * newGameEnd.X),
					(ratio * newGameStart.Y + inverse * newGameEnd.Y));
				newGame.Alpha = alpha;

				// Settings
				settings.Point = new PointF(
					(ratio * settingsStart.X + inverse * settingsEnd.X),
					(ratio * settingsStart.Y + inverse * settingsEnd.Y));
				settings.Alpha = alpha;

				// Resume
				resume.Point = new PointF(
					(ratio * resumeStart.X + inverse * resumeEnd.X),
					(ratio * resumeStart.Y + inverse * resumeEnd.Y));
				resume.Alpha = alpha;

				// Help
				help.Point = new PointF(
					(ratio * helpStart.X + inverse * helpEnd.X),
					(ratio * helpStart.Y + inverse * helpEnd.Y));
				help.Alpha = alpha;

				// High Scores
				scores.Point = new PointF(
					(ratio * scoresStart.X + inverse * scoresEnd.X),
					(ratio * scoresStart.Y + inverse * scoresEnd.Y));
				scores.Alpha = alpha;

				// Credits
				credits.Point = new PointF(
					(ratio * creditsStart.X + inverse * creditsEnd.X),
					(ratio * creditsStart.Y + inverse * creditsEnd.Y));
				credits.Alpha = alpha;

				// Quit
				quit.Point = new PointF(
					(ratio * quitStart.X + inverse * quitEnd.X),
					(ratio * quitStart.Y + inverse * quitEnd.Y));
				quit.Alpha = alpha;
			}
			else
			{
				// Set everything into the proper place
				title.Point = titleEnd;
				title.Tint = Color.White;
				version.Tint = Color.FromArgb(64, Color.White);

				newGame.Point = newGameEnd;
				newGame.Alpha = 255;

				settings.Point = settingsEnd;
				settings.Alpha = 255;

				resume.Point = resumeEnd;
				resume.Alpha = 255;

				help.Point = helpEnd;
				help.Alpha = 255;

				scores.Point = scoresEnd;
				scores.Alpha = 255;

				credits.Point = creditsEnd;
				credits.Alpha = 255;

				quit.Point = quitEnd;
				quit.Alpha = 255;
			}

			// Change the board around
			ChangeBoard(args);

			// Update the sprites and the viewport
			board.Update(args);
			sprites.Update(args);
			viewport.Update(args);
		}
        #endregion

		#region Controller
		/// <summary>
		/// Triggered when the user releases or presses a key.
		/// </summary>
		public void Key(KeyboardEventArgs args)
		{
			// We only care about keyboard presses
			if (!args.Pressed)
				return;

			// Switch the mode
			switch (args.Key)
			{
			case BooGameKeys.Escape:
				// Quit the game
				QuitClicked(this, new EventArgs());
				break;
				
#if DEBUG
			case BooGameKeys.F9:
				// Restart the mode
				Game.GameMode = new MainMenuMode();
				break;
#endif
			}
		}
		#endregion

		#region Events
		private void NewGameClicked(object sender, EventArgs args)
		{
			Game.GameMode = new NewGameMode();
		}

		private void ResumeClicked(object sender, EventArgs args)
		{
			Game.GameMode = Game.PlayMode;
		}

		private void CreditsClicked(object sender, EventArgs args)
		{
			Game.GameMode = new CreditsMode();
		}

		private void QuitClicked(object sender, EventArgs args)
		{
			// Kill our system
			Game.Sound.Stop();
			Game.ConfigSource.Save(Program.ConfigFile);
			Core.Exit();
			
			// Force the quit
			Error("Quitting CuteGod - Bibi!");
			System.Environment.Exit(0);
		}
		#endregion
    }
}
