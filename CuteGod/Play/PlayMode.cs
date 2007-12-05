using BooGame;
using BooGame.Input;
using C5;
using MfGames.Sprite3;
using MfGames.Sprite3.Backends;
using MfGames.Sprite3.Gui;
using MfGames.Sprite3.PlanetCute;
using MfGames.Utility;
using System;
using System.Drawing;

namespace CuteGod.Play
{
    /// <summary>
    /// The bulk of the game, this is what actually allows the user
    /// to play the game. It has minor modes (stealing from Emacs)
    /// which represent specific aspects of the game, such as the
    /// scoring parts or the end game components.
    /// </summary>
    public class PlayMode
	: Logable, IGameMode, IGuiMouseListener
    {
        #region Activation and Deactivation
        /// <summary>
        /// Sets the size of the game mode (based on the screen).
        /// </summary>
        public SizeF Size
        {
            set
            {
                // Make sure everything is properly sized
                sprites.Size = value;
                blockViewport.Point = new PointF(0, 0);
                blockViewport.Size = value;
                speeches.Size = value;
				environment.Size = value;

                if (minorMode != null)
                    minorMode.Size = value;

                // Adjust the scroller
                scroller.EdgeWidth = value.Height / 10;

                // Arrange the minimap at the bottom
                minimap.Point =
                    new PointF(0, value.Height - minimap.Size.Height);
                minimap.Size =
                    new SizeF(value.Width, minimap.Size.Height);

                // Adjust the indicators to be twice the height of
                // the minimap. The coordinates don't change since the
                // indicators are on the top of the window.
                indicators.Size = new SizeF(value.Width,
                    minimap.Size.Height * 1.6f);
            }
        }

        /// <summary>
        /// Indicates to the game mode that it is activated.
        /// </summary>
        public void Activate()
        {
			// If we don't have a game state, kick this over to the
            // new game state as an error condition.
            if (Game.State == null)
                Game.GameMode = new NewGameMode();

            // Create the sprite manager
            sprites = new SpriteViewport();
            sprites.IsMouseSensitive = true;
            sprites.ClipContents = true;
            Game.SpriteManager.Add(sprites);

            // Create the board viewport
            blockViewport =
                new BlockViewport(Game.State.Board, AssetLoader.Instance);
            blockViewport.ClipContents = true;
            blockViewport.IsMouseSensitive = true;
            blockViewport.MousePosition = 8;
            blockViewport.FocusPosition = Constants.FocusPosition;
            sprites.Add(blockViewport);

			// Create the top-down vertical explorer
			vertViewport = new VerticalViewport(Game.State.Board);
			vertViewport.Z = 50;
			vertViewport.BackgroundColor = Color.FromArgb(32, Color.Black);
			vertViewport.Padding = 2;
			vertViewport.BlockWidth = vertViewport.BlockHeight = 16;
			vertViewport.MaximumHeight = vertViewport.MinimumHeight = 4;
			vertViewport.Size = vertViewport.Extents.Size;
			vertViewport.Visible = false;
			vertViewport.Opacity = 0.75f;
			sprites.Add(vertViewport);

            // Speeches and overlays
            speeches = new SpriteViewport();
            speeches.Z = 100;
            speeches.IsMouseSensitive = true;
            sprites.Add(speeches);

            // Connect our events for selectors
            blockViewport.StackMouseEnter += OnStackMouseEnter;
            blockViewport.StackMouseExit += OnStackMouseExit;

            // Create the scroller on the viewport
            scroller = new MouseEdgeScroller();
            scroller.Register(blockViewport);

            // Create our selector icon
            selector = AssetLoader.Instance.CreateBlock("Selector");
            selector.Height = 0;
            selector.CastsShadows = false;

            // Create the scores and other indicators
            CreateIndicators();

            // Connect the events
            Game.State.Board.MovingChanged += OnMovingChanged;
			Game.State.Board.BlockMovingChanged += OnBlockMovingChanged;
            Game.State.Board.BlockMovingChanged += Game.Sound.BlockLanded;
        }

        /// <summary>
        /// Triggered when the game mode is deactivated.
        /// </summary>
        public void Deactivate()
        {
            // Remove ourselves from the sprite manager
            Game.SpriteManager.Remove(sprites);

            // Disconnect the events
            Game.State.Board.MovingChanged -= OnMovingChanged;
            Game.State.Board.BlockMovingChanged -=
				Game.Sound.BlockLanded;
			Game.GuiManager.MouseUpListeners.Remove(this);
			Game.GuiManager.MouseDownListeners.Remove(this);

			// Remove the minor mode
			if (minorMode != null)
				minorMode = null;
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
				// Go to the main menu
				Game.GameMode = new MainMenuMode();
				break;

#if DEBUG
			case BooGameKeys.F3:
				// Add to the minor mode if we already have one
				if (MinorMode != null && MinorMode is ChestOpenedMinorMode)
				{
					// Just increment it
					(MinorMode as ChestOpenedMinorMode).ChestCounter++;
				}
				else
				{
					// Open up the new mode
					MinorMode = new ChestOpenedMinorMode(1);
				}
				return;

			case BooGameKeys.F4:
				// Add a bug into the board
				Game.State.Board.AddBugs(1);
				return;

			case BooGameKeys.F5:
				// Give another stage with penalties
				MinorMode = new NewStageMinorMode();
				return;
				
			case BooGameKeys.F6:
				// Creates a test prayer and inserts it into the system.
				Prayer prayer = Game.PrayerFactory.CreatePrayer();
				prayer.IsAccepted = true;
				Game.State.Prayers.Add(prayer);
				return;
				
			case BooGameKeys.F7:
				Game.State.GrabCount = 3;
				return;
				
			case BooGameKeys.F8:
				// Finish up a prayer
				Prayer p = Game.PrayerFactory.CreatePrayer();
				MinorMode = new PrayerCompletedMinorMode(0, 0, p);
				return;
				
			case BooGameKeys.F9:
				// Finish the game
				MinorMode = new EndOfGameMinorMode();
				return;
#endif
			}
			
			// If we have a minor mode, pass it on
			if (minorMode != null)
				minorMode.Key(args);
		}
		#endregion

