using C5;
using CuteGod.Play;
using MfGames.Sprite3.PlanetCute;
using MfGames.Utility;
using System;
using System.IO;
using System.Xml;

using Tao.Sdl;

namespace CuteGod
{
    /// <summary>
    /// This class handles all the elements needed to play sounds
    /// during the game.
    /// </summary>
    public class SoundManager
	: Logable
    {
        #region Constants
        private static readonly int MaximumSoundsChunks = 30;
        #endregion

        #region Initialization
        private HashDictionary<string, SoundCategory> categories =
            new HashDictionary<string, SoundCategory>();
        private SdlMixer.MusicFinishedDelegate musicStopped;
        private SdlMixer.ChannelFinishedDelegate channelStopped;

        /// <summary>
        /// Sets up the sound system and loads in the music controls. This
        /// does use the full library for Tao, but only the mixer functions
        /// are used.
        /// </summary>
        public void Initialize()
        {
            // Set up the SDL sound
            SdlMixer.Mix_OpenAudio(
                SdlMixer.MIX_DEFAULT_FREQUENCY,
                (short) SdlMixer.MIX_DEFAULT_FORMAT,
                2,
                1024);

            // Allocate channels
            SdlMixer.Mix_AllocateChannels(MaximumSoundsChunks);

            // Default volumnes
			int vol = Game.Config.GetInt(Constants.ConfigMusicVolume, 75);
            SdlMixer.Mix_VolumeMusic(vol);

            // Hook up the events
            musicStopped = new SdlMixer.MusicFinishedDelegate(OnMusicEnded);
            SdlMixer.Mix_HookMusicFinished(musicStopped);

            channelStopped =
				new SdlMixer.ChannelFinishedDelegate(OnChannelEnded);
            SdlMixer.Mix_ChannelFinished(channelStopped);
        }

		/// <summary>
		/// Stops the music processing.
		/// </summary>
		public void Stop()
		{
			// Kill all music
			SdlMixer.Mix_HaltMusic();
			SdlMixer.Mix_HaltChannel(-1);
			SdlMixer.Mix_CloseAudio();
		}
        #endregion

		#region Registering
		/// <summary>
		/// Registers a single sound in a given category, creating it
		/// if needed.
		/// </summary>
		public void Register(string category, FileInfo soundFile)
		{
			// Create the category if we don't have it, otherwise load
			// it into a variable
			SoundCategory sc = null;

			if (!categories.Contains(category))
			{
				sc = new SoundCategory();
				sc.Name = category;
				categories[category] = sc;
			}
			else
			{
				sc = categories[category];
			}

			// Add this file
			sc.Add(soundFile);
		}
		#endregion

        #region Playing Music
        private IntPtr musicChunk;
        private IntPtr [] soundChunks = new IntPtr[MaximumSoundsChunks];
        private int soundIndex = 0;

        /// <summary>
        /// Stops the currently played music and triggers a new one.
        /// </summary>
        /// <param name="category"></param>
        private void InterruptMusic(string category)
        {
            // Stop the music
            SdlMixer.Mix_HaltMusic();
            PlayMusic(category);
            SdlMixer.Mix_ResumeMusic();
        }

        /// <summary>
        /// Triggers when a channel is finished.
        /// </summary>
        /// <param name="channel"></param>
        private void OnChannelEnded(int channel)
        {
            try
            {
                SdlMixer.Mix_FreeChunk(soundChunks[channel]);
            }
            catch
            {
                Console.WriteLine("Cannot free sound chunk");
            }
        }

        /// <summary>
        /// Triggered when the background music has stopped processing.
        /// </summary>
        private void OnMusicEnded()
        {
            // Unallocate our music
            SdlMixer.Mix_FreeMusic(musicChunk);

            // Start up some new music
            PlayMusic("Background");
        }

        /// <summary>
        /// Internal function that processes the play request as a music
        /// file and not a sound element.
        /// </summary>
        /// <param name="key"></param>
        private void PlayMusic(string key)
        {
            // See if we have the category
            if (!categories.Contains(key))
			{
                // No music, no effect
				Debug("No music for {0}", key);
                return;
			}

            // If the category is empty, remove it
            if (categories[key].Count == 0)
            {
                // Remove the category and return
                categories.Remove(key);
                return;
            }

            // Get the random song key
			FileInfo file = categories[key].GetRandomSound();
            string sound = file.FullName;
			Debug("Playing music: {0}", sound);
            musicChunk = SdlMixer.Mix_LoadMUS(sound);

            // Start playing the music
			int result = -1;

			try
			{
				result = SdlMixer.Mix_PlayMusic(musicChunk, 1);
			}
			catch (Exception e)
			{
				Error("Can't play music: {0}", e.Message);
			}
				
			if (result == -1)
			{
                // Remove the chunk
                SdlMixer.Mix_FreeMusic(musicChunk);
                categories[key].Remove(file);
				
                // Find a new one
                PlayMusic(key);
            }
        }

        /// <summary>
        /// Triggers a sound to be played.
        /// </summary>
        /// <param name="key"></param>
        private void PlaySound(string key)
        {
            // See if we have the category
            if (!categories.Contains(key))
			{
                // No music, no effect
				Debug("No sounds for {0}", key);
                return;
			}

            // If the category is empty, remove it
            if (categories[key].Count == 0)
            {
                // Remove the category and return
                categories.Remove(key);
                return;
            }

            // Get the random song key
			FileInfo file = categories[key].GetRandomSound();
            string sound = file.FullName;
			Debug("Playing sound: {0}", sound);
            IntPtr soundChunk = SdlMixer.Mix_LoadWAV(sound);

            // Start playing the music
			int result = -1;

			try
			{
				result = SdlMixer.Mix_PlayChannel(soundIndex, soundChunk, 0);
			}
			catch (Exception e)
			{
				Error("Can't play sound: {0}", e.Message);
			}

            if (result == -1)
            {
                // Remove the chunk
                SdlMixer.Mix_FreeChunk(soundChunk);
                categories[key].Remove(file);

                // Find a new one
                PlaySound(key);
            }

            // Save the chunk
            soundChunks[soundIndex] = soundChunk;
            soundIndex = (soundIndex + 1) % MaximumSoundsChunks;
        }
        #endregion

        #region Other Events
        /// <summary>
        /// Triggered when a chest is opened
        /// </summary>
        /// <param name="prayer"></param>
        public void ChestOpened()
        {
			PlaySound("Chest Opened");
        }

        /// <summary>
        /// Triggered when a prayer is completed.
        /// </summary>
        /// <param name="prayer"></param>
        public void PrayerCompleted(Prayer prayer)
        {
            InterruptMusic("Prayer Completed");
        }

		/// <summary>
		/// Triggered when the background music needs to be started.
		/// </summary>
		public void StartBackground()
		{
			PlayMusic("Background");
		}
        #endregion

        #region Block Events
        /// <summary>
        /// This triggers the sound for when a block is grabbed from
        /// the ground.
        /// </summary>
        /// <param name="stack"></param>
        /// <param name="block"></param>
        public void BlockGrabbed(object sender, BlockStackEventArgs args)
        {
            PlaySound("Block Grabbed");
        }

        /// <summary>
        /// This triggers the sounds for when the block lands on the ground.
        /// </summary>
        /// <param name="stack"></param>
        /// <param name="block"></param>
        public void BlockLanded(object sender, BlockStackEventArgs args)
        {
			// Ignore blanks
			if (args.Block == null ||
				args.Block.Sprite == null ||
				args.Block.Sprite.ID == null)
				return;

			// Make sure we don't have a character
			if (args.Block.Sprite.ID.Contains("Character"))
				return;

			// Play a sound
			if (Board.IsGroundBlock(args.Block))
				PlaySound("Block Landed");
            /*
                String.Format("{0} - {1}", args.Block.DrawableName),
                String.Format("{0} - {1} - {2}", args.Block.DrawableName));
             */
        }
        #endregion
    }
}
