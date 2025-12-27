namespace GameFish;

[Icon( "pedal_bike" )]
public abstract partial class Vehicle : DynamicEntity
{
	protected const int VEHICLE_ORDER = DEFAULT_ORDER - 1000;
	protected const int SEATS_ORDER = VEHICLE_ORDER + 50;

	protected override bool? IsNetworkedOverride => true;
	protected override bool IsNetworkedAutomatically => true;

	[Sync( SyncFlags.FromHost )]
	public NetDictionary<Seat, bool> Seats { get; set; }

	[Sync]
	public float InputAcceleration { get; set; }

	[Sync]
	public float InputSteering { get; set; }

	protected override void OnEnabled()
	{
		Tags?.Add( TAG_VEHICLE );

		if ( Networking.IsHost && this.InGame() )
		{
			(Seats ??= [])?.Clear(); // fuck you. bitch. pussy.

			if ( Seats is not null )
			{
				var childSeats = Components.GetAll<Seat>();

				foreach ( var seat in childSeats )
					TryAddSeat( seat );
			}
		}

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

	public virtual bool TryAddSeat( Seat seat )
	{
		if ( !Networking.IsHost || !seat.IsValid() )
			return false;

		Seats ??= [];
		Seats[seat] = seat.IsDriver;

		return true;
	}

	/// <summary>
	/// Called to set how the vehicle is being controlled.
	/// </summary>
	protected virtual void UpdateInput( in float deltaTime )
	{
		var driverSeat = Seats?
			.Select( kv => kv.Key )
			.Where( seat => seat.IsValid() && seat.IsOccupied )
			.FirstOrDefault( seat => IsDriver( seat, seat.Sitter ) );

		var driver = driverSeat?.Sitter;

		if ( !driver.IsValid() || driver.Owner is not Client cl )
		{
			InputAcceleration = 0f;
			InputSteering = 0f;
			return;
		}

		InputAcceleration = cl.InputForward;
		InputSteering = cl.InputHorizontal;
	}

	public virtual bool IsDriver( Seat seat, Pawn pawn )
	{
		if ( !seat.IsValid() || !seat.IsDriver )
			return false;

		return pawn.IsValid() && pawn.IsAlive;
	}

	/// <summary>
	/// Called by the occupant of a seat.
	/// </summary>
	public virtual void Simulate( Seat seat, Pawn sitter, in float deltaTime, in bool isFixedUpdate )
	{
	}

	protected abstract void ApplyForces( in float deltaTime, in bool isFixedUpdate );
}
