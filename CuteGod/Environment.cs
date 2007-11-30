using BooGame;
using BooGame.Video;
using C5;
using MfGames.Sprite3;
using MfGames.Sprite3.Backends;
using MfGames.Sprite3.PlanetCute;
using MfGames.Utility;
using System;
using System.Drawing;

namespace CuteGod
{
	/// <summary>
	/// Environment control for the game. This handles updating the
	/// background to show some moving elements and to switch around
	/// the day/night cycle.
	/// </summary>
	public class Environment
	: Logable
	{
		#region Constructors
		public Environment()
		{
			// Create the sprite viewports
			sprites = new SpriteViewport [4];

			for (int i = 0; i < 4; i++)
			{
				sprites[i] = new SpriteViewport();
				sprites[i].Point = PointF.Empty;
			}
		}
		#endregion

		#region Drawing
		private SizeF size;

		/// <summary>
		/// Contains the window size for this environment.
		/// </summary>
		public SizeF Size
		{
			get { return size; }
			set
			{
				// Ignore if the same
				if (size == value)
					return;

				// Save the size
				size = value;

				for (int i = 0; i < 4; i++)
					sprites[i].Size = value;

				// Reconstruct the stars
				sprites[0].Clear();

				for (int i = 0; i < 50; i++)
				{
					ISprite ds =
						AssetLoader.Instance.CreateSprite("Mini Star");
					ds.Visible = true;
					ds.Point = new PointF(
						Entropy.NextFloat(0, value.Width),
						Entropy.NextFloat(0, value.Height / 2));
					sprites[0].Add(ds);
				}
			}
		}

		/// <summary>
		/// Renders the stuff intended for the background.
		/// </summary>
		public void DrawBackground(DrawingArgs args)
		{
			// Draw the background sprites
			sprites[0].Draw(args);

			// Figure out the day colors
			if (day > 0)
			{
				int dayAlpha = (int) Math.Min(255, day * 255);
				Color day1 = Color.FromArgb(dayAlpha, Color.DarkBlue);
				Color day2 = Color.FromArgb(dayAlpha, Color.SkyBlue);
				
				args.Backend.DrawGradientRectangle(
					new RectangleF(PointF.Empty, size),
					day1, day1, day2, day2);
			}

			// Add the dusk
			if (dusk > 0)
			{
				int duskAlpha = (int) (dusk * 255 * 0.5);
				Color dusk1 = Color.FromArgb(duskAlpha, Color.Red);
				Color dusk2 = Color.FromArgb(duskAlpha, Color.Orange);
				
				args.Backend.DrawGradientRectangle(
					new RectangleF(PointF.Empty, size),
					dusk1, dusk1, dusk2, dusk2);
			}

			// Add the dawn
			if (dawn > 0)
			{
				int dawnAlpha = (int) (dawn * 255 * 0.5);
				Color dawn1 = Color.FromArgb(dawnAlpha, Color.Pink);
				Color dawn2 = Color.FromArgb(dawnAlpha, Color.Gray);
				
				args.Backend.DrawGradientRectangle(
					new RectangleF(PointF.Empty, size),
					dawn1, dawn1, dawn2, dawn2);
			}

			// Draw the background sprites
			sprites[1].Draw(args);
		}

		/// <summary>
		/// Renders the stuff intended for the foreground.
		/// </summary>
		public void DrawForeground(DrawingArgs args)
		{
			// Add the night tint
			if (night > 0)
			{
				int nightAlpha = (int) (night * 255 * 0.25);
				args.Backend.DrawFilledRectangle(
					new RectangleF(PointF.Empty, size),
					Color.FromArgb(nightAlpha, Color.Black));
			}

			// Add the dusk tint
			if (dusk > 0)
			{
				int duskAlpha = (int) (dusk * 255 * 0.25);
				args.Backend.DrawFilledRectangle(
					new RectangleF(PointF.Empty, size),
					Color.FromArgb(duskAlpha, Color.DarkOrange));
			}

			// Add the dawn tint
			if (dawn > 0)
			{
				int dawnAlpha = (int) (dawn * 255 * 0.25);
				args.Backend.DrawFilledRectangle(
					new RectangleF(PointF.Empty, size),
					Color.FromArgb(dawnAlpha, Color.Pink));
			}
		}
		#endregion

		#region Update
		private double time;
		private double dayInSeconds = 60 * 10; // 10 minute day/night cycles
		private double dawn, dusk, night, day;

		/// <summary>
		/// Updates the environment state.
		/// </summary>
		public void Update(UpdateArgs args)
		{
			// Figure out the time of day (0 is midnight, 0.5 is noon)
			double sec = ((double) DateTime.UtcNow.Ticks / 10000000.0)
				% dayInSeconds;
			time = sec / dayInSeconds;

			// Figure out the day and night cycles
			night = Math.Abs(time - 0.5) * 2;
			day = 1 - night;

			// Figure out dawn
			dawn = dusk = 0;

			if (time >= 0.2 && time <= 0.4)
			{
				// Dawn
				dawn = time - 0.2;
				dawn *= 5;
				dawn = 1 - Math.Abs(dawn - 0.5) * 2;
			}
			else if (time >= 0.6 && time <= 0.8)
			{
				// Dusk
				dusk = time - 0.6;
				dusk *= 5;
				dusk = 1 - Math.Abs(dusk - 0.5) * 2;
			}

			// Update the sprites
			for (int i = 0; i < 4; i++)
				sprites[i].Update(args);

			// Update the start alpha
			if (night > 0)
			{
				foreach (ISprite ds in sprites[0])
				{
					int alpha = ds.Tint.A;
					alpha += Entropy.Next(-3, 3);
					alpha = Math.Min(alpha, 128);
					alpha = Math.Max(alpha, 64);
					ds.Tint = Color.FromArgb(alpha, ds.Tint);
				}
			}
		}
		#endregion

		#region Sprites
		private SpriteViewport [] sprites;
		#endregion
	}
}
