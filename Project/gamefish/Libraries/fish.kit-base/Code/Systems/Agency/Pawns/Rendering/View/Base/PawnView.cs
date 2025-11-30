namespace GameFish;

/// <summary>
/// The central view manager for the pawn. <br />
/// This should be on a child object of the pawn(otherwise expect problems).
/// </summary>
[Icon( "videocam" )]
public partial class PawnView : Module, ISimulate
{
	public const string TRANSITIONING = "↔ Transitioning";

	protected override bool? IsNetworkedOverride => true;

	public override bool IsParent( ModuleEntity comp )
		=> comp is BasePawn;

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
	public virtual float CollisionRadius { get; set; } = 8f;
	public virtual float GetCollisionRadius() => CollisionRadius * WorldScale.x.NonZero();

	/// <summary>
	/// If true: collide with objects we have explicit ownership over.
	/// </summary>
	[Property]
	[Title( "Hit Owned" )]
	[Feature( VIEW ), ToggleGroup( nameof( Collision ) )]
	public virtual bool CollideOwned { get; set; } = false;

	/// <summary>
	/// Tags of objects that will obstruct the camera view.
	/// </summary>
	[Property]
	[Title( "Hit Tags" )]
	[Feature( VIEW ), ToggleGroup( nameof( Collision ) )]
	public TagSet CollisionHitTags { get; set; } = ["solid"];

	/// <summary>
	/// Tags of objects that will obstruct the camera view.
	/// </summary>
	[Property]
	[Title( "Ignore Tags" )]
	[Feature( VIEW ), ToggleGroup( nameof( Collision ) )]
	public TagSet CollisionIgnoreTags { get; set; } = [TAG_PAWN];

	/// <summary>
	/// The pawn this view actually belongs to.
	/// </summary>
	public virtual BasePawn ParentPawn => Parent as BasePawn;

	/// <summary>
	/// The pawn we're currently looking at/through.
	/// </summary>
	public virtual BasePawn TargetPawn => ParentPawn;

	public virtual bool CanSimulate()
		=> ParentPawn?.CanSimulate() ?? false;

	protected override void OnStart()
	{
		base.OnStart();

		EnsureValidHierarchy();
	}

	public virtual void FrameSimulate( in float deltaTime )
	{
		HandleInput();

		UpdateTransition( deltaTime );

		UpdateViewMode( deltaTime );

		UpdatePawn();
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
