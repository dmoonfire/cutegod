using C5;
using MfGames.Sprite3.PlanetCute;
using MfGames.Utility;
using System;

namespace CuteGod.Play
{
    /// <summary>
    /// Extends the standard BlockBard and adds game-specific elements
    /// to the board, such as stage creation.
    /// </summary>
    public class Board
        : BlockBoard
    {
        #region Constructors
        /// <summary>
        /// Create a new board with the appropriate number of rows but
        /// no columns.
        /// </summary>
        public Board()
            : base(5, 0)
        {
            // Set our limits for rendering
            MaximumPosition = Constants.MaximumPosition;
            MinimumPosition = 0;
        }
        #endregion

        #region Stage Generation
        /// <summary>
        /// Performs the placement of the three standard blocks on the
        /// board by grabbing random amounts of blocks of the three types
        /// and placing them to result in some kind of scattered board.
        /// </summary>
        private void AddBlocks()
        {
            // Yes, this is over-engineered.

            // Keep track of our counters, since we'll be decrementing them
            int water = Game.State.WaterCount;
            int dirt = Game.State.DirtCount;
            int grass = Game.State.GrassCount;
            int deadWater = Game.State.DeadWaterCount;
            int deadDirt = Game.State.DeadDirtCount;
            int deadGrass = Game.State.DeadGrassCount;

            // Loop through as long as we have something
            while (water + dirt + grass + deadWater + deadDirt + deadWater > 0)
            {
                // Place dirt tiles
                int place = Entropy.Next(0, Constants.MaximumBlocksPlaced);

                if (place > dirt)
                    place = dirt;

                AddBlocks(place, Constants.DirtBlockName);
                dirt -= place;

                // Place the water tiles
                place = Entropy.Next(0, Constants.MaximumBlocksPlaced);

                if (place > water)
                    place = water;

                AddBlocks(place, Constants.WaterBlockName);
                water -= place;

                // Place the grass
                place = Entropy.Next(0, Constants.MaximumBlocksPlaced);

                if (place > grass)
                    place = grass;

                AddBlocks(place, Constants.GrassBlockName);
                grass -= place;

                // Place dead dirt tiles
                place = Entropy.Next(0, Constants.MaximumBlocksPlaced);

                if (place > deadDirt)
                    place = deadDirt;

                AddBlocks(place, Constants.DeadDirtBlockName);
                deadDirt -= place;

                // Place the dead water tiles
                place = Entropy.Next(0, Constants.MaximumBlocksPlaced);

                if (place > deadWater)
                    place = deadWater;

                AddBlocks(place, Constants.DeadWaterBlockName);
                deadWater -= place;

                // Place the grass
                place = Entropy.Next(0, Constants.MaximumBlocksPlaced);

                if (place > deadGrass)
                    place = deadGrass;

                AddBlocks(place, Constants.DeadGrassBlockName);
                deadGrass -= place;
            }
        }

        /// <summary>
        /// Adds a number of blocks of the give name to the board. This
        /// only places blocks in the last Constants.StateWidth rows.
        /// </summary>
        /// <param name="count"></param>
        /// <param name="name"></param>
        private void AddBlocks(int count, string name)
        {
            // Loop through the blocks
            for (int i = 0; i < count; i++)
            {
                // Create a new block
                Block block = CreateBlock(name);

                // Since we have a maximum count here, we loop until we
                // find a valid stack to drop it on
                while (true)
                {
                    // Figure out a random position
                    int col = Entropy.Next(0, Constants.StageWidth)
                        + Columns - Constants.StageWidth;
                    int row = Entropy.Next(0, Constants.BoardRows);
                    BlockStack stack = this[col, row];

                    // Make sure the stack doesn't have too many items
                    if (stack.Count >= Constants.MaximumPlacementCount)
                        continue;

                    // Set the position based on the count
                    block.BottomPosition =
                        2f + (float) stack.Count * Constants.PlacementSpacing;
                    block.Vector = Constants.DroppedVector;
                    block.IsMoving = true;
                    stack.Add(block);

                    // Break out of the loop
                    break;
                }
            }
        }

        /// <summary>
        /// This creates a new stage for the game, using the
        /// properties of the state to determine how the board is
        /// generated.
        /// </summary>
        public void GenerateStage()
        {
            // See if we already have a gate column (i.e. if we have
            // any columns, then we do).
            int newColumns = Constants.StageWidth;

            // Extend the board by the number of columns
            int oldColumns = Columns;
            Columns = Columns + newColumns;

            // Populate the base level board
            for (int i = 0; i < newColumns; i++)
            {
                // Get the column
                IList<BlockStack> stacks = this[oldColumns + i];

                // Go through the rows
                for (int j = 0; j < Constants.BoardRows; j++)
                {
                    // Get the stack
                    BlockStack stack = stacks[j];
                    stack.IsInInitialPlacement = true;

                    // Set a new immobile block into place
                    Block block = new Block(AssetLoader.Instance
						.CreateSprite(Constants.ImmobileBlockName));
                    block.BottomPosition = 0;
                    block.Height = 1;

                    // Add it to the stack
                    stack.Add(block);
                }
            }

            // Populate the additional immobile boards
            AddBlocks(Game.State.ImmobileCount, Constants.ImmobileBlockName);

            // Add the grass, dirt, and water blocks
            AddBlocks();

            // Add the character prayer, but not on the far-right side
            Prayer prayer = new Prayer();
            int px = Entropy.Next(0, Constants.StageWidth - 1);
            int py = Entropy.Next(0, Constants.BoardRows);
            prayer.X = px + oldColumns;
            prayer.Y = py;
            Game.State.Prayers.Add(prayer);

            BlockStack ps = Game.State.Board[oldColumns + px, py];
            prayer.BottomPosition = ps.TopPosition;
            prayer.Vector = Constants.DroppedVector;
            ps.Add(prayer);
        }
        #endregion

        #region Block Management
        /// <summary>
        /// Returns true if there is at least one block moving on the board.
        /// </summary>
        public bool IsMoving
        {
            get
            {
                // Loop through
                foreach (IList<BlockStack> stacks in this)
                    foreach (BlockStack stack in stacks)
                        foreach (Block block in stack)
                            if (block.IsMoving)
                                return true;

                // We didn't find any
                return false;
            }
        }

        /// <summary>
        /// Constructs a block of the given key and returns the results.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Block CreateBlock(string key)
        {
            // Create the basic block
            Block block = new Block(AssetLoader.Instance.CreateSprite(key));

            // Set up some of the default properties
            block.CastsShadows = true;
            block.Height = 1;
            block.Mass = Constants.BlockMass;

            // Return the results
            return block;
        }

		/// <summary>
		/// Returns true if there is at least one block with the given
		/// name on top of a stack.
		/// </summary>
		public bool HasTopBlock(string drawableName)
		{
			// Loop through
			foreach (IList<BlockStack> stacks in this)
			{
				foreach (BlockStack stack in stacks)
				{
					// Grab the top and check the name
					if (stack.Count > 0 && 
						stack.TopBlock.Sprite.ID == drawableName)
					{
						return true;
					}
				}
			}
			
			// We didn't find any
			return false;
		}

        /// <summary>
        /// Returns true if the block is one of the six standard blocks
        /// that may be grabbed.
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public static bool IsGroundBlock(Block block)
        {
			// Ignore nulls
			if (block == null ||
				block.Sprite == null ||
				block.Sprite.ID == null)
			{
				return false;
			}

            // Get the name
            string name = block.Sprite.ID;

            // Return if it contains the appropriate name
            return name.Contains("Water") ||
                name.Contains("Dirt") ||
                name.Contains("Grass");
        }

        /// <summary>
        /// Returns true if the block is an immobile block.
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public static bool IsImmobileBlock(Block block)
        {
            return block.Sprite.ID == Constants.ImmobileBlockName;
        }

		/// <summary>
		/// Replaces every top-facing block with a drawable name equal
		/// to the search name and replaces it with the replacement name.
		/// </summary>
		public void ReplaceTopDrawables(string searchName, string replaceName)
		{
			// Loop through the stacks
			foreach (IList<BlockStack> stacks in this)
			{
				foreach (BlockStack stack in stacks)
				{
					// Ignore empty blocks
					if (stack.Count == 0)
						continue;

					// Grab the block
					Block b = stack.TopBlock;

					if (b.Sprite.ID == searchName)
					{
						b.Sprite = AssetLoader.Instance
							.CreateSprite(replaceName);
					}
				}
			}
		}
        #endregion

        #region Prayers
        /// <summary>
        /// Checks the board against all of the current prayers
        /// and initiates the construction phase.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void CheckLayout(int x, int y)
        {
            // Ignore empty prayers
            if (Game.State.Prayers.Count == 0)
                return;

            // We want to loop through the prayers
            ArrayList<Prayer> tmpPrayers = new ArrayList<Prayer>();
            tmpPrayers.AddAll(Game.State.Prayers);

            foreach (Prayer prayer in tmpPrayers)
            {
                // Don't bother if we haven't been accepted
                if (!prayer.IsAccepted)
                    continue;

                // Check the layout, if it returns true, we are done
                if (CheckLayouts(x, y, prayer))
                    return;
            }
        }

        /// <summary>
        /// Checks a specific prayer against a specific location,
        /// returning true if the layout is found and false if it
        /// is invalid.
        /// </summary>
        private bool CheckLayout(int x, int y, Prayer prayer)
        {
			// Figure out the extents for searching on this point
            int pRows = prayer.Board.Rows;
            int pCols = prayer.Board.Columns;
			int bRows = Game.State.Board.Rows;
			int bCols = Game.State.Board.Columns;
			Block first = null;

			// Sanity checking
			if (x < 0 || y < 0 ||
				x + pCols > bCols ||
				y + pRows > bRows)
			{
				return false;
			}

			// Noise
			Debug("Checking for prayer at {0},{1}", x, y);

			// Loop through the columns
			for (int i = 0; i < pCols; i++)
			{
				// Get the game column
				IList<BlockStack> boardColumn =	Game.State.Board[x + i];
				
				// Loop through each of the rows
				for (int j = 0; j < pRows; j++)
				{
					// Grab the top block
					BlockStack stack = prayer.Board[i, j];
					
					// If this stack is empty, just skip it as valid
					if (stack.Count == 0)
						continue;
					
					// Grab the stack
					Block pBottom = stack.BottomBlock;

					// Grab the board block
					BlockStack bStack = boardColumn[y + j];
					Block bTop = bStack.TopBlock;
					
					// Compare the drawable names
					if (pBottom.Sprite.ID == "Invisible")
					{
						// Invisible always matches
						continue;
					}

					if (bTop.Sprite.ID != pBottom.Sprite.ID)
					{
						Debug("  Name rejected prayer: {0} v. {1}",
							bTop.Sprite.ID, pBottom.Sprite.ID);
						return false;
					}
							
					// Copy first or verify that the position is valid
					if (first == null)
					{
						first = bTop;
					}
					else
					{
						if (first.BottomPosition != bTop.BottomPosition)
						{
							Debug("  Position rejected prayer: {0} v {1}",
								first.BottomPosition, bTop.BottomPosition);
							return false;
						}
					}
				}
			}

            // We didn't have any errors
			Debug("  Found prayer!");
            return true;
        }

        /// <summary>
        /// Checks a specific location for a given set of prayers.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private bool CheckLayouts(int x, int y, Prayer prayer)
        {
			// We check all the board coordinates that may include
            // this point as a coordinate.
            int pCols = prayer.Board.Columns;
            int pRows = prayer.Board.Rows;

			for (int ox = -pCols; ox < pCols; ox++)
			{
				for (int oy = -pRows; oy < pRows; oy++)
				{
					// Check it
					bool results = CheckLayout(x + ox, y + oy, prayer);
					
					// If we are true, process it
					if (results)
					{
                        // We found a layout, so process the layout event
                        ProcessFoundLayout(x + ox, y + oy, prayer);
                        return true;
					}
				}
			}

            // We didn't find a prayer
            return false;
        }

        /// <summary>
        /// Migrates the current board to the other one, dropping
        /// everything on top.
        /// </summary>
        /// <param name="board"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void MigrateBoard(Board board, int x, int y)
        {
            // Go through the columns
            for (int i = 0; i < Columns; i++)
            {
                // Get the data for speed
                IList<BlockStack> stacks = Game.State.Board[x + i];
                IList<BlockStack> stacks0 = this[i];

                // Go through the rows
                for (int j = 0; j < Rows; j++)
                {
                    // Get the stacks
                    BlockStack stack = stacks[y + j];
                    BlockStack stack0 = stacks0[j];

                    // Get the position
					Block bBlock = stack.TopBlock;
                    float position = stack.TopPosition;

                    // Move everything but the bottom-most one
                    foreach (Block block in stack0)
                    {
						// Positions 0's are sealed
                        if (block.BottomPosition == 0)
						{
							// Seal it to change how it looks on the
							// mini map
							switch (block.Sprite.ID)
							{
							case "Water Block":
								bBlock.Sprite =	AssetLoader.Instance
									.CreateSprite("Sealed Water Block");
								bBlock.Data = false;
								break;
							case "Grass Block":
								bBlock.Sprite =	AssetLoader.Instance
									.CreateSprite("Sealed Grass Block");
								bBlock.Data = false;
								break;
							case "Dirt Block":
								bBlock.Sprite =	AssetLoader.Instance
									.CreateSprite("Sealed Dirt Block");
								bBlock.Data = false;
								break;
							case "Invisible":
								// Do nothing
								break;
							default:
								Error("Cannot identify base block: {0}",
									block.Sprite.ID);
								break;
							}
						}
						else
						{
							// Move it over to the new one and add the position
							// to the bottom, minus one because we ignore the
							// bottom row
							block.BottomPosition += position -1f;
							stack.Add(new Block(block));
						}                   
					}
                }
            }
        }

        /// <summary>
        /// Handles the actual processing of a found prayer layout and
        /// places the game in completion mode.
        /// </summary>
        private void ProcessFoundLayout(int x, int y, Prayer prayer)
        {
            // Activate the minor mode
            (Game.GameMode as PlayMode).MinorMode =
                new PrayerCompletedMinorMode(x, y, prayer);
        }
        #endregion

		#region Bugs
		/// <summary>
		/// Adds some bugs into the stage.
		/// </summary>
		public void AddBugs(int howMany)
		{
			// Loop through the bugs
			for (int i = 0; i < howMany; i++)
			{
				// Create a bug
				Bug bug = new Bug();

				// Place the bug
				while (true)
				{
					// Find a random location
                    int col = Entropy.Next(0, Columns);
                    int row = Entropy.Next(0, Constants.BoardRows);
					BlockStack stack = this[col, row];

					// Go through the stack, looking for bugs
					float immobilePosition = 0;
					bool foundGround = false;
					bool validStack = false;

					foreach (Block block in stack)
					{
						// Don't bother if we already have a bug here
						if (block.Sprite.ID == Constants.BugBlockName)
						{
						}

						// Make sure we have at least one land block
						else if (IsGroundBlock(block))
						{
							foundGround = true;
						}

						// If we have an immobile, keep it
						else if (block.Sprite.ID ==
							Constants.ImmobileBlockName)
						{
							immobilePosition =
								Math.Max(block.TopPosition, immobilePosition);
							validStack = true;
						}

						// Not valid, a character or someting
						else
						{
							validStack = false;
							break;
						}
					}

					// See if we have a valid one
					if (!validStack || !foundGround)
						continue;

					// Put the bug in
					bug.BottomPosition = immobilePosition;
					bug.X = col;
					bug.Y = row;
					bug.BlockStack = stack;
					stack.Add(bug);
					stack.Sort();
					Game.State.Bugs.Add(bug);
					break;
				}
			}
		}
		#endregion
    }
}
