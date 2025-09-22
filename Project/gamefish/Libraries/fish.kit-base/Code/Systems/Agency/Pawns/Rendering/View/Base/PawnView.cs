namespace GameFish;

/// <summary>
/// The central view manager for the pawn. <br />
/// This should be on a child object of the pawn(otherwise expect problems).
/// </summary>
[Icon( "videocam" )]
public partial class PawnView : Module, ISimulate
{
	public const string SPECTATING = BasePawn.SPECTATOR;

	public const string CYCLING = "Cycling";
	public const string TRANSITIONING = "Transitioning";

	/// <summary>
	/// If true: the view will be traced from aiming origin to the destination and collide according to its settings.
	/// </summary>
	[Property]
	[Title( "Enabled" )]
	[Feature( VIEW ), Group( COLLISION )]
	public virtual bool Collision { get; set; } = true;

	/// <summary>
	/// Radius of the sphere collider used when tracing.
	/// </summary>
	[Property]
	[Title( "Radius" )]
	[Range( 1f, 64f, clamped: false )]
	[Feature( VIEW ), Group( COLLISION )]
	public virtual float CollisionRadius { get; set; } = 8f;
	public virtual float GetCollisionRadius() => CollisionRadius * WorldScale.x.NonZero();

	/// <summary>
	/// If true: collide with objects we have explicit ownership over.
	/// </summary>
	[Property]
	[Title( "Hit Owned" )]
	[Feature( VIEW ), Group( COLLISION )]
	public virtual bool CollideOwned { get; set; } = false;

	/// <summary>
	/// Tags of objects that will obstruct the camera view.
	/// </summary>
	[Property]
	[Title( "Hit Tags" )]
	[Feature( VIEW ), Group( COLLISION )]
	public TagSet CollisionHitTags { get; set; } = ["solid"];

	/// <summary>
	/// Tags of objects that will obstruct the camera view.
	/// </summary>
	[Property]
	[Title( "Ignore Tags" )]
	[Feature( VIEW ), Group( COLLISION )]
	public TagSet CollisionIgnoreTags { get; set; } = [TAG_PAWN];

	/// <summary>
	/// The pawn this view actually belongs to.
	/// </summary>
	public BasePawn ParentPawn => Parent as BasePawn;

	/// <summary>
	/// The pawn we're currently looking at/through.
	/// </summary>
	public virtual BasePawn TargetPawn => ParentPawn;

	public override bool IsParent( ModuleEntity comp )
		=> comp.IsValid() && comp is BasePawn;

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

		OnPerspectiveUpdate( deltaTime );

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
	/// Checks if something would fuck up and if so: warns about it then disables this view.
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
}
