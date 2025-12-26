namespace GameFish;

[Icon( "pedal_bike" )]
public abstract partial class Vehicle : DynamicEntity
{
	protected const int VEHICLE_ORDER = DEFAULT_ORDER - 1000;
	protected const int SEATS_ORDER = VEHICLE_ORDER + 50;

	protected override bool? IsNetworkedOverride => true;
	protected override bool IsNetworkedAutomatically => true;

	/// <summary>
	/// The designated driver seat.
	/// </summary>
	[Property]
	[Sync( SyncFlags.FromHost )]
	[Feature( VEHICLE ), Group( SEATS ), Order( SEATS_ORDER )]
	public Seat DriverSeat { get; set; }

	protected override void OnEnabled()
	{
		Tags?.Add( TAG_VEHICLE );

		base.OnEnabled();
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		if ( IsProxy )
			return;

		ApplyForces( Time.Delta, isFixedUpdate: true );
	}

	protected abstract void ApplyForces( in float deltaTime, in bool isFixedUpdate );

	public virtual IEnumerable<Seat> GetSeats()
		=> GetModules<Seat>();

	public virtual bool IsDriver( Seat seat, Pawn pawn )
	{
		if ( !seat.IsValid() || !pawn.IsValid() )
			return false;

		return seat == DriverSeat;
	}

	public virtual Pawn GetDriver()
		=> DriverSeat?.Sitter;

	/// <summary>
	/// Called by the occupant of a seat for input and such.
	/// </summary>
	public virtual void Simulate( Seat seat, Pawn sitter, in float deltaTime, in bool isFixedUpdate )
	{
		if ( !seat.IsValid() || !sitter.IsValid() )
			return;

		if ( !sitter.IsAlive )
			return;

		if ( IsDriver( seat, sitter ) )
			DriverSimulate( seat, sitter, in deltaTime, in isFixedUpdate );
	}

	public virtual void DriverSimulate( Seat seat, Pawn sitter, in float deltaTime, in bool isFixedUpdate )
	{
		UpdateInput( seat, sitter, in deltaTime, in isFixedUpdate );
	}

	public virtual void UpdateInput( Seat seat, Pawn sitter, in float deltaTime, in bool isFixedUpdate )
	{
	}
}
