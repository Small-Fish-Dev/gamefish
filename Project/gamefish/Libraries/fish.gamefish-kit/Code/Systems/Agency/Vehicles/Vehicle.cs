namespace GameFish;

[Icon( "pedal_bike" )]
public class Vehicle : Module
{
	protected const int VEHICLE_ORDER = DEFAULT_ORDER - 1000;
	protected const int SEATS_ORDER = VEHICLE_ORDER + 50;

	public override bool IsParent( ModuleEntity comp )
		=> comp is DynamicEntity;

	/// <summary>
	/// The designated driver seat.
	/// </summary>
	[Property]
	[Title( "Driving" )]
	[Sync( SyncFlags.FromHost )]
	[Feature( VEHICLE ), Group( SEATS ), Order( SEATS_ORDER )]
	public Seat DriverSeat { get; set; }

	public virtual IEnumerable<Seat> GetSeats()
		=> GetModules<Seat>();

	public virtual void SitterSimulate( Pawn sitter, in float deltaTime, in bool isFixedUpdate )
	{
		UpdateInput( in deltaTime, isDriver: true );
	}

	public virtual void UpdateInput( in float deltaTime, in bool isDriver )
	{
	}

	public virtual bool IsDriver( Pawn pawn )
	{
		if ( !pawn.IsValid() || !pawn.Seat.IsValid() )
			return false;

		if ( !pawn.Seat.Vehicle.IsValid() )
			return false;

		return pawn.Seat.Vehicle == this;
	}

	public virtual Pawn GetDriver()
		=> DriverSeat?.Sitter;
}