        #region Sprites
		private Environment environment = new Environment();
        private SpriteViewport sprites;
        private IndicatorsViewport indicators;
        private BlockViewport blockViewport;
		private VerticalViewport vertViewport;
        private MouseEdgeScroller scroller;
        private SpriteViewport speeches;

        MinimapViewport minimap;

        /// <summary>
        /// Creates the various window widgets around the main play view.
        /// All indicators are higher than the block viewport.
        /// </summary>
        private void CreateIndicators()
        {
            // Create the indicators
            indicators = new IndicatorsViewport();
            sprites.Add(indicators);

            // Create the minimap
            minimap = new MinimapViewport(this);
            sprites.Add(minimap);
        }
        #endregion

        #region Minor Modes
        private IPlayMinorMode minorMode;

        /// <summary>
        /// Sets a minor mode for the play or clears it.
        /// </summary>
        public IPlayMinorMode MinorMode
        {
            get { return minorMode; }
            set
            {
                // If we have a minor mode, deactivate it
                if (minorMode != null)
                    minorMode.Deactivate();

                // Set the mode
                minorMode = value;

                // If we have one, activate it
                if (minorMode != null)
                {
                    minorMode.Size = sprites.Size;
                    minorMode.Activate();
                }
            }
        }
        #endregion

        #region Drawing
        /// <summary>
        /// Triggers a rendering of the game mode.
        /// </summary>
        /// <param name="args"></param>
        public void Draw(DrawingArgs args)
        {
			// Draw the background
			environment.DrawBackground(args);

            // See if we have a minor mode
            bool hasMinorMode = minorMode != null;

            // Check for a stack
            if (!hasMinorMode && currentStack != null)
            {
                // Add the grab stack
                AddGrabStack();
            }

            // Pass it on to the sprites
            MoveSpeechSprites(args);
            sprites.Draw(args);

            // Clear out the minor mode stuff
            if (!hasMinorMode)
            {
                // Remove it
                RemoveGrabStack();
            }

            // If we have a minor mode, first screen out everything
            if (hasMinorMode)
            {
                // Draw a rectangle
                args.Backend.DrawFilledRectangle(
                    new RectangleF(sprites.Point, sprites.Size),
                    minorMode.BackgroundColor);

                // Draw the backend mode
                minorMode.Draw(args);
            }

			// Draw the swarms
			DrawSwarms(args);
			environment.DrawForeground(args);
        }
        #endregion

        #region Updating
        /// <summary>
        /// Triggers an update of the mode's state.
        /// </summary>
        /// <param name="args"></param>
        public void Update(UpdateArgs args)
        {
			// Update the environment
			environment.Update(args);

            // Update the blocks
            Game.State.Board.Update(args);
            UpdateSpeechBubbles(args);
			UpdateSwarms(args);
			Game.State.Prayers.Update(args);
			Game.State.Bugs.Update(args);

            // See if we need to update our minor mode
            if (minorMode != null)
			{
				// Update the minor mode
                minorMode.Update(args);
			}
			else
			{
				// Only update the state if we don't have a minor mode
				Game.State.Update(args);
			}
        }
        #endregion

