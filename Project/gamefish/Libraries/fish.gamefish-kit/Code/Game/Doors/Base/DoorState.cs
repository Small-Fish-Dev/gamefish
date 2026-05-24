namespace GameFish;

[DefaultValue( Closed )]
public enum DoorState
{
	/// <summary>
	/// Not yet defined.
	/// </summary>
	[Hide]
	[Icon( "⚠" )]
	Unset = 0,

	/// <summary>
	/// Fully open.
	/// </summary>
	[Icon( "📭" )]
	Opened,

	/// <summary>
	/// Actively opening.
	/// </summary>
	[Icon( "💌" )]
	Opening,

	/// <summary>
	/// Fully closed.
	/// </summary>
	[Icon( "🚪" )]
	Closed,

	/// <summary>
	/// Actively closing.
	/// </summary>
	[Icon( "📩" )]
	Closing,
}

partial class Library
{
	/// <returns> If this state is related to a door opening. </returns>
	public static bool IsOpening( this DoorState state )
		=> state is DoorState.Opened or DoorState.Opening;

	/// <returns> If this state is related to a door closing. </returns>
	public static bool IsClosing( this DoorState state )
		=> state is DoorState.Closed or DoorState.Closing;

	/// <returns> If opposite of that state. </returns>
	public static DoorState Reverse( this DoorState state )
	{
		if ( state is DoorState.Opened or DoorState.Opening )
			return DoorState.Closing;

		if ( state is DoorState.Closed or DoorState.Closing )
			return DoorState.Opening;

		return DoorState.Unset;
	}
}
