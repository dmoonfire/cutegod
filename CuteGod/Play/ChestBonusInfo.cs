using C5;
using MfGames.Utility;
using System;
using System.Drawing;
using System.IO;
using System.Xml;

namespace CuteGod.Play
{
	/// <summary>
	/// Represents a chest bonus description which is automatically
	/// loaded by using the static members of this class.
	/// </summary>
	public class ChestBonusInfo
	: Logable
	{
		#region Static Constructors
		private static HashDictionary<ChestBonusType,ChestBonusInfo> hash =
			new HashDictionary<ChestBonusType,ChestBonusInfo>();

		/// <summary>
		/// Loads the chest bonus descriptions from the given XML
		/// file and sets up the internal structures.
		/// </summary>
		static ChestBonusInfo()
		{
			try
			{
				// Get the manifest string
				Stream stream = typeof(ChestBonusInfo).Assembly
					.GetManifestResourceStream("CuteGod.bonuses.xml");
				
				// Create an XML reader out of it
				XmlTextReader xml = new XmlTextReader(stream);
				ChestBonusInfo sp = null;
				ChestBonusType type = ChestBonusType.Maximum;
				
				// Loop through the file
				while (xml.Read())
				{
					// See if we finished a node
					if (xml.NodeType == XmlNodeType.EndElement &&
						xml.LocalName == "bonus")
					{
						hash[type] = sp;
						sp = null;
					}
					
					// Ignore all but start elements
					if (xml.NodeType != XmlNodeType.Element)
						continue;
					
					// Check for name
					if (xml.LocalName == "bonus")
					{
						sp = new ChestBonusInfo();
						type = (ChestBonusType) Enum
							.Parse(typeof(ChestBonusType), xml["type"]);
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
				Logger.Error(typeof(ChestBonusInfo),
					"Cannot load penalties: {0}", e.Message);
				throw e;
			}
		}

		/// <summary>
		/// Picks out a random chest bonus that is applicable to the
		/// system and returns the object.
		/// </summary>
		public static ChestBonusInfo Random
		{
			get
			{
				// Loop until we find a good valid one
				while (true)
				{
					// Get a random chest bonus and get its info
					int select =
						Entropy.Next(0, (int) ChestBonusType.Maximum);
					ChestBonusType type = (ChestBonusType) select;
					ChestBonusInfo info = hash[type];

					// See if we can apply this
					if (info.IsApplicable)
					{
						info.Apply1();
						return info;
					}
				}
			}
		}
		#endregion

		#region Constructors
		private ChestBonusInfo()
		{
		}
		#endregion

		#region Properties
		private string title;
		private string block;
		private string description;
		private ChestBonusType type;

		/// <summary>
		/// The block key used to display the message.
		/// </summary>
		public string BlockKey
		{
			get { return block; }
			private set { block = value; }
		}

		/// <summary>
		/// Contains the description of this chest bonus.
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
		/// This is the title of the chest bonus.
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
				case ChestBonusType.ExtraHearts:
					// We can always get extra hearts
					return true;

				case ChestBonusType.GrabStack:
					// Only if we don't have a max and we don't get it
					// more than one every 5 stages.
					return Game.State.GrabCount < Constants.MaximumGrabCount
						&& Game.State.StagesStarted
						> Game.State.GrabCount * 5;

				case ChestBonusType.RemoveDeadGrass:
					return Game.State.DeadGrassCount > 0 ||
						Game.State.ImmobileCount > 0;

				case ChestBonusType.RemoveDeadDirt:
					return Game.State.DeadDirtCount > 0 ||
						Game.State.ImmobileCount > 0;

				case ChestBonusType.RemoveDeadWater:
					return Game.State.DeadWaterCount > 0 ||
						Game.State.ImmobileCount > 0;

				case ChestBonusType.RestoreGrass:
					return Game.State.Board.HasTopBlock("Dead Grass Block");

				case ChestBonusType.RestoreDirt:
					return Game.State.Board.HasTopBlock("Dead Dirt Block");

				case ChestBonusType.RestoreWater:
					return Game.State.Board.HasTopBlock("Dead Water Block");

				case ChestBonusType.NoClock:
					// For removing the clock if the timeout is higher
					// than the intial time
					return Game.State.SecondsPerTurn > 0 &&
						Game.State.SecondsPerTurn * 2
						> Constants.StartingCountdown;

				case ChestBonusType.DoubleTime:
					// For doubling the time given
					return Game.State.SecondsPerTurn > 0 &&
						Game.State.SecondsPerTurn * 2
						<= Constants.StartingCountdown;

				case ChestBonusType.GrabCost:
					// Halfs the grab cost
					return Game.State.GrabCost > 1;

				default:
					return false;
				}
			}
		}