        #region Selection and Grabbing
        private Block[] grabStack = new Block[Constants.MaximumGrabCount];
        private Block selector;

        private BlockStack currentStack;
		private BlockStack lastGrabStack;
		private LinkedList<Block> droppingBlocks = new LinkedList<Block>();

        /// <summary>
        /// Defines the priority of the mouse events, this is set to
        /// an arbitrarily high one to indicate that the mode really wants
        /// to be in charge.
        /// </summary>
        public float MouseListenerPriority
        {
            get { return 1000; }
        }

        /// <summary>
        /// Adds and repositions the block stack from the board.
        /// </summary>
        /// <param name="stack"></param>
        private void AddGrabStack()
        {
            // Ignore nulls stacks
            if (currentStack == null)
                return;

            // Figure out the top-most block that isn't moving
            float topPosition = GetTopPosition();
            float grabPosition =
                Math.Max(topPosition + Constants.GrabStackOffset,
                Constants.MinimumGrabHeight);

            // Move the selector
            selector.BottomPosition = topPosition;
            currentStack.Add(selector);

            // Go through the blocks
            for (int i = 0; i < grabStack.Length; i++)
            {
                // Stop processing when we hit a null
                if (grabStack[i] == null)
                    break;

                // Reset the position
                currentStack.Add(grabStack[i]);
                grabStack[i].BottomPosition = grabPosition + (float) i;
            }
        }

        /// <summary>
        /// This returns true if the currently selected block can be
        /// dropped on by the bottom-most (index 0) index of the grab
        /// stack.
        /// </summary>
        /// <param name="stack"></param>
        /// <returns></returns>
        private bool CanDrop()
        {
            // Ignore nulls stacks
            if (currentStack == null)
                return false;

            // We don't bother if we don't have anything to drop
            if (grabStack[0] == null)
                return false;

            // Get the positions we use
            float topPosition = currentStack.TopPosition;
            Block topBlock = currentStack.TopBlock;

            // Check the top position
            if (topPosition >= Constants.TopGroundPosition)
                // We have too many blocks
                return false;

            // See if we are a ground block
            if (!Board.IsGroundBlock(topBlock) &&
                !Board.IsImmobileBlock(topBlock) &&
				topBlock.Sprite.ID != Constants.BugBlockName ||
				topBlock.Data != null)
                // We aren't a grabbable block
                return false;

            // It appears to be a valid block
            return true;
        }

        /// <summary>
        /// This returns true if the current stack may be grabbed from.
        /// </summary>
        /// <param name="stack"></param>
        /// <returns></returns>
        private bool CanGrab()
        {
            // Ignore everything if we don't have a stack
            if (currentStack == null)
                return false;

            // See if we have room to grab things
            if (grabStack[Game.State.GrabCount - 1] != null)
                // The highest one is filled, so we can't grab
                return false;

            // Get the positions we use
            float topPosition = currentStack.TopPosition;
            Block topBlock = currentStack.TopBlock;

            // Check the top position
            if (topPosition == Constants.TopImmobilePosition)
                // We only have an immobile block
                return false;

            // See if we are a ground block
            if (!Board.IsGroundBlock(topBlock))
                // We aren't a grabbable block
                return false;

			// If we have data in the block, it is immobile
			if (topBlock.Data != null)
				return false;

            // It appears to be a valid block
            return true;
        }

        /// <summary>
        /// Drops the bottom-most grab stack (index 0) from the stack
        /// onto the currently selected stack.
        /// </summary>
        /// <param name="stack"></param>
        private void Drop()
        {
            // Don't bother if we can't drop
            if (currentStack == null || !CanDrop())
                return;

            // Grab the block we are dropping
            Block block = grabStack[0];

            // Move the rest of the stack down
            for (int i = 0; i < Constants.MaximumGrabCount - 1; i++)
                grabStack[i] = grabStack[i + 1];

            grabStack[Constants.MaximumGrabCount - 1] = null;

            // Set down the vector
            block.BottomPosition -= 1f;
            block.Vector = Constants.DroppedVector;

            // Put that block back into the stack
			droppingBlocks.Add(block);
            currentStack.Add(block);

            // Mark this as the end of the turn
			if (currentStack != lastGrabStack)
			{
				// Subtract a heart and score it
				Game.State.Hearts -= Game.State.GrabCost;
				Game.State.EndTurn();
			}

			// Reset the grab stack for "undo"
			lastGrabStack = null;
        }

