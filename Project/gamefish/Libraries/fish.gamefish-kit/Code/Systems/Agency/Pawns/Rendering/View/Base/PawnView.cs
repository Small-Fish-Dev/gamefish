namespace GameFish;

/// <summary>
/// The central view manager for the pawn. <br />
/// This should be on a child object of the pawn(otherwise expect problems).
/// </summary>
[Icon( "videocam" )]
public partial class PawnView : PawnModule, ISimulate
{
	protected const int VIEW_ORDER = PAWN_ORDER - 1000;
	protected const int VIEW_DEBUG_ORDER = VIEW_ORDER - 10;

	protected const int MODES_ORDER = VIEW_ORDER + 100;
	protected const int MODES_DEBUG_ORDER = MODES_ORDER - 10;

	protected override bool? IsNetworkedOverride => true;

	/// <summary>
	/// If true: the view will be traced from aiming origin to the destination and collide according to its settings.
	/// </summary>
	[Property]
	[Title( "Enabled" )]
	[Feature( VIEW ), ToggleGroup( nameof( Collision ), Label = COLLISION )]
	public virtual bool Collision { get; set; } = true;

	/// <summary>
	/// Radius of the sphere collider used when tracing.
	/// </summary>
	[Property]
	[Title( "Radius" )]
	[Range( 1f, 64f, clamped: false )]
	[Feature( VIEW ), ToggleGroup( nameof( Collision ) )]
	protected virtual float DefaultCollisionRadius { get; set; } = 8f;

	public virtual float GetCollisionRadius() => DefaultCollisionRadius * WorldScale.x.NonZero();

	/// <summary>
	/// If true: collide with objects we have explicit ownership over.
	/// </summary>
	[Property]
	[Title( "Hit Owned" )]
	[Feature( VIEW ), ToggleGroup( nameof( Collision ) )]
	public virtual bool CollideOwned { get; set; } = false;

	/// <summary>
	/// The pawn this view actually belongs to.
	/// </summary>
	public virtual Pawn ParentPawn => Parent as Pawn;

	/// <summary>
	/// The pawn we're currently looking at/through.
	/// </summary>
	public virtual Pawn TargetPawn => ParentPawn;

	public virtual bool CanSimulate()
		=> ParentPawn?.CanSimulate() ?? false;

	protected override void OnStart()
	{
		base.OnStart();

		EnsureValidHierarchy();
	}

	protected override void OnPreRender()
	{
		base.OnPreRender();

		if ( !InGame )
			return;

		UpdatePawn();
		UpdateViewRenderer();
	}

	public virtual void FrameSimulate( in float deltaTime )
	{
		if ( !this.InGame() )
			return;

		HandleInput();

		UpdateRecoil( in deltaTime );

		UpdateTransition( in deltaTime );

		UpdateViewMode( in deltaTime );
	}

	/// <returns> If we are looking through this view. </returns>
	public virtual bool IsViewing()
	{
		var pawn = ParentPawn;

		if ( !pawn.IsValid() )
			return false;

		if ( pawn.AllowInput() )
			return true;

		var specView = (Client.Local?.Pawn as Spectator)?.View as SpectatorView;

		if ( !specView.IsValid() )
			return false;

		return specView.TargetPawn == pawn;
	}

	protected virtual void UpdateViewRenderer()
	{
		if ( !IsViewing() )
		{
			ToggleViewRenderer( false );
			return;
		}

		var isFirstPerson = Mode?.InFirstPerson() is true;
		ToggleViewRenderer( isFirstPerson );
	}

	/// <summary>
	/// Tell the targeted pawn about this view. <br />
	/// You should call this once after processing whatever else.
	/// </summary>
	protected virtual void UpdatePawn()
	{
		TargetPawn?.OnViewUpdate( this );
	}

	/// <summary>
	/// Checks if something would fuck up and if so: warns about it.
	/// </summary>
	protected void EnsureValidHierarchy()
	{
		var pawn = ParentPawn;

		if ( !pawn.IsValid() )
			return;

		if ( pawn.GameObject == GameObject )
		{
			this.Warn( this + " was directly on the pawn! It needs to be a child!" );
			GameObject.SetParent( pawn.GameObject );
		}
	}

	public virtual void ToggleViewRenderer( bool isEnabled )
	{
		var vm = ViewRenderer;

		if ( vm.IsValid() )
			vm.GameObject.Enabled = isEnabled;
	}

	protected virtual void UpdateViewMode( in float deltaTime )
	{
		Mode?.OnModeUpdate( in deltaTime );
	}
}
