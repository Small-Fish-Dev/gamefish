namespace GameFish;

[Icon( "pedal_bike" )]
public abstract partial class Vehicle : DynamicEntity
{
	protected const int VEHICLE_ORDER = DEFAULT_ORDER - 1000;
	protected const int SEATS_ORDER = VEHICLE_ORDER + 50;

	protected override bool? IsNetworkedOverride => true;
	protected override bool IsNetworkedAutomatically => true;

	[Sync( SyncFlags.FromHost )]
	public NetList<Seat> Seats { get; set; }

	[Sync]
	public float InputAcceleration { get; set; }

	[Sync]
	public float InputSteering { get; set; }

	protected override void OnEnabled()
	{
		Tags?.Add( TAG_VEHICLE );

		FindSeats();

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
	/// Looks for seats this belongs to.
	/// </summary>
	public virtual void FindSeats()
	{
		if ( Networking.IsHost && this.InGame() )
			return;

		Seats ??= [];

		if ( Seats is null )
			return;

		ValidateSeats();

		var childSeats = Components.GetAll<Seat>();

		foreach ( var seat in childSeats )
			TryAddSeat( seat, autoValidate: false );
	}

	/// <returns> The actively registered seats(or null). </returns>
	public virtual IEnumerable<Seat> GetSeats()
		=> Seats?.Where( seat => seat.IsValid() );

	/// <returns> The actively registered driver seats(or null). </returns>
	public virtual IEnumerable<Seat> GetDriverSeats()
		=> GetSeats()?.Where( seat => seat?.IsDriving is true );

	public virtual bool TryAddSeat( Seat seat, bool autoValidate = true )
	{
		if ( !Networking.IsHost )
			return false;

		if ( !seat.IsValid() || !GameObject.IsValid() )
			return false;

		Seats ??= [];

		if ( !Seats.Contains( seat ) )
			Seats.Add( seat );

		seat.Vehicle = this;

		if ( autoValidate )
			ValidateSeats();

		return true;
	}

	/// <summary>
	/// Cleans up references towards invalid/unowned seats.
	/// </summary>
	protected virtual void ValidateSeats()
	{
		if ( !Networking.IsHost || !GameObject.IsValid() )
			return;

		if ( Seats is null || Seats.Count == 0 )
			return;

		var badSeats = Seats.Where( seat => !seat.IsValid() || seat.Vehicle != this );

		if ( badSeats.Any() )
		{
			foreach ( var seat in badSeats.ToArray() )
				Seats.Remove( seat );
		}
	}

	/// <returns> If the pawn is able to drive in this seat. </returns>
	public virtual bool CanDrive( Pawn pawn, Seat seat )
	{
		if ( !seat.IsValid() || !seat.IsDriving || seat.Vehicle != this )
			return false;

		return pawn.IsValid() && pawn.IsAlive;
	}

	/// <summary>
	/// Called to set how the vehicle is being controlled.
	/// </summary>
	protected virtual void UpdateInput( in float deltaTime )
	{
		var driverSeat = GetDriverSeats()
			.Where( seat => seat.IsValid() && seat.IsOccupied )
			.FirstOrDefault( seat => CanDrive( seat.Sitter, seat ) );

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

	/// <summary>
	/// Called by the occupant of a seat.
	/// </summary>
	public virtual void Simulate( Seat seat, Pawn sitter, in float deltaTime, in bool isFixedUpdate )
	{
	}

	protected abstract void ApplyForces( in float deltaTime, in bool isFixedUpdate );
}
