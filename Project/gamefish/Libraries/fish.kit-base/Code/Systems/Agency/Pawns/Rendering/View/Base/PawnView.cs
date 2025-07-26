namespace GameFish;

/// <summary>
/// The central view manager for the pawn. <br />
/// This should be on a child object of the pawn(otherwise expect problems).
/// </summary>
[Icon( "videocam" )]
public partial class PawnView : Module<BasePawn>, ISimulate
{
	public const string VIEW = "🎥 View";
	public const string SPECTATING = BasePawn.SPECTATING;

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
	public virtual float CollisionRadius { get; set; } = 16f;

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
	public TagSet CollisionIgnoreTags { get; set; } = [BaseEntity.TAG_PAWN];

	/// <summary>
	/// The pawn we're looking at/through.
	/// </summary>
	public virtual BasePawn Pawn => ModuleParent;

	/// <summary>
	/// How to scale the opacity from distance.
	/// This is for the pawn itself to consider using.
	/// </summary>
	public float PawnOpacity { get; set; } = 1f;

	public virtual bool CanSimulate()
		=> ModuleParent?.CanSimulate() ?? false;

	protected override void OnStart()
	{
		base.OnStart();

		EnsureValidHierarchy();
	}

	public virtual void FrameSimulate( in float deltaTime )
	{
		HandleInput();

		OnPerspectiveUpdate( Time.Delta );

		UpdateTransition();

		PawnOpacity = DistanceFromEye.Remap( FirstPersonRange.Max, FirstPersonRange.Min );
	}

	/// <summary>
	/// Checks if something would fuck up and if so: warns about it then disables this view.
	/// </summary>
	protected void EnsureValidHierarchy()
	{
		var pawn = Pawn;

		if ( !pawn.IsValid() )
			return;

		if ( pawn.GameObject == GameObject )
		{
			this.Warn( this + " was directly on the pawn! It needs to be a child!" );
			Enabled = false;
		}
	}
}
