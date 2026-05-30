using System;

namespace GameFish;

/// <summary>
/// The different ways something can interact with a trigger.
/// <br /> <br />
/// <b> NOTE: </b> Useful for contextual activation.
/// </summary>
[Flags]
[DefaultValue( Enter )]
public enum TriggerPhase
{
	/// <summary>
	/// Touched the trigger.
	/// </summary>
	Enter = 1 << 1,

	/// <summary>
	/// Left the trigger.
	/// </summary>
	Exit = 1 << 2,

	/// <summary>
	/// Actively inside of the trigger.
	/// </summary>
	Inside = 1 << 3,
}

partial class Library
{
	/// <returns> If entering the trigger is considered. </returns>
	public static bool HasEnter( this TriggerPhase phase )
		=> phase.HasFlag( TriggerPhase.Enter );

	/// <returns> If exiting the trigger is considered. </returns>
	public static bool HasExit( this TriggerPhase phase )
		=> phase.HasFlag( TriggerPhase.Exit );

	/// <returns> If being inside of the trigger is considered. </returns>
	public static bool HasInside( this TriggerPhase phase )
		=> phase.HasFlag( TriggerPhase.Inside );
}
