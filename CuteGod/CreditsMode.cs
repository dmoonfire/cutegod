using C5;
using BooGame.Input;
using MfGames.Sprite3;
using MfGames.Utility;
using System;
using System.Drawing;
using System.IO;
using System.Xml;

namespace CuteGod.Play
{
    /// <summary>
	/// Displays the credits for the game.
    /// </summary>
    public class CreditsMode
	: Logable, IGameMode
    {
        #region Constants
        private float FontSize = 100;
        private float TitleFontSize = 40;
        #endregion

        #region Constructors
        /// <summary>
        /// Constructs the minor mode and sets up the internal
        /// sprites.
        /// </summary>
        public CreditsMode()
        {
            // Create the titles
            text = new TextDrawableSprite(Game.GetFont(TitleFontSize),
				"Credits");
            text.Color = Color.White;
			text.Alignment = ContentAlignment.TopLeft;
            sprites.Add(text);

            category = new TextDrawableSprite(Game.GetFont(FontSize));
            category.Color = Color.White;
			category.Alignment = ContentAlignment.TopRight;
            sprites.Add(category);
        }
        #endregion

        #region Activation and Deactivation
        /// <summary>
        /// Indicates to the game mode that it is activated.
        /// </summary>
        public void Activate()
        {
			try
			{
				// Get the manifest string
				Stream stream = typeof(ChestBonusInfo).Assembly
					.GetManifestResourceStream("CuteGod.credits.xml");
				
				// Create an XML reader out of it
				XmlTextReader xml = new XmlTextReader(stream);
				LinkedList<CreditsLine> category = null;
				CreditsLine creator = null;
				
				// Loop through the file
				while (xml.Read())
				{
					// Ignore all but start elements
					if (xml.NodeType != XmlNodeType.Element)
						continue;
					
					// Check for name
					if (xml.LocalName == "category")
					{
						// Get the category from the given name
						string title = xml["title"];

						if (!credits.Contains(title))
							credits[title] = new LinkedList<CreditsLine>();

						category = credits[title];
					}
					else if (xml.LocalName == "creator")
					{
						// Seed the name
						string name = xml["name"];
						creator = new CreditsLine();
						creator.Name = name;
						category.Add(creator);
					}
					else if (xml.LocalName == "asset")
					{
						// Increment the creator's asset counter
						creator.Assets++;
					}
				}
			}
			catch (Exception e)
			{
				Error("Cannot load credits: {0}", e.Message);
				throw e;
			}
        }

        /// <summary>
        /// Triggered when the game mode is deactivated.
        /// </summary>
        public void Deactivate()
        {
        }
        #endregion

        #region Drawing and Activation
		private string currentCategory = null;
		private double newSeconds = 0.2;

		private void ChooseCategory()
		{
			// Pick the order for the standard ones
			if (credits.Contains("Programming"))
			{
				// Select that
				currentCategory = "Programming";
			}
			else
			{
				// Randomly picked the next one
				foreach (string key in credits.Keys)
				{
					currentCategory = key;
					break;
				}
			}

			// If we don't have a category, we are done
			if (currentCategory == null)
				return;

			// Set the text, add the pending items, and clear the category
			category.Text = currentCategory;
			pending.Clear();
			pending.AddAll(credits[currentCategory]);
			credits.Remove(currentCategory);
		}

        public void Draw(DrawingArgs args)
        {
            sprites.Draw(args);
        }

        public void Update(UpdateArgs args)
        {
			// See if we have a new category
			if (currentCategory == null)
			{
				// Create a category and force the first credit to show
				ChooseCategory();
				Debug("Starting category: {0}", currentCategory);
				newSeconds = 0;
			}

			// If we don't have a category, we are done
			if (currentCategory == null)
			{
				Game.GameMode = new MainMenuMode();
				return;
			}

			// Update any of the displayed ones
			LinkedList<CreditsLine> tmpList = new LinkedList<CreditsLine>();
			tmpList.AddAll(displayed);

			foreach (CreditsLine cl in tmpList)
			{
				// Add the time
				cl.SecondsDisplayed += args.SecondsSinceLastUpdate;

				// If we exceeded the life, kill it
				if (cl.SecondsDisplayed > cl.SecondsToLive)
				{
					displayed.Remove(cl);
					sprites.Remove(cl.Sprite);
					Debug("Removing credit line: {0}", cl.Name);
				}
			}

			// If the displayed and pending list are empty, then we
			// are done
			if (displayed.Count == 0 && pending.Count == 0)
			{
				Debug("Finished category: {0}", currentCategory);
				currentCategory = null;
				return;
			}

			// See if we are showing a new one
			newSeconds -= args.SecondsSinceLastUpdate;

			if (slots > 0 && newSeconds <= 0 && pending.Count > 0)
			{
				// Reset the counter
				newSeconds = 0.2;

				// See if we have too many
				if (displayed.Count <= slots)
				{
					// Pick a random location for this
					int kill = slots * 2;

					while (true)
					{
						// Check the kill
						if (kill-- < 0)
							break;

						// Set up some variables
						int row = Entropy.Next(0, slots);
						float y = row * CreditsLine.FontSize + FontSize * 2;
						bool found = false;

						foreach (CreditsLine cl0 in displayed)
						{
							if (cl0.Point.Y == y)
							{
								found = true;
								break;
							}
						}

						// If we found something, try again
						if (found)
							continue;

						// Add a new one
						CreditsLine cl = pending.RemoveFirst();
						displayed.Add(cl);
						cl.Point = new PointF(
							Entropy.NextFloat(sprites.Size.Width
								- cl.Sprite.Size.Width - 160) + 80,
							y);
						sprites.Add(cl.Sprite);
						break;
					}
				}				
			}

			// Update the sprites
            sprites.Update(args);
        }
        #endregion

		#region Controller
		/// <summary>
		/// Triggered when the user releases or presses a key.
		/// </summary>
		public void Key(KeyboardEventArgs args)
		{
			// Break out
			Game.GameMode = new MainMenuMode();
		}
		#endregion

        #region Properties
        private SpriteViewport sprites = new SpriteViewport();
        private TextDrawableSprite text, category;
		private int slots = 0;

		private HashDictionary<string, LinkedList<CreditsLine>> credits =
			new HashDictionary<string, LinkedList<CreditsLine>>();
		private LinkedList<CreditsLine> displayed =
			new LinkedList<CreditsLine>();
		private LinkedList<CreditsLine> pending =
			new LinkedList<CreditsLine>();

        /// <summary>
        /// Sets the size of the game mode (based on the screen).
        /// </summary>
        public SizeF Size
        {
            set
            {
                // Set the sprite
                sprites.Size = value;

                // Center the game over
                text.Point = new PointF(FontSize / 10, FontSize / 10);
                category.Point = new PointF(value.Width - FontSize / 10,
					FontSize / 10);

				// Figure out the credit slots
				float r = value.Height - FontSize * 3;
				slots = (int) (r / CreditsLine.FontSize);
            }
        }
        #endregion
    }
}
