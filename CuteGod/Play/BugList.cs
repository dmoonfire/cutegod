using C5;

using MfGames.Sprite3;

using System;

namespace CuteGod.Play
{
    /// <summary>
    /// Contains the list of bugs in the system.
    /// </summary>
    public class BugList
        : LinkedList<Bug>
    {
		#region Updating
		public void Update(UpdateArgs args)
		{
			foreach (Bug p in this)
				p.Update(args);
		}
		#endregion
    }
}