		/// <summary>
		/// Applies any immediate changes to the system.
		/// </summary>
		public void Apply1()
		{
			// Common counters
			State state = Game.State;
			int diff = Constants.MaximumBlocksChanged;

			// We have two applies because the state ones need to
			// change before the text is given.
            // Switch based on the bonus
            switch (type)
            {
			case ChestBonusType.GrabStack:
				// Increment the grab stack by one
				state.GrabCount++;
				break;

			case ChestBonusType.NoClock:
				// Removes the clock
				state.SecondsPerTurn = -1;
				state.SecondsRemaining = 0;
				break;

			case ChestBonusType.DoubleTime:
				// Double the time
				state.SecondsPerTurn *= 2;
				state.SecondsRemaining = Game.State.SecondsPerTurn;
				break;

			case ChestBonusType.GrabCost:
				// Halves the grab cost
				state.GrabCost /= 2;
				break;

			case ChestBonusType.RestoreWater:
				// Remove the top dead blocks
				state.Board.ReplaceTopDrawables(
					"Dead Water Block",
					"Water Block");
				break;

			case ChestBonusType.RestoreDirt:
				// Remove the top dead blocks
				state.Board.ReplaceTopDrawables(
					"Dead Dirt Block",
					"Dirt Block");
				break;

			case ChestBonusType.RestoreGrass:
				// Remove the top dead blocks
				state.Board.ReplaceTopDrawables(
					"Dead Grass Block",
					"Grass Block");
				break;

			case ChestBonusType.RemoveDeadGrass:
				// Restore some dead grass
				if (state.DeadGrassCount >= diff)
				{
					state.GrassCount += diff;
					state.DeadGrassCount -= diff;
				}
				else
				{
					state.GrassCount += diff;
					diff -= state.DeadGrassCount;
					state.ImmobileCount -= diff;

					if (state.ImmobileCount < 0)
					{
						state.GrassCount += state.ImmobileCount;
						state.ImmobileCount = 0;
					}

					state.DeadGrassCount = 0;
				}
				break;

			case ChestBonusType.RemoveDeadDirt:
				// Restore some dead dirt
				if (state.DeadDirtCount >= diff)
				{
					state.DirtCount += diff;
					state.DeadDirtCount -= diff;
				}
				else
				{
					state.DirtCount += diff;
					diff -= state.DeadDirtCount;
					state.ImmobileCount -= diff;

					if (state.ImmobileCount < 0)
					{
						state.DirtCount += state.ImmobileCount;
						state.ImmobileCount = 0;
					}

					state.DeadDirtCount = 0;
				}
				break;

			case ChestBonusType.RemoveDeadWater:
				// Restore some dead water
				if (state.DeadWaterCount >= diff)
				{
					state.WaterCount += diff;
					state.DeadWaterCount -= diff;
				}
				else
				{
					state.WaterCount += diff;
					diff -= state.DeadWaterCount;
					state.ImmobileCount -= diff;

					if (state.ImmobileCount < 0)
					{
						state.WaterCount += state.ImmobileCount;
						state.ImmobileCount = 0;
					}

					state.DeadWaterCount = 0;
				}
				break;
			}
		}

		/// <summary>
		/// Applies the bonus 
		/// </summary>
		public void Apply(PointF point)
		{
            // Figure out common values
			PointF p = new PointF(
				point.X + 31,
				point.Y + 90);
			
            // Switch based on the bonus
            switch (type)
            {
			case ChestBonusType.ExtraHearts:
				// Add a bunch of hearts based on the number of columns
				Game.PlayMode
					.SwarmHearts(
						(int) (Game.State.Board.Columns //State.StagesStarted
							* Constants.ExtraHeartsStageMultiplier),
						p);
				break;
            }
		}
		#endregion
	}
}
