using System.Text.Json.Serialization;

namespace GameFish;

partial class Pawn
{
	/// <summary>
	/// The thing we're currently focusing on.
	/// </summary>
	[Title( "Focus" )]
	[Property, JsonIgnore]
	[Feature( PAWN ), Group( VEHICLE )]
	protected Seat InspectorSeat
	{
		get => Seat;
		set => Seat = value;
	}

	[Sync]
	public Seat Seat { get; set; }

	public virtual void OnSeatMoved( Seat seat )
	{
		if ( IsProxy || !Seat.IsValid() )
			return;

		// TEMP!
		WorldPosition = seat.WorldPosition;
		WorldRotation = seat.WorldRotation;
	}
}
