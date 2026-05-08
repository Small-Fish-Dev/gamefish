namespace GameFish;

partial class Equipment
{
	protected const int VIEW_ORDER = EQUIP_ORDER + 100;

	/// <summary>
	/// The idle position/rotation.
	/// </summary>
	[Property, InlineEditor]
	[Feature( EQUIP ), Group( VIEW )]
	public virtual Offset DefaultOffset { get; set; } = new( new Vector3( 15f, -6f, -2.5f ) );

	/// <summary>
	/// The position/rotation to go to when aiming.
	/// </summary>
	[Order( VIEW_ORDER )]
	[Property, InlineEditor]
	[Feature( EQUIP ), Group( VIEW )]
	public virtual Offset AimingOffset { get; set; } = new( new Vector3( 10f, 0f, -2f ) );

	/// <summary>
	/// The postion/rotation when first deploying this.
	/// </summary>
	[Order( VIEW_ORDER )]
	[Property, InlineEditor]
	[Feature( EQUIP ), Group( VIEW )]
	public virtual Offset DeployOffset { get; set; } = new( Vector3.Down * 70f, Rotation.Identity );

	/// <summary>
	/// The position/rotation to go to when holstering.
	/// </summary>
	[Order( VIEW_ORDER )]
	[Property, InlineEditor]
	[Feature( EQUIP ), Group( VIEW )]
	public virtual Offset HolsterOffset { get; set; } = new( Vector3.Down * 70f, Rotation.FromYaw( -45f ) );

	public ViewRenderer ViewRenderer => Pawn?.ViewRenderer;

	public virtual Offset GetViewRendererOffset()
		=> DefaultOffset;
}
