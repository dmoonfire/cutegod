using MfGames.Sprite3;
using MfGames.Sprite3.Backends;
using MfGames.Utility;

using System;
using System.Drawing;

namespace CuteGod.Play
{
	/// <summary>
	/// Represents a low-profile structure that identifies a single
	/// swarm partical, such as a heart or star, that is in the
	/// process of moving into the right place.
	public class SwarmParticle
	{
		#region Constructors
		public SwarmParticle()
		{
			// Keep track of our life
			secondsRemaining = Constants.MaxSwarmParticleLife;

			// Randomize the vectors in any direction
			VectorX = Entropy.NextFloat(
				-Constants.InitialSwarmSpeed,
				Constants.InitialSwarmSpeed);
			VectorY = Entropy.NextFloat(
				-Constants.InitialSwarmSpeed,
				Constants.InitialSwarmSpeed);
		}
		#endregion

		#region Properties
		public float CurrentX, CurrentY;
		public float TargetX, TargetY;
		public double secondsRemaining;

		/// <remarks>
		/// Vectors are mesured pixels/second.
		/// </remarks>
		public float VectorX, VectorY;
		#endregion

		#region Drawing
		/// <summary>
		/// Draws out the particle with the given drawable at the
		/// particle's position.
		/// </summary>
		public void Draw(IDrawable drawable, DrawingArgs args)
		{
			double ratio = secondsRemaining / Constants.MaxSwarmParticleLife;
			Color c = Color.FromArgb((int) (ratio * 255.0), Color.White);
			PointF p = new PointF(CurrentX, CurrentY);
			drawable.Draw(p, c, args.BackendDrawingArgs);
		}
		#endregion

		#region Updating
		private bool particleDied = false;

		/// <summary>
		/// Contains true if this particle has been killed.
		/// </summary>
		public bool Died
		{
			get { return particleDied; }
		}

		/// <summary>
		/// Updates the position of the swarm.
		/// </summary>
		public void Update(UpdateArgs args)
		{
			// Reduces the seconds
			float seconds = (float) args.SecondsSinceLastUpdate;
			secondsRemaining -= seconds;

			// Add the gravity
			float dx = TargetX - CurrentX;
			float dy = TargetY - CurrentY;

			// Get the distance to our target
			double distance = Math.Sqrt((dx * dx) + (dy * dy));

			// If we are within range, then just make it disappear
			if (distance < Constants.SwarmTargetRadius ||
				secondsRemaining < 0)
			{
				particleDied = true;
				return;
			}

			// Change the location
			CurrentX += VectorX * seconds;
			CurrentY += VectorY * seconds;

			// Figure out the vector to the target. We slow down the
			// current vector slightly to let gravity take effect,
			// then we include a vector directly toward the target,
			// with the given strength.
			double strength = Constants.SwarmTargetGravity * seconds;
			double angle = Math.Atan2(dy, dx);
			double cos = Math.Cos(angle) * strength;
			double sin = Math.Sin(angle) * strength;

			VectorX /= 1.01f;
			VectorY /= 1.01f;
			VectorX += (float) cos;
			VectorY += (float) sin;
		}
		#endregion
	}
}