        /// <summary>
        /// Returns true if this block is in the grab stack.
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public bool IsInGrabStack(Block block)
        {
            // Look through the grab stack
            foreach (Block b in grabStack)
                if (b != null && b == block)
                    return true;

            // We didn't find it
            return false;
        }

        /// <summary>
        /// Returns true if the block is the selector.
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public bool IsSelector(Block block)
        {
            return block == selector;
        }

        /// <summary>
        /// Determines if the current stack is a valid target.
        /// </summary>
        /// <param name="stack"></param>
        /// <returns></returns>
        private bool IsValidTarget()
        {
            // Ignore nulls
            if (currentStack == null)
                return false;

            // Since we allow either or, check both
            return CanDrop() || CanGrab();
        }

        /// <summary>
        /// Calculates the top, non-grabStack position.
        /// </summary>
        /// <returns></returns>
        private float GetTopPosition()
        {
            float topPosition = 0;
            ArrayList<Block> blocks = new ArrayList<Block>();
            blocks.AddAll(currentStack);
            blocks.Reverse();

            foreach (Block block in blocks)
            {
                // See if we are moving or if we have no height
                if (block.IsMoving || block.Height == 0)
                    continue;

                // Grab this one
                topPosition = block.TopPosition;
                break;
            }
            return topPosition;
        }

        /// <summary>
        /// Grabs a block from the currently selected stack.
        /// </summary>
        /// <param name="stack"></param>
        private void Grab()
        {
            // We only care if we can grab something
            if (currentStack == null || !CanGrab())
                return;

            // Move everything up in the stack.
            Block topBlock = currentStack.TopBlock;

            for (int i = Constants.MaximumGrabCount - 1; i > 0; i--)
                grabStack[i] = grabStack[i - 1];

            // Grab the top block and change its position
            grabStack[0] = topBlock;
            topBlock.IsMoving = false;
            topBlock.Vector = 0;
            topBlock.Mass = 0;

            // Remove the stack for processing
            currentStack.Remove(topBlock);

            // Fire the sound event
            BlockStackEventArgs args = new BlockStackEventArgs();
            args.Stack = currentStack;
            args.Block = topBlock;
            Game.Sound.BlockGrabbed(this, args);

			// Save the last grab stack
			lastGrabStack = currentStack;
        }

        /// <summary>
        /// Triggered when the mouse button is pressed down.
        /// </summary>
        /// <param name="args"></param>
        public void MouseDown(GuiMouseEventArgs args)
        {
        }

        /// <summary>
        /// Triggered with mouse movement.
        /// </summary>
        /// <param name="args"></param>
        public void MouseMotion(GuiMouseEventArgs args)
        {
            // We don't do anything about this one
        }

        /// <summary>
        /// Triggered when the mouse is released.
        /// </summary>
        /// <param name="args"></param>
        public void MouseUp(GuiMouseEventArgs args)
        {
			// Pass it to the minor mode if we have it
            if (minorMode != null)
			{
				minorMode.MouseUp(args);
                return;
			}

            // Figure out the mouse button
            if (args.Button == MfGames.Sprite3.Gui.MouseButtons.Left)
            {
                // Grab it
                Grab();
            }
            else if (args.Button == MfGames.Sprite3.Gui.MouseButtons.Right)
            {
                // Drop the block
                Drop();
            }
        }

		/// <summary>
		/// Triggered when a block stops moving.
		/// </summary>
		private void OnBlockMovingChanged(
			object sender, BlockStackEventArgs args)
		{
			// Only bother if we are dropping a ground block
			if (!Board.IsGroundBlock(args.Block) ||
				!droppingBlocks.Contains(args.Block))
				return;
			
			// First check for bugs
			foreach (Block block in args.Stack)
			{
				// Ignore processing ourselves
				if (args.Block == block)
					continue;

				// Ignore if the position isn't close
				if ((int) args.Block.BottomPosition != (int) block.TopPosition)
					continue;
				
				// Check for the drawable
				if (block.Sprite.ID == Constants.BugBlockName)
				{
					// Remove the bug
					(block as Bug).Remove();
					
					// Start falling again
					args.Block.BottomPosition -= 1f;
					args.Block.Vector = Constants.DroppedVector;

					// Finish processing
					return;
				}
			}

