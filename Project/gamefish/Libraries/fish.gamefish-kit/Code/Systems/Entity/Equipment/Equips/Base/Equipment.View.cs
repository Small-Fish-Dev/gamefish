using System.Text.Json.Serialization;

namespace GameFish;

partial class Equipment
{
	public const string OFFSETS = "Offsets";

	protected const int VIEW_ORDER = EQUIP_ORDER + 100;

	/// <summary>
	/// The idle position/rotation.
	/// </summary>
	[Property, InlineEditor]
	[Feature( VIEW ), Group( OFFSETS )]
	public virtual Offset DefaultOffset { get; set; } = new( new Vector3( 15f, -6f, -2.5f ) );

	/// <summary>
	/// The position/rotation to go to when aiming.
	/// </summary>
	[Order( VIEW_ORDER )]
	[Property, InlineEditor]
	[Feature( VIEW ), Group( OFFSETS )]
	public virtual Offset AimingOffset { get; set; } = new( new Vector3( 10f, 0f, -2f ) );

	/// <summary>
	/// The postion/rotation when first deploying this.
	/// </summary>
	[Order( VIEW_ORDER )]
	[Property, InlineEditor]
	[Feature( VIEW ), Group( OFFSETS )]
	public virtual Offset DeployOffset { get; set; } = new( Vector3.Down * 70f, Rotation.Identity );

	/// <summary>
	/// The position/rotation to go to when holstering.
	/// </summary>
	[Order( VIEW_ORDER )]
	[Property, InlineEditor]
	[Feature( VIEW ), Group( OFFSETS )]
	public virtual Offset HolsterOffset { get; set; } = new( Vector3.Down * 70f, Rotation.FromYaw( -45f ) );

	/// <summary>
	/// Where this view model should be moved towards over time.
	/// </summary>
	[Property]
	[InlineEditor]
	[Order( VIEW_ORDER )]
	[JsonIgnore, ReadOnly]
	[Title( "Target Offset" )]
	[Feature( VIEW ), Group( DEBUG )]
	[ShowIf( nameof( InGame ), true )]
	protected Offset InspectorTargetOffset => GetViewOffsetTarget();

	/// <summary>
	/// The current orientation. <br />
	/// Setting this automatically sets the transform.
	/// </summary>
	[Order( VIEW_ORDER )]
	[Feature( VIEW ), Group( DEBUG )]
	[Property, ReadOnly, InlineEditor]
	public virtual Offset Offset
	{
		get => _offset;
		set
		{
			_offset = value;

			if ( this.InGame() )
				OnSetOffset( in value );
		}
	}

	protected Offset _offset;

	public ViewRenderer ViewRenderer => Pawn?.ViewRenderer;

	protected override void OnEnabled()
	{
		base.OnEnabled();

		// Snap to the destination.
		Offset = GetViewOffsetTarget();
	}

	public virtual void OnSetOffset( in Offset newOffset ) { }

	public virtual void UpdateOffset( in float speed, in float deltaTime )
	{
		var target = GetViewOffsetTarget();

		if ( !ITransform.IsValid( target ) )
			return;

		// Transition to it.
		Offset = Offset.LerpTo( target, speed * deltaTime );
	}

	public virtual Offset GetViewOffsetTarget()
		=> DefaultOffset;
}
