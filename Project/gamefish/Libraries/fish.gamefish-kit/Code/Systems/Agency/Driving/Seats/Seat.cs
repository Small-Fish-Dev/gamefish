using System.Text.Json.Serialization;
using Sandbox.Movement;

namespace GameFish;

[Icon( "chair" )]
public partial class Seat : DynamicEntity, IUsable, ISitTarget
{
	protected const int SEAT_ORDER = DEFAULT_ORDER - 1000;
	protected const int SEAT_DEBUG_ORDER = SEAT_ORDER - 10;

	protected const int VEHICLE_ORDER = SEAT_ORDER + 50;

	protected override bool? IsNetworkedOverride => true;
	protected override bool IsNetworkedAutomatically => true;

	[Title( "Sitter" )]
	[Property, JsonIgnore, ReadOnly]
	[Feature( SEAT ), Group( DEBUG ), Order( SEAT_DEBUG_ORDER )]
	protected Pawn InspectorSitter => Sitter;

	[Sync( SyncFlags.FromHost )]
	public Pawn Sitter
	{
		get => _sitter;
		protected set
		{
			if ( _sitter == value )
				return;

			var old = _sitter;
			_sitter = value;

			OnSetSitter( _sitter, old );
		}
	}

	protected Pawn _sitter;

	public bool IsOccupied => Sitter.IsValid() && Sitter.Seat == this;

	[Sync( SyncFlags.FromHost )]
	public Vehicle Vehicle
	{
		get => _vehicle;
		set
		{
			if ( _vehicle == value )
				return;

			var old = _vehicle;
			_vehicle = value;

			OnSetVehicle( _vehicle, old );
		}
	}

	protected Vehicle _vehicle;

	/// <summary>
	/// Is this a seat that lets you drive?
	/// </summary>
	[Property]
	[Sync( SyncFlags.FromHost )]
	[Feature( SEAT ), Group( VEHICLE ), Order( VEHICLE_ORDER )]
	public bool IsDriver { get; set; } = false;

	public virtual float GetUsablePriority( Pawn pawn )
	{
		if ( !pawn.IsValid() )
			return 0f;

		return Center.DistanceSquared( pawn.EyePosition );
	}

	protected override void OnEnabled()
	{
		Tags?.Add( TAG_SEAT );

		FindVehicle();

		base.OnEnabled();
	}

	protected override void OnStart()
	{
		base.OnStart();

		Transform.OnTransformChanged = OnMoved;
	}

	public virtual void FindVehicle()
	{
		if ( !Networking.IsHost )
			return;

		if ( !Vehicle.IsValid() )
			Vehicle = Components.Get<Vehicle>( FindMode.EnabledInSelf | FindMode.InAncestors );
	}

	protected virtual void OnSetSitter( Pawn newSitter, Pawn oldSitter )
	{
	}

	protected virtual void OnSetVehicle( Vehicle newVehicle, Vehicle oldVehicle )
	{
	}

	protected virtual void OnMoved()
	{
		if ( Sitter.IsValid() )
			Sitter.OnSeatMoved( this );
	}

	public virtual void Simulate( in float deltaTime, in bool isFixedUpdate )
	{
		if ( Vehicle.IsValid() )
			Vehicle.Simulate( this, Sitter, in deltaTime, in isFixedUpdate );
	}

	public virtual bool CanEnter( Pawn pawn )
		=> !IsOccupied;

	public virtual bool CanExit( Pawn pawn )
		=> true;

	public virtual bool TryRequestEnter( Pawn pawn )
	{
		if ( !CanEnter( pawn ) )
			return false;

		RpcRequestEnter();
		return true;
	}

	public virtual bool TryRequestExit( Pawn pawn )
	{
		if ( !CanExit( pawn ) )
			return false;

		RpcRequestExit();
		return true;
	}

	public bool IsUsable( Pawn pawn )
		=> pawn.IsValid() && pawn.IsAlive;

	[Rpc.Host]
	public void RpcUse()
	{
		if ( Server.TryFindPawn( Rpc.Caller, out var pawn ) )
			OnRequestUse( pawn );
	}

	protected virtual void OnRequestUse( Pawn pawn )
	{
		if ( IsUsable( pawn ) )
			OnRequestEnter( pawn );
	}

	/// <summary>
	/// Request to enter the seat.
	/// </summary>
	[Rpc.Host]
	protected void RpcRequestEnter()
	{
		if ( Server.TryFindPawn( Rpc.Caller, out var pawn ) )
			OnRequestEnter( pawn );
	}

	/// <summary>
	/// Request to exit the seat.
	/// </summary>
	[Rpc.Host]
	protected void RpcRequestExit()
	{
		if ( Server.TryFindPawn( Rpc.Caller, out var pawn ) )
			OnRequestExit( pawn );
	}

	protected virtual void OnRequestEnter( Pawn pawn )
	{
		if ( !CanEnter( pawn ) )
			return;

		// TEMP: this is shit
		if ( Sitter.IsValid() )
			Sitter.Seat = null;

		Sitter = pawn;
		pawn.Seat = this;
	}

	protected virtual void OnRequestExit( Pawn pawn )
	{
		if ( Sitter.IsValid() && Sitter != pawn )
			return;

		// TEMP: this is also shit
		Sitter = null;
		pawn.Seat = null;
	}

	protected virtual void OnPawnEnter( Pawn pawn )
	{
	}

	protected virtual void OnPawnExit( Pawn pawn )
	{
	}

	public void UpdatePlayerAnimator( PlayerController controller, SkinnedModelRenderer renderer )
	{
	}

	public Transform CalculateEyeTransform( PlayerController controller )
		=> controller.EyeTransform; // TEMP?

	public void AskToLeave( PlayerController controller )
		=> RpcRequestExit();
}