			// Remove it
			droppingBlocks.Remove(args.Block);
		}

        /// <summary>
        /// Triggered when an entire block stack stops moving.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnMovingChanged(object sender, BlockStackEventArgs args)
        {
			// Check the board, but only if we dropped a block
			if (Board.IsGroundBlock(args.Stack.TopBlock))
				Game.State.Board.CheckLayout(args.Stack.X, args.Stack.Y);
        }

        /// <summary>
        /// Triggered when the mouse enters a specific stack
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnStackMouseEnter(object sender, BlockStackEventArgs args)
        {
            // Ignore if we have a minor mode
            if (minorMode != null)
                return;

            // Add the selector to the current position and set it on top
            currentStack = args.Stack;

            // Remove the stack, just in case
            bool isValidTarget = IsValidTarget();

            // See if we have a valid target
            if (!isValidTarget)
            {
                // Change the selector so it shows an incorrect selection
                selector.Sprite = AssetLoader.Instance
					.CreateSprite(Constants.InvalidSelectorName);
				vertViewport.Visible = false;

                // We are done
                return;
            }

            // Change the selector to show a valid selector.
            selector.Sprite = AssetLoader.Instance
				.CreateSprite(Constants.SelectorName);

			// Move the vertical stack display to the currently
			// selected stack and make it visible
			PointF vertPoint = blockViewport.ToPoint(
				currentStack.X, currentStack.Y,
				currentStack.TopPosition);
			vertViewport.Visible = true;
			vertViewport.BlockStackX = currentStack.X;
			vertViewport.BlockStackY = currentStack.Y;
			vertViewport.Point = new PointF(
				vertPoint.X
				+ blockViewport.BlockWidth / 2
				- vertViewport.Size.Width / 2,
				vertPoint.Y);

            // We are going to be grabbing or dropping
            Game.GuiManager.MouseUpListeners.Add(this);
        }

        /// <summary>
        /// Triggered when a mouse leaves a stack.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnStackMouseExit(object sender, BlockStackEventArgs args)
        {
            // Ignore if we have a minor mode
            if (minorMode != null)
                return;

            // If we don't have a current stack, don't bother
            if (currentStack == null)
                return;

            // Remove the selectors
            // We use RemoveAll() because of a duplication bug
            currentStack = null;
			vertViewport.Visible = false;

            // Since we won't be listening to down, remove it
            Game.GuiManager.MouseUpListeners.Remove(this);
        }

        /// <summary>
        /// Removes the entire grab stack from the given block stack.
        /// </summary>
        /// <param name="stack"></param>
        private void RemoveGrabStack()
        {
            // Ignore nulls
            if (currentStack == null)
                return;

            // Remove everything
            foreach (Block block in grabStack)
            {
                // Ignore nulls
                if (block == null)
                    break;

                // Remove it
                currentStack.RemoveAllCopies(block);
            }


            // Remove the selector
            currentStack.Remove(selector);
        }
        #endregion

        #region Prayer Indicators
        private HashDictionary<Prayer, PrayerSpeechSprite> speechSprites =
            new HashDictionary<Prayer, PrayerSpeechSprite>();

        /// <summary>
        /// Moves the speech sprites appropriately for drawing.
        /// </summary>
        /// <param name="args"></param>
        private void MoveSpeechSprites(DrawingArgs args)
        {
            foreach (Prayer prayer in Game.State.Prayers)
            {
                // Ignore if we don't have it
                if (!speechSprites.Contains(prayer))
                    continue;

                // Move the sprite
                PrayerSpeechSprite pss = speechSprites[prayer];
                PointF p = blockViewport.ToPoint(
                    prayer.X,
                    prayer.Y,
                    prayer.BottomPosition);
                p.X += blockViewport.BlockWidth - blockViewport.BlockWidth / 3;
                p.Y -= 2 * blockViewport.BlockOffsetZ;
                pss.Point = p;
            }
        }

