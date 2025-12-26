using Microsoft.VisualBasic;

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

	[Sync]
	public float InputAcceleration { get; set; }

	[Sync]
	public float InputSteering { get; set; }

	protected override void OnEnabled()
	{
		Tags?.Add( TAG_VEHICLE );

		base.OnEnabled();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( IsProxy )
			return;

		UpdateInput( Time.Delta );
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		if ( IsProxy )
			return;

		ApplyForces( Time.Delta, isFixedUpdate: true );
	}

	/// <summary>
	/// Called to set how the vehicle is being controlled.
	/// </summary>
	protected virtual void UpdateInput( in float deltaTime )
	{
		var driver = GetDriver();

		if ( !driver.IsValid() || driver.Owner is not Client cl )
		{
			InputAcceleration = 0f;
			InputSteering = 0f;

			return;
		}

		InputAcceleration = cl.VehicleInput.Acceleration;
		InputSteering = cl.VehicleInput.Steering;
	}

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
	/// Called by the occupant of a seat.
	/// </summary>
	public virtual void Simulate( Seat seat, Pawn sitter, in float deltaTime, in bool isFixedUpdate )
	{
	}

	protected abstract void ApplyForces( in float deltaTime, in bool isFixedUpdate );
}
