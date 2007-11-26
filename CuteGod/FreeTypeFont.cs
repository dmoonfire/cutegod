#if REMOVED
using BooGame;
using BooGame.Video;

using MfGames.Sprite3.Backends;

using System;
using System.Drawing;
using System.Drawing.Text;
using System.Runtime.InteropServices;

using Tao.OpenGl;

namespace MfGames.Sprite3.BooGameBE
{
    /// <summary>
    /// Encapsulates the font-drawing routines for BooGame.
    /// </summary>
    public class FreeTypeFont
    : IBackendFont
    {
        #region Constants
        private static readonly int GlyphCount = 128;
        #endregion

        #region Static Setup
        private static bool isInitialized = false;
        private static IntPtr libptr;

        /// <summary>
        /// Sets up the internal FreeType structures.
        /// </summary>
        static FreeTypeFont()
        {
            // We begin by creating a library pointer
            int ret = Library.FT_Init_FreeType(out libptr);

            if (ret != 0)
                throw new Exception("Cannot load FreeType font structure!");

            // Mark the font subsystem is initalized
            isInitialized = true;
        }

        /// <summary>
        /// Returns the state of the font system setup.
        /// </summary>
        public static bool IsInitialized
        {
            get { return isInitialized; }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Loads a font into memory by the given name.
        /// </summary>
        public FreeTypeFont(string fontName, float size, FontStyle style)
        {
            //Once we have the library we create and load the font face
            int retb = Face.FT_New_Face(libptr, fontName, 0, out faceptr);

            if (retb != 0)
                throw new Exception("Cannot initialize the font");

            face = (Face) Marshal.PtrToStructure(faceptr, typeof(Face));

            //Freetype measures the font size in 1/64th of pixels for accuracy 
            //so we need to request characters in size*64
            int sz = fontSize = (int) size;
            Face.FT_Set_Char_Size(faceptr, sz << 6, sz << 6, 96, 96);

            //Provide a reasonably accurate estimate for expected pixel sizes
            //when we later on create the bitmaps for the font
            Face.FT_Set_Pixel_Sizes(faceptr, sz, sz);

            // Once we have the face loaded and sized we generate opengl textures 
            // from the glyphs  for each printable character
            textures = new int[GlyphCount];
            extent_x = new int[GlyphCount];
            glyphSizes = new SizeF[GlyphCount];
            list_base = Gl.glGenLists(GlyphCount);
            baselines = new int[GlyphCount];

            Gl.glGenTextures(GlyphCount, textures);

            for (int c = 0; c < GlyphCount; c++)
            {
                CompileCharacter(face, faceptr, c);
            }
        }
        #endregion

        #region Properties
        Face face;
        IntPtr faceptr;
        private int list_base;
        private int fontSize;
        private int[] textures;
        private int[] extent_x;
        private SizeF[] glyphSizes;
        private int[] baselines;
        #endregion

        #region FreeType Font Rendering
        /// <summary>
        /// Draws text out to the screen directly.
        /// </summary>
        public void DrawText(PointF point, Color color, string text)
        {
            // Some optimization tests
            if (text.Length == 0)
                return;

            if (color.A == 0)
                return;

            // Set up the colors
            // DREM Broken Gl.glColor4f(color.R, color.G, color.B, color.A);

            // Set up some variables and adjust the height because
            // OpenGL is lower-left instead of upper-right as we
            // normally expect it to be.
            float x = point.X;
            float y = point.Y + GetTextSize(text, false).Height;
            //float y = point.Y + GetTextBaseline(text) + GetTextSize(text).Height;
            //GetTextSize(text).Height + GetTextBaseline(text) - face.descender;
            int font = list_base;

            // Setup the GL matrix
            Gl.glPushMatrix();
            Gl.glTranslatef(x, y, 0f);

#if REMOVED
            //Prepare openGL for rendering the font characters
            push_scm();
            Gl.glPushAttrib(Gl.GL_LIST_BIT | Gl.GL_CURRENT_BIT | Gl.GL_ENABLE_BIT | Gl.GL_TRANSFORM_BIT);
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glDisable(Gl.GL_LIGHTING);
            Gl.glEnable(Gl.GL_TEXTURE_2D);
            Gl.glDisable(Gl.GL_DEPTH_TEST);
            Gl.glEnable(Gl.GL_BLEND);
            Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA);
#endif

            Gl.glListBase(font);
#if REMOVED
            float[] modelview_matrix = new float[16];
            Gl.glGetFloatv(Gl.GL_MODELVIEW_MATRIX, modelview_matrix);
            Gl.glPushMatrix();
            Gl.glLoadIdentity();
            Gl.glTranslatef(x, y, 0);
            Gl.glMultMatrixf(modelview_matrix);
#endif

            // Render the text using the given display lists
            byte[] textbytes = new byte[text.Length];

            for (int i = 0; i < text.Length; i++)
                textbytes[i] = (byte) text[i];

            Gl.glCallLists(text.Length, Gl.GL_UNSIGNED_BYTE, textbytes);
            textbytes = null;

            //Restore openGL state
            Gl.glPopMatrix();
        }

        /// <summary>
        /// Gets the baseline of a given string.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public int GetTextBaseline(string text)
        {
            // Create a basic size
            int baseline = 0;

            // Loop through the text
            foreach (char c in text)
            {
                // Add width to the size
                baseline = Math.Min(baseline, baselines[c]);
            }

            // Return the results
            return baseline;
        }

        /// <summary>
        /// Returns the size of the rendered text.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public SizeF GetTextSize(string text)
        {
            return GetTextSize(text, true);
        }

        private SizeF GetTextSize(string text, bool addBaseline)
        {
            // Create a basic size
            SizeF size = new SizeF();

            // Loop through the text
            foreach (char c in text)
            {
                // Add width to the size
                size.Width += glyphSizes[c].Width;
                size.Height = Math.Max(size.Height, glyphSizes[c].Height + baselines[c]);
            }

            // If we add the baseline, add the negative
            if (addBaseline)
                size.Height -= GetTextBaseline(text);

            // Return the results
            return size;
        }
        #endregion

        #region Display List Generation
        /// <summary>
        /// Sets up a single character as a OpenGL texture for rendering.
        /// </summary>
        /// <param name="face"></param>
        /// <param name="faceptr"></param>
        /// <param name="c"></param>
        private void CompileCharacter(Face face, IntPtr faceptr, int c)
        {
            //We first convert the number index to a character index
            int index = Face.FT_Get_Char_Index(faceptr, Convert.ToChar(c));

            //Here we load the actual glyph for the character
            int ret = Face.FT_Load_Glyph(faceptr, index, FT_LOAD_TYPES.FT_LOAD_DEFAULT);

            if (ret != 0)
                return;

            //Convert the glyph to a bitmap
            IntPtr glyph;
            int retb = Glyph.FT_Get_Glyph(face.glyphrec, out glyph);

            if (retb != 0)
                return;

            // Render the glphy to a bitmap
            Glyph.FT_Glyph_To_Bitmap(out glyph, FT_RENDER_MODES.FT_RENDER_MODE_NORMAL, 0, 1);
            BitmapGlyph glyph_bmp = (BitmapGlyph) Marshal.PtrToStructure(glyph, typeof(BitmapGlyph));
            int size = (glyph_bmp.bitmap.width * glyph_bmp.bitmap.rows);

            if (size <= 0)
            {
                //space is a special `blank` character
                extent_x[c] = 0;

                if (c == 32)
                {
                    Gl.glNewList((int) (list_base + c), Gl.GL_COMPILE);
                    Gl.glTranslatef(fontSize >> 1, 0, 0);
                    extent_x[c] = fontSize >> 1;
                    Gl.glEndList();
                }
                return;

            }

            // Allocate space and grab the bitmap
            byte[] bmp = new byte[size];
            Marshal.Copy(glyph_bmp.bitmap.buffer, bmp, 0, bmp.Length);

            // Save the size of the bitmap and create a power of 2 version
            // for OpenGL.
            glyphSizes[c] =
                new SizeF(glyph_bmp.bitmap.width,
                    glyph_bmp.bitmap.rows);
            baselines[c] = glyph_bmp.top - glyph_bmp.bitmap.rows;
            int width = next_po2(glyph_bmp.bitmap.width);
            int height = next_po2(glyph_bmp.bitmap.rows);

            byte[] expanded = new byte[2 * width * height];

            for (int j = 0; j < height; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    expanded[2 * (i + j * width)] = expanded[2 * (i + j * width) + 1] =
                        (i >= glyph_bmp.bitmap.width || j >= glyph_bmp.bitmap.rows) ?
                        (byte) 0 : bmp[i + glyph_bmp.bitmap.width * j];
                }
            }

            // Set up some texture parameters for opengl
            Gl.glBindTexture(Gl.GL_TEXTURE_2D, textures[c]);
            Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);
            Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR);

            // Create the texture
            Gl.glTexImage2D(Gl.GL_TEXTURE_2D, 0, Gl.GL_RGBA, width, height,
                0, Gl.GL_LUMINANCE_ALPHA, Gl.GL_UNSIGNED_BYTE, expanded);
            expanded = null;
            bmp = null;

            // Create a display list and bind a texture to it
            Gl.glNewList((int) (list_base + c), Gl.GL_COMPILE);
            Gl.glBindTexture(Gl.GL_TEXTURE_2D, textures[c]);

            //Account for freetype spacing rules
            Gl.glTranslatef(glyph_bmp.left, 0, 0);
            Gl.glPushMatrix();
            //Gl.glRotatef(180, 1f, 0, 0);
            Gl.glScalef(1, -1, 1);
            Gl.glTranslatef(0, glyph_bmp.top - glyph_bmp.bitmap.rows, 0);
            float x = (float) glyph_bmp.bitmap.width / (float) width;
            float y = (float) glyph_bmp.bitmap.rows / (float) height;

            //Draw the quad
            Gl.glBegin(Gl.GL_QUADS);
            {
                Gl.glTexCoord2d(0, 0);
                Gl.glVertex2f(0, glyph_bmp.bitmap.rows);
                Gl.glTexCoord2d(0, y);
                Gl.glVertex2f(0, 0);
                Gl.glTexCoord2d(x, y);
                Gl.glVertex2f(glyph_bmp.bitmap.width, 0);
                Gl.glTexCoord2d(x, 0);
                Gl.glVertex2f(glyph_bmp.bitmap.width, glyph_bmp.bitmap.rows);
            }
            Gl.glEnd();
            Gl.glPopMatrix();

            // Advance for the next character
            Gl.glTranslatef(glyph_bmp.bitmap.width, 0, 0);
            extent_x[c] = glyph_bmp.left + glyph_bmp.bitmap.width;
            Gl.glEndList();
        }
        #endregion

        #region Unintegrated Stuff
        public void Dispose()
        {
            Gl.glDeleteLists(list_base, GlyphCount);
            Gl.glDeleteTextures(GlyphCount, textures);
            textures = null;
            extent_x = null;
        }

        internal int next_po2(int a)
        {
            int rval = 1;
            while (rval < a)
                rval <<= 1;
            return rval;
        }

        internal void push_scm()
        {
            Gl.glPushAttrib(Gl.GL_TRANSFORM_BIT);
            int[] viewport = new int[4];
            Gl.glGetIntegerv(Gl.GL_VIEWPORT, viewport);
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glPushMatrix();
            Gl.glLoadIdentity();
            Gl.glOrtho(viewport[0], viewport[2], viewport[1], viewport[3], 0, 1);
            Gl.glPopAttrib();
            viewport = null;
        }

        internal void pop_pm()
        {
            Gl.glPushAttrib(Gl.GL_TRANSFORM_BIT);
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glPopMatrix();
            Gl.glPopAttrib();
        }

        public int extent(string what)
        {
            int ret = 0;
            for (int c = 0; c < what.Length; c++)
                ret += extent_x[what[c]];
            return ret;
        }
        #endregion
    }
}

#if INTEGRATE
using System;
using Tao.OpenGl;
using System.Runtime.InteropServices;
using FreeTypeWrap;

namespace FreeType
{

	// A true 3D Font 
	public class Font3D
	{

    	//Public members
    	private int list_base;
    	private int font_size;
    	private int[] textures;
    	private int[] extent_x;

	    public Font3D( string font, int size ) 
	    {
	    	
	    	// Save the size we need it later on when printing
	    	font_size=size;	    	

//REMOVED			
			
			
			// Dispose of these as we don't need
			Face.FT_Done_Face(faceptr);
	    	Library.FT_Done_FreeType(libptr);
	    }
	    
				
	}
	
}
#endif
#endif