        /// <summary>
        /// Updates the various speech bubbles for all prayers.
        /// </summary>
        private void UpdateSpeechBubbles(UpdateArgs args)
        {
            // Keep a list of keys, so we know which ones to remove
            ArrayList<Prayer> tmpPrayers = new ArrayList<Prayer>();
            tmpPrayers.AddAll(speechSprites.Keys);

            // Loop through our list of speed sprites
            foreach (Prayer prayer in Game.State.Prayers)
            {
                // See if we have one
                PrayerSpeechSprite pss = null;

                if (!speechSprites.Contains(prayer))
                {
                    // We need to construct the speech sprite
                    pss = new PrayerSpeechSprite(prayer);
                    speeches.Add(pss);
                    speechSprites[prayer] = pss;
                }
                else
                {
                    // Pull out the speech sprite
                    pss = speechSprites[prayer];
                    tmpPrayers.Remove(prayer);
                }

                // Update the sprite
                pss.Update(args);
            }

            // Remove everything left since we don't have it anymore
            foreach (Prayer prayer in tmpPrayers)
            {
                // Remove from our sprites
                speeches.Remove(speechSprites[prayer]);
                speechSprites.Remove(prayer);
            }
        }
        #endregion

		#region Swarming
		private LinkedList<SwarmParticle> heartSwarm =
			new LinkedList<SwarmParticle>();
		private LinkedList<SwarmParticle> starSwarm =
			new LinkedList<SwarmParticle>();

		/// <summary>
		/// Draws out all the swarms on the screen.
		/// </summary>
		private void DrawSwarms(DrawingArgs args)
		{
			// Draw the hearts if we have something
			if (heartSwarm.Count > 0)
			{
				// Get the drawable
				IDrawable id = AssetLoader.Instance
					.CreateDrawable(Constants.MiniHeartName);

				// Draw out the swarm
				foreach (SwarmParticle sp in heartSwarm)
				{
					sp.Draw(id, args);
				}
			}

			// Draw the hearts if we have something
			if (starSwarm.Count > 0)
			{
				// Get the drawable
				IDrawable id = AssetLoader.Instance
					.CreateDrawable(Constants.MiniStarName);

				// Draw out the swarm
				foreach (SwarmParticle sp in starSwarm)
				{
					sp.Draw(id, args);
				}
			}
		}

		/// <summary>
		/// Updates the position of the swarms.
		/// </summary>
		private void UpdateSwarms(UpdateArgs args)
		{
			// Draw the hearts if we have something
			LinkedList<SwarmParticle> list = new LinkedList<SwarmParticle>();

			if (heartSwarm.Count > 0)
			{
				// Draw out the swarm
				foreach (SwarmParticle sp in heartSwarm)
				{
					// Update the particle
					sp.Update(args);

					// Remove it if died
					if (sp.Died)
					{
						list.Add(sp);
						Game.State.Hearts++;
					}
				}

				// Remove the list
				foreach (SwarmParticle sp1 in list)
					heartSwarm.Remove(sp1);
			}

			// Draw the hearts if we have something
			if (starSwarm.Count > 0)
			{
				// Draw out the swarm
				list.Clear();

				foreach (SwarmParticle sp in starSwarm)
				{
					// Update the particle
					sp.Update(args);

					// Remove it if died
					if (sp.Died)
					{
						list.Add(sp);
						Game.State.Stars++;
					}
				}

				// Remove the list
				foreach (SwarmParticle sp1 in list)
					starSwarm.Remove(sp1);
			}
		}

		/// <summary>
		/// Creates a number of hearts at a given point and swarms
		/// them to the location on the screen. As they reach the
		/// point, this plays the new heart sound and adds the heart
		/// to the score. The directions of the hearts are random and
		/// radiate out from the point.
		/// </summary>
		public void SwarmHearts(int number, PointF origin)
		{
			// Create a number of hearts swarm particals
			for (int i = 0; i < number; i++)
			{
				// Create the particle
				SwarmParticle sp = new SwarmParticle();

				sp.CurrentX = origin.X;
				sp.CurrentY = origin.Y;

				sp.TargetX = indicators.HeartPoint.X;
				sp.TargetY = indicators.HeartPoint.Y;

				// Add the heart
				heartSwarm.Add(sp);
			}
		}

		/// <summary>
		/// Creates a number of stars at a given point and swarms
		/// them to the location on the screen. As they reach the
		/// point, this plays the new star sound and adds the star
		/// to the score. The directions of the stars are random and
		/// radiate out from the point.
		/// </summary>
		public void SwarmStars(int number, PointF origin)
		{
			// Create a number of stars swarm particals
			for (int i = 0; i < number; i++)
			{
				// Create the particle
				SwarmParticle sp = new SwarmParticle();

				sp.CurrentX = origin.X;
				sp.CurrentY = origin.Y;

				sp.TargetX = indicators.StarPoint.X;
				sp.TargetY = indicators.StarPoint.Y;

				// Add the star
				starSwarm.Add(sp);
			}
		}
		#endregion
    }
}
