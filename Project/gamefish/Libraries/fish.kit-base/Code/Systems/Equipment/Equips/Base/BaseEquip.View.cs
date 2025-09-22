namespace GameFish;

partial class BaseEquip
{
	/// <summary>
	/// The idle position/rotation.
	/// </summary>
	[Property, InlineEditor]
	[Feature( EQUIP ), Group( VIEW ), Order( 69 )]
	public Offset DefaultOffset { get; set; } = new( global::Transform.Zero );

	/// <summary>
	/// The postion/rotation when first deploying this.
	/// </summary>
	[Property, InlineEditor]
	[Feature( EQUIP ), Group( VIEW ), Order( 69 )]
	public Offset DeployedOffset { get; set; } = new( Vector3.Down * 70f, Rotation.Identity );

	/// <summary>
	/// The position/rotation to go to when holstering.
	/// </summary>
	[Property, InlineEditor]
	[Feature( EQUIP ), Group( VIEW ), Order( 69 )]
	public Offset HolsteringOffset { get; set; } = new( Vector3.Down * 70f, Rotation.FromYaw( -45f ) );

	public ViewRenderer ViewRenderer => Owner?.ViewRenderer;
}
