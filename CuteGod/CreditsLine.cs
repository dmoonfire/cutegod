using MfGames.Sprite3;
using MfGames.Utility;
using System;
using System.Drawing;

namespace CuteGod
{
    public class CreditsLine
    : Logable
    {
        #region Constants
        public const float FontSize = 50;
        public const float OffsetX = 20;
        public const double OffsetS = 2;
        public const double OffsetSH = 1;
        #endregion

        #region Constructors
        public CreditsLine()
        {
            // Set up the sprites
            Sprite = new TextDrawableSprite(Game.GetFont(FontSize));
            Sprite.Color = Color.White;
            Sprite.Alignment = ContentAlignment.TopLeft;
        }
        #endregion

        #region Properties
        private int assets;
        private double displayed = 0;

        public int Assets
        {
            get { return assets; }
            set
            {
                assets = value;
                SecondsToLive = Math.Log(assets) + OffsetS;
            }
        }

        public string Name
        {
            get { return Sprite.Text; }
            set { Sprite.Text = value; }
        }

        public double SecondsToLive;

        public double SecondsDisplayed
        {
            get { return displayed; }
            set
            {
                // Set the value
                displayed = value;

                // Figure out the fade in
                if (displayed < OffsetSH)
                {
                    // Figure out the color
                    double fadeIn = displayed / OffsetSH;
                    Sprite.Color =
                        Color.FromArgb((int) (fadeIn * 255), Color.White);

                    // Figure out the position
                    Sprite.Point = new PointF(
                        Point.X - OffsetX + OffsetX * (float) fadeIn,
                        Point.Y);
                }
                else if (displayed > SecondsToLive)
                {
                    // We shouldn't get here
                }
                else if (displayed > SecondsToLive - OffsetSH)
                {
                    // Figure out the color
                    double fadeOut =
                        Math.Min(1,
                            Math.Abs(displayed - (SecondsToLive - OffsetSH)));
                    fadeOut = 1 - (fadeOut / OffsetSH);
                    Sprite.Color =
                        Color.FromArgb((int) (fadeOut * 255), Color.White);

                    // Figure out the position	
                    Sprite.Point = new PointF(
                        Point.X + OffsetX * (float) (1 - fadeOut),
                        Point.Y);
                }
                else
                {
                    // Set the color
                    Sprite.Color = Color.White;

                    // Set the position
                    Sprite.Point = Point;
                }
            }
        }

        public PointF Point;
        public TextDrawableSprite Sprite;
        #endregion
    }
}
