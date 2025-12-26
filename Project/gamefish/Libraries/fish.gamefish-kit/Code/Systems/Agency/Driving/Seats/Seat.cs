using System.Text.Json.Serialization;
using Sandbox.Movement;

namespace GameFish;

[Icon( "chair" )]
public partial class Seat : Module, IUsable, ISitTarget
{
	protected const int SEAT_ORDER = DEFAULT_ORDER - 1000;
	protected const int SEAT_DEBUG_ORDER = SEAT_ORDER - 10;

	protected override bool? IsNetworkedOverride => true;
	protected override bool IsNetworkedAutomatically => true;

	public override bool IsParent( ModuleEntity comp )
		=> comp is Vehicle; // auto-attach to vehicles

	public Vehicle Vehicle => Parent as Vehicle;

	public virtual float UsablePriority => 0f;

	[Title( "Sitter" )]
	[Property, JsonIgnore, ReadOnly]
	[Feature( SEAT ), Group( DEBUG ), Order( SEAT_DEBUG_ORDER )]
	public Pawn InspectorSitter => Sitter;

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

	protected virtual void OnSetSitter( Pawn newSitter, Pawn oldSitter )
	{
		if ( newSitter.IsValid() )
			newSitter.OnSeatMoved( this );
	}

	protected override void OnStart()
	{
		base.OnStart();

		Transform.OnTransformChanged = OnMoved;
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
			TryRequestEnter( pawn );
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
