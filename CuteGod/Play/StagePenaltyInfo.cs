using C5;
using MfGames.Utility;
using System;
using System.IO;
using System.Xml;

namespace CuteGod.Play
{
	/// <summary>
	/// Represents a stage penalty description which is automatically
	/// loaded by using the static members of this class.
	/// </summary>
	public class StagePenaltyInfo
	: Logable
	{
		#region Static Constructors
		private static HashDictionary<StagePenaltyType,StagePenaltyInfo> hash =
			new HashDictionary<StagePenaltyType,StagePenaltyInfo>();

		/// <summary>
		/// Loads the stage penalty descriptions from the given XML
		/// file and sets up the internal structures.
		/// </summary>
		static StagePenaltyInfo()
		{
			try
			{
				// Get the manifest string
				Stream stream = typeof(StagePenaltyInfo).Assembly
					.GetManifestResourceStream("CuteGod.penalties.xml");
				
				// Create an XML reader out of it
				XmlTextReader xml = new XmlTextReader(stream);
				StagePenaltyInfo sp = null;
				StagePenaltyType type = StagePenaltyType.Maximum;
				
				// Loop through the file
				while (xml.Read())
				{
					// See if we finished a node
					if (xml.NodeType == XmlNodeType.EndElement &&
						xml.LocalName == "penalty")
					{
						hash[type] = sp;
						sp = null;
					}
					
					// Ignore all but start elements
					if (xml.NodeType != XmlNodeType.Element)
						continue;
					
					// Check for name
					if (xml.LocalName == "penalty")
					{
						sp = new StagePenaltyInfo();
						type = (StagePenaltyType) Enum
							.Parse(typeof(StagePenaltyType), xml["type"]);
						sp.type = type;
					}
					else if (xml.LocalName == "title")
						sp.Title = xml.ReadString();
					else if (xml.LocalName == "block")
						sp.BlockKey = xml.ReadString();
					else if (xml.LocalName == "description")
						sp.Description = xml.ReadString();
				}
			}
			catch (Exception e)
			{
				Logger.Error(typeof(StagePenaltyInfo),
					"Cannot load penalties: {0}", e.Message);
				throw e;
			}
		}

		/// <summary>
		/// Picks out a random stage penalty that is applicable to the
		/// system and returns the object.
		/// </summary>
		public static StagePenaltyInfo Random
		{
			get
			{
				// Loop until we find a good valid one
				while (true)
				{
					// Get a random stage penalty and get its info
					int select =
						Entropy.Next(0, (int) StagePenaltyType.Maximum);
					StagePenaltyType type = (StagePenaltyType) select;

					// Return null if we are none
					if (type == StagePenaltyType.None)
						return null;

					// See if we can apply this
					StagePenaltyInfo info = hash[type];

					if (info.IsApplicable)
					{
						info.Apply();
						return info;
					}
				}
			}
		}
		#endregion

		#region Constructors
		private StagePenaltyInfo()
		{
		}
		#endregion

		#region Properties
		private string title;
		private string block;
		private string description;
		private StagePenaltyType type;

		/// <summary>
		/// The block key used to display the message.
		/// </summary>
		public string BlockKey
		{
			get { return block; }
			private set { block = value; }
		}

		/// <summary>
		/// Contains the description of this stage penalty.
		/// </summary>
		public string Description
		{
			get { return description; }
			private set
			{
				description = value
					.Replace("\t", " ")
					.Replace("\r", " ")
					.Replace("\n", " ")
					.Replace("  ", " ")
					.Replace("  ", " ")
					.Trim();
			}
		}

		/// <summary>
		/// This is the title of the stage penalty.
		/// </summary>
		public string Title
		{
			get { return title; }
			private set { title = value; }
		}
		#endregion

		#region Game State
		/// <summary>
		/// Returns true if this info object is applicable to the
		/// current game state.
		/// </summary>
		public bool IsApplicable
		{
			get
			{
				// Ignore empty states
				if (Game.State == null)
					return false;

				// Switch based on the type
				switch (type)
				{
				case StagePenaltyType.ReducedGrass:
					return Game.State.GrassCount > 0;
					
				case StagePenaltyType.ReducedDirt:
					return Game.State.DirtCount > 0;
					
				case StagePenaltyType.ReducedWater:
					return Game.State.WaterCount > 0;
					
				case StagePenaltyType.IncreasedImmobile:
					return Game.State.GrassCount
						+ Game.State.DirtCount
						+ Game.State.WaterCount
						+ Game.State.DeadGrassCount
						+ Game.State.DeadDirtCount
						+ Game.State.DeadWaterCount > 0;

				case StagePenaltyType.IncreasedGrabCost:
					return Game.State.GrabCost * 5 <= Game.State.StagesStarted;

				case StagePenaltyType.Countdown:
					return Game.State.SecondsPerTurn < 0 &&
						Game.State.StagesStarted >=
						Constants.MinimumStageForClock;

				case StagePenaltyType.LessTime:
					return Game.State.SecondsPerTurn
						> Constants.MinimumSecondsPerTurn;

				case StagePenaltyType.Bugs:
					return false; // Removed
					//return Game.State.StagesStarted >=
					//Constants.MinimumStageForBugs;

				default:
					throw new Exception("Cannot identify type: " + type);
				}
			}
		}

		/// <summary>
		/// Applies the object to the game state. This assumes it is
		/// applicable.
		/// </summary>
		public void Apply()
		{
            // Figure out common values
            int diff = Constants.MaximumBlocksChanged;
			
            // Switch based on the penalty
            switch (type)
            {
			case StagePenaltyType.ReducedGrass:
				// Switch grass to dead grass
				if (diff > Game.State.GrassCount)
					diff = Game.State.GrassCount;
				
				Game.State.GrassCount -= diff;
				Game.State.DeadGrassCount += diff;
				break;
			case StagePenaltyType.ReducedDirt:
				// Switch dirt to sand
				if (diff > Game.State.DirtCount)
					diff = Game.State.DirtCount;
				
				Game.State.DirtCount -= diff;
				Game.State.DeadDirtCount += diff;
				break;
			case StagePenaltyType.ReducedWater:
				// Switch water to brackish water
				if (diff > Game.State.WaterCount)
					diff = Game.State.WaterCount;
				
				Game.State.WaterCount -= diff;
				Game.State.DeadWaterCount += diff;
				break;
				
			case StagePenaltyType.IncreasedImmobile:
				// Remove a number of immobile blocks
				ApplyIncreaseImmobile(diff);
				break;
				
			case StagePenaltyType.IncreasedGrabCost:
				// Increase the grab cost by one
				Game.State.GrabCost++;
				break;

			case StagePenaltyType.Countdown:
				// Set the timer
				Game.State.SecondsPerTurn = Constants.StartingCountdown;
				Game.State.SecondsRemaining = Game.State.SecondsPerTurn;
				break;

			case StagePenaltyType.LessTime:
				// Reduce the counter
				Game.State.SecondsPerTurn -= Constants.LessTimeSeconds;
				Game.State.SecondsRemaining = Game.State.SecondsPerTurn;
				break;

			case StagePenaltyType.Bugs:
				// Add some bugs
				Game.State.Board.AddBugs(Game.State.BugRound);

				// Increase the number for next time
				Game.State.BugRound++;
				break;
            }
		}

		/// <summary>
		/// Applies the IncreasedImmobile stage penalty which is a bit
		/// more involved.
		/// </summary>
		private void ApplyIncreaseImmobile(int diff)
		{
			// Loop until we don't have any more blocks or we are done
			int cutoff = 100;
			int current = 0;

			while (diff > 0)
			{
				// Make sure we have at least one to take
				int total = Game.State.GrassCount + Game.State.DeadGrassCount
					+ Game.State.DirtCount + Game.State.DeadDirtCount
					+ Game.State.WaterCount + Game.State.DeadWaterCount;

				if (total == 0)
					return;

				// Knock down the cut off
				if (cutoff-- < 0)
				{
					Error("Immobile block cutoff: {0} from {1}",
						diff, total);
					return;
				}

				// Randomly pick one
				switch (current++ % 6)
				{
				case 3: // Grass
					if (Game.State.GrassCount > 0)
					{
						Game.State.GrassCount--;
						Game.State.ImmobileCount++;
						diff--;
					}
					break;
					
				case 4: // Dirt
					if (Game.State.DirtCount > 0)
					{
						Game.State.DirtCount--;
						Game.State.ImmobileCount++;
						diff--;
					}
					break;
					
				case 5: // Water
					if (Game.State.WaterCount > 0)
					{
						Game.State.WaterCount--;
						Game.State.ImmobileCount++;
						diff--;
					}
					break;
					
				case 0: // Dead Grass
					if (Game.State.DeadGrassCount > 0)
					{
						Game.State.DeadGrassCount--;
						Game.State.ImmobileCount++;
						diff--;
					}
					break;
					
				case 1: // Dead Dirt
					if (Game.State.DeadDirtCount > 0)
					{
						Game.State.DeadDirtCount--;
						Game.State.ImmobileCount++;
						diff--;
					}
					break;
					
				case 2: // Dead Water
					if (Game.State.DeadWaterCount > 0)
					{
						Game.State.DeadWaterCount--;
						Game.State.ImmobileCount++;
						diff--;
					}
					break;
				}
			}
		}
		#endregion
	}
}
