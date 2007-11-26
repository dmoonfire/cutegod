namespace CuteGod
{
	/// <summary>
	/// Defines the common interface for all asset loading delegates.
	/// </summary>
	public interface IAssetLoader
	{
		/// <summary>
		/// Triggers the asset loader to load the asset into memory
		/// and perform the needed registration to install it into the
		/// system.
		/// </summary>
		void Load();
	}
}
