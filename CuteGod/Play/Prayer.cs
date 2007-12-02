using MfGames.Sprite3;
using MfGames.Sprite3.PlanetCute;
using MfGames.Utility;
using System;

namespace CuteGod.Play
{
    /// <summary>
    /// Represents a single prayer in the system. This includes the layout
    /// for the prayer. This also includes the block drawable for the
    /// character requesting the figure.
    /// </summary>
    public class Prayer
        : Block
    {
        #region Constructors
        /// <summary>
        /// Create an empty prayer.
        /// </summary>
        public Prayer()
            : base(GetRandomCharacterSprite())
        {
            // Set the block information
            Height = 1;
            CastsShadows = false;

            // TODO Set the mass to half a normal block to make sure
            // they fall properly instead of just getting stuck.
            Mass = Constants.BlockMass / 2;

            // Create a random board and assign it
            this.board = Game.PrayerFactory.CreatePrayerBoard();

			// Set up the bounce
			timeUntilBounce = Entropy.NextDouble(
				Constants.MinimumCharacterBounce,
				Constants.MaximumCharacterBounce);

			// Connect some events
			DirectionChanged += OnDirectionChanged;
        }

        /// <summary>
        /// Create a prayer with a specific board.
        /// </summary>
        /// <param name="board"></param>
        public Prayer(Board board)
            : this()
        {
            this.board = board;
        }
        #endregion

        #region Properties
        public int BonusValue = 15;
        public int BonusStep = 10;
        public int BonusCounter = 0;
        public bool IsAccepted = false;
        public int X;
        public int Y;
		private double timeUntilBounce;
        #endregion

        #region Board
        private Board board = new Board();

        /// <summary>
        /// Contains the block board that represents both the layout
        /// needed to create the board and also the constructions that
        /// are placed on top of it.
        /// </summary>
        public Board Board
        {
            get { return board; }
        }
        #endregion

        #region Block
		/// <summary>
		/// Returns a randomly created character sprite.
		/// </summary>
		private static ISprite GetRandomCharacterSprite()
		{
			return AssetLoader.Instance.CreateSprite("Character");
		}
        #endregion

		#region Events
		/// <summary>
		/// This event is used to handle jumping from one block to
		/// another.
		/// </summary>
		private void OnDirectionChanged(
			object sender,
			BlockStackEventArgs args)
		{
			Debug("OnDirectionChanged: {0}", direction);

			// At the apex, we change our direction by moving to the
			// next stack as appropriate. Start by ignoring direction
			// 0 since that is an inplace jumping.
			if (direction == 0)
				return;

			// Get the destination stack
			Board board = Game.State.Board;
			BlockStack srcStack = board[X, Y];
			BlockStack destStack = null;

			if (direction == 1)
				destStack = board[X, Y - 1];
			else if (direction == 2)
				destStack = board[X + 1, Y];
			else if (direction == 3)
				destStack = board[X, Y + 1];
			else if (direction == 4)
				destStack = board[X - 1, Y];

			// Make sure we are still a valid destination
			if (IsInvalidTarget(srcStack, destStack))
			{
				// We want to reverse the direction, but stay in the
				// same stack because our destination became
				// invalid. This works because the code will show them
				// "bouncing" back.
				if (direction == 1) direction = 3;
				else if (direction == 2) direction = 4;
				else if (direction == 3) direction = 1;
				else if (direction == 4) direction = 2;

				// We are done
				Debug("OnDirectionChanged: Destination no longer valid");
				srcStack.Add(this);
				return;
			}

			// Move this block to the next one
			Debug("Removing: {0}", srcStack);
			srcStack.Remove(this);
			destStack.Add(this);

			// Change our coordinates
			if (direction == 1)
			{
				BottomPosition -= 4;
				Y--;
			}
			else if (direction == 2)
			{
				OffsetX -= 101;
				X++;
			}
			else if (direction == 3)
			{
				BottomPosition += 1;
				Y++;
			}
			else if (direction == 4)
			{
				OffsetX += 101;
				X--;
			}
		}
		#endregion

		#region Updating
		/// <remarks>
		/// This is the direction that the character will be jumping
		/// at the height of their apex.
		/// </summary>
		private int direction;

		/// <summary>
		/// Internal function to determine if the target is valid or
		/// not.
		/// </summary>
		private bool IsInvalidTarget(BlockStack srcStack, BlockStack destStack)
		{
			// Remove ourselves to take ourselves out of the calculation
			srcStack.Remove(this);

			try
			{
				// Make sure the destination is a ground block or an
				// immobile one, but nothing else
				Block dest = destStack.TopBlock;

				if (!Board.IsGroundBlock(dest) &&
					!Board.IsImmobileBlock(dest))
				{
					// Neither ground or immobile
					Debug("Invalid target: Block type: {0}", dest.Sprite.ID);
					return true;
				}

				// We need the distance to be 1 or less
				float dist =
					Math.Abs(srcStack.TopPosition - destStack.TopPosition);

				if (dist > 1)
				{
					// Too much to jump
					Debug("Invalid target: Height: {0}", dist);
					return true;
				}

				// It is good
				return false;
			}
			finally
			{
			   
				// Put ourselves back
				srcStack.Add(this);
			}
		}

		/// <summary>
		/// This function causes the prayer to jump and potentially
		/// move around the board.
		/// </summary>
		public void Jump()
		{
			// Reset the time
			timeUntilBounce = Entropy.NextDouble(
				Constants.MinimumCharacterBounce,
				Constants.MaximumCharacterBounce);

			// We want the characters to bounce around a bit, so
			// we randomly pick a direction to jump into. We get a
			// random number between 0 and 4:
			//   0 no change
			//   1 north
			//   2 east
			//   3 south
			//   4 west
			// Actually, 0 happens because of fallback
			direction = Entropy.Next(4) + 1;

			// East/West only
			// TODO Fix
			if (direction == 1 || direction == 3)
				direction++;

			// Do sanity checking on the bounds of the stage. If
			// we are on the end and we would have jumped off, set
			// the direction to "none"
			Board board = Game.State.Board;

			if (direction == 1 && Y == 0)
				direction = 0;
			else if (direction == 2 && X == board.Columns - 1)
				direction = 0;
			else if (direction == 3 && Y == board.Rows - 1)
				direction = 0;
			else if (direction == 4 && X == 0)
				direction = 0;

			// If we still non-zero, then get the appropriate
			// stacks
			BlockStack myStack = board[X, Y];
			BlockStack destStack = myStack;

			if (direction > 0)
			{
				// Get the proper stack and make sure it is a
				// valid target
				if (direction == 1)
					destStack = board[X, Y - 1];
				else if (direction == 2)
					destStack = board[X + 1, Y];
				else if (direction == 3)
					destStack = board[X, Y + 1];
				else if (direction == 4)
					destStack = board[X - 1, Y];

				// We don't jump if the destination block isn't a
				// ground or immobile block. We also don't jump if
				// the difference in height is more than one.
				if (IsInvalidTarget(myStack, destStack))
				{
					direction = 0;
					destStack = myStack;
				}
			}

			// Figure out the vector, we need a higher one if we
			// are jumping north and/or if the destination block
			// is higher.
			myStack.Remove(this);
			float myTop = myStack.TopPosition;
			float destTop = destStack.TopPosition;
			float vector = Constants.PrayerBounceVector;
			myStack.Add(this);

			if (direction == 1)
				vector += Constants.PrayerBounceNorthVector;

			if (destTop > myTop)
				vector += Constants.PrayerBounceUpVector;

			// Add a positive vector
			Vector += vector;
		}

		/// <summary>
		/// Processes the updating of the block, both in random
		/// jumping and also in moving into proper position.
		/// </summary>
		public void Update(UpdateArgs args)
		{
			// If we are moving, we want to get into position
			if (IsMoving)
			{
				// Get the movement rates
				float rate = (float) args.SecondsSinceLastUpdate;
				float ewRate = Constants.PrayerEastWestAdjust * rate;

				// The direction is important. When we are moving up,
				// we are heading toward the edge of something. If we
				// are going down, then we are aiming for the center
				// of the block.
				if (Vector > 0)
				{
					// If we are doing an in-place jump (direction 0),
					// we don't do anything
					if (direction == 0)
						return;

					// Check our direction
					if (direction == 1)
					{
					}
					else if (direction == 2)
					{
						OffsetX += ewRate;

						if (OffsetX > 0)
							OffsetX = 0;
					}
					else if (direction == 3)
					{
					}
					else if (direction == 4)
					{
						OffsetX -= ewRate;

						if (OffsetX < 0)
							OffsetX = 0;
					}
				}
				else
				{
					// If we are doing an in-place jump (direction 0),
					// we don't do anything
					if (direction == 0)
						return;

					// Check our direction
					if (direction == 1)
					{
					}
					else if (direction == 2)
					{
						OffsetX += ewRate;

						if (OffsetX < -101)
							OffsetX = -101;
					}
					else if (direction == 3)
					{
					}
					else if (direction == 4)
					{
						OffsetX -= ewRate;

						if (OffsetX > 101)
							OffsetX = 101;
					}
				}
			}
			else
			{
				// Clear the offsets
				OffsetX = OffsetY = 0;

				// Decrement the time
				timeUntilBounce -= args.SecondsSinceLastUpdate;
				
				// If we are negative, bounce
				if (timeUntilBounce < 0)
					Jump();
			}
		}
		#endregion
    }
}
