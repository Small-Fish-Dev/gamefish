namespace GameFish;

/// <summary>
/// A spherical gravitational field.
/// <br /> <br />
/// <b> NOTE: </b> Very useful for planets.
/// </summary>
[Icon( "nat" )]
public partial class GravityField : ModuleEntity, Component.ITriggerListener
{
	protected const int GRAVITY_ORDER = ENTITY_ORDER - 1000;

	public override string ToString()
		=> $"{nameof( GravityField )}|{Name}";

	[Property]
	[Title( "Logging" )]
	[Order( GRAVITY_ORDER )]
	[Feature( GRAVITY ), Group( DEBUG )]
	public bool DebugLogging { get; set; } = false;

	[Property]
	[WideMode]
	[Order( GRAVITY_ORDER )]
	[Feature( GRAVITY ), Group( DISPLAY )]
	public string Name { get; set; }

	/// <summary>
	/// A higher priority is chosen as the primary field.
	/// </summary>
	[Property]
	[Feature( GRAVITY )]
	[Order( GRAVITY_ORDER )]
	[Range( 0, 1000, clamped: false )]
	public int Priority { get; set; } = 0;

	[Property]
	[Feature( GRAVITY )]
	[Order( GRAVITY_ORDER )]
	public float Force { get; set; } = 600f;

	/// <summary>
	/// If true: if this is the primary field it will block the effects of other fields.
	/// </summary>
	[Property]
	[Feature( GRAVITY )]
	[Order( GRAVITY_ORDER )]
	public virtual bool IsOverride { get; set; } = false;

	public override Vector3 Center => WorldPosition;

	/// <summary>
	/// Tells <see cref="GravityModule"/>s what force they should apply.
	/// </summary>
	/// <param name="point"> Probably the object's center of gravity. </param>
	/// <returns> What force to apply(per second) from that point. </returns>
	public virtual Vector3 GetForce( Vector3 point )
		=> point.Direction( Center ) * Force;

	protected static bool TryFindGravityModule( GameObject obj, out GravityModule g )
	{
		g = null;

		if ( !obj.IsValid() )
			return false;

		return obj.Components.TryGet( out g, FindMode.EnabledInSelf | FindMode.InChildren );
	}

	void ITriggerListener.OnTriggerEnter( GameObject other )
	{
		if ( !TryFindGravityModule( other, out var g ) )
			return;

		if ( DebugLogging )
			this.Log( $"Enter: {g?.Parent ?? g}" );

		OnEnter( g );
	}

	void ITriggerListener.OnTriggerExit( GameObject other )
	{
		if ( !TryFindGravityModule( other, out var g ) )
			return;

		if ( DebugLogging )
			this.Log( $"Exit: {g?.Parent ?? g}" );

		OnExit( g );
	}

	protected virtual void OnEnter( GravityModule g )
		=> g?.OnEnter( this );

	protected virtual void OnExit( GravityModule g )
		=> g?.OnExit( this );
}
