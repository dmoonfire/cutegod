using System;
using System.ComponentModel;

namespace CuteGod.Play
{
	/// <summary>
	/// Defines the types of bonuses that can come out of chests.
	/// </summary>
    public enum ChestBonusType
    {
		ExtraHearts,
		GrabStack, // +1 grab stack

		RestoreGrass, // Restores all top-facing blocks
		RestoreDirt,
		RestoreWater,

		RemoveDeadGrass, // Halves the ratio difference
		RemoveDeadDirt,
		RemoveDeadWater,

		NoClock,    // Removes the clock
		DoubleTime, // Doubles the time for the round, if over 60 stop
        GrabCost,   // Halves grab cost

		BugKiller, // Remove a round of bugs

		/// <remarks>
		/// This is used by the random selector to get a random event.
		/// </summary>
		Maximum
    }
}
