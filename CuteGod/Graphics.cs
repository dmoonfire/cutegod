using BooGame;

namespace CuteGod
{
	/// <summary>
	/// Implements a proxy interface to the backend that switched
	/// between FreeGlut and SDL as appropriate.
	/// </summary>
	public class Graphics
	{
		#region Constructors
		/// <summary>
		/// Constructs the backend using environment variables and
		/// settings to determine the best fit.
		/// </summary>
		public Graphics()
		{
			// Set the backend
            if (System.Environment.OSVersion.Platform == System.PlatformID.Unix)
            {
                // SDL seems to work better under Unix
                backend = new BooGame.Sdl.SdlBackend();
            }
            else
            {
                // FreeGlut has been behaving better under Windows.
                backend = new BooGame.FreeGlut.FreeGlutBackend();
            }
		}
		#endregion

		#region Graphics
		private IBackend backend;

		/// <summary>
		/// Contains the BooGame backend to use.
		/// </summary>
		public IBackend Backend
		{
			get { return backend; }
		}
		#endregion
	}
}
