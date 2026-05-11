using System.Text.Json.Serialization;

namespace GameFish;

partial class Equipment
{
	public const string OFFSETS = "Offsets";

	protected const int VIEW_ORDER = EQUIP_ORDER + 100;

	/// <summary>
	/// The idle position/rotation.
	/// </summary>
	[Property]
	[InlineEditor]
	[Title( "Default" )]
	[Feature( VIEW ), Group( OFFSETS )]
	public virtual Offset ViewDefaultOffset { get; set; } = new( new Vector3( 15f, -6f, -2.5f ) );

	/// <summary>
	/// The position/rotation to go to when aiming.
	/// </summary>
	[Property]
	[InlineEditor]
	[Title( "Aiming" )]
	[Order( VIEW_ORDER )]
	[Feature( VIEW ), Group( OFFSETS )]
	public virtual Offset ViewAimingOffset { get; set; } = new( new Vector3( 10f, 0f, -2f ) );

	/// <summary>
	/// The postion/rotation when first deploying this.
	/// </summary>
	[Property]
	[InlineEditor]
	[Order( VIEW_ORDER )]
	[Title( "Deploying" )]
	[Feature( VIEW ), Group( OFFSETS )]
	public virtual Offset ViewDeployOffset { get; set; } = new( Vector3.Down * 70f, Rotation.Identity );

	/// <summary>
	/// The position/rotation to go to when holstering.
	/// </summary>
	[Property]
	[InlineEditor]
	[Order( VIEW_ORDER )]
	[Title( "Holstering" )]
	[Feature( VIEW ), Group( OFFSETS )]
	public virtual Offset ViewHolsterOffset { get; set; } = new( Vector3.Down * 70f, Rotation.FromYaw( -45f ) );

	/// <summary>
	/// Where this view model should be moved towards over time.
	/// </summary>
	[Property]
	[Order( VIEW_ORDER )]
	[JsonIgnore, ReadOnly]
	[Title( "Target Offset" )]
	[Feature( VIEW ), Group( DEBUG )]
	[ShowIf( nameof( InGame ), true )]
	protected Transform InspectorViewTargetTransform => GetViewOffsetTarget();

	/// <summary>
	/// The current relative orientation. <br />
	/// Setting this automatically sets the transform.
	/// </summary>
	[Property]
	[Order( VIEW_ORDER )]
	[Feature( VIEW ), Group( DEBUG )]
	public Transform ViewOffset
	{
		get => _offset;
		set
		{
			if ( !ITransform.IsValid( value ) )
				return;

			_offset = value;

			if ( this.InGame() )
				OnSetOffset( in value );
		}
	}

	protected Transform _offset = global::Transform.Zero;

	/// <inheritdoc cref="ViewRenderer.RelativeVelocity"/>
	protected virtual Vector3 RelativeVelocity => ViewRenderer?.RelativeVelocity ?? default;

	/// <summary>
	/// Multiplies the effects of sway and bob and such.
	/// </summary>
	protected virtual float ViewOffsetEffectScale => IsAiming ? ViewOffsetAimingEffectScale : 1.0f;

	/// <summary>
	/// The offset effect multiplier when aiming down sight.
	/// </summary>
	protected virtual float ViewOffsetAimingEffectScale => 0.2f;

	protected override void OnEnabled()
	{
		base.OnEnabled();

		// Snap to the destination.
		ViewOffset = GetViewOffsetTarget();
	}

	public virtual void OnSetOffset( in Transform newOffset ) { }

	/// <summary>
	/// Transitions the current offset with optional effects.
	/// </summary>
	/// <param name="moveSpeed"> How fast should we transition to the target offset? </param>
	/// <param name="deltaTime"> The frame rate. </param>
	public virtual void UpdateViewOffset( in float moveSpeed, in float deltaTime )
	{
		if ( !ITransform.IsValid( ViewOffset ) )
			ViewOffset = ViewDefaultOffset;

		// Transition to the target.
		var target = GetViewOffsetTarget();
		var offset = ViewOffset.LerpTo( target, moveSpeed * deltaTime );

		// Apply effects like sway and such.
		ViewRenderer?.ApplyOffsetEffects( ref offset, ViewOffsetEffectScale, in deltaTime );

		// Apply to active offset.
		ViewOffset = offset;
	}

	/// <returns> Where the view renderer wants to go to. </returns>
	public virtual Transform GetViewOffsetTarget()
	{
		if ( IsAiming )
			return ViewAimingOffset;

		return ViewDefaultOffset;
	}
}
