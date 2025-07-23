namespace GameFish;

/// <summary>
/// This should be on a child object of the pawn's viewing object.
/// </summary>
[Icon( "sports_mma" )]
public partial class ViewModel : Component, ISkinned
{
	public const string VIEW = PawnView.VIEW;

	public const string GROUP_OFFSETS = "Offsets";

	[Property]
	[Title( "Renderer" )]
	[Feature( VIEW )]
	public SkinnedModelRenderer SkinRenderer
	{
		// Auto-cache the component.
		get => _r.IsValid() ? _r
			: _r = Components?.Get<SkinnedModelRenderer>( FindMode.EverythingInSelf );

		set { _r = value; }
	}

	protected SkinnedModelRenderer _r;

	/// <summary>
	/// How quickly to affect the view model's orientation towards its destination.
	/// </summary>
	[Sync]
	[Property]
	[Feature( VIEW ), Group( GROUP_OFFSETS )]
	public float Speed { get; set; } = 15f;

	/// <summary>
	/// The current orientation. <br />
	/// Setting this automatically sets the transform.
	/// </summary>
	[Property, ReadOnly, InlineEditor]
	[Feature( VIEW ), Group( GROUP_OFFSETS )]
	public Offset Offset
	{
		get => _offset;
		set
		{
			_offset = value;

			UpdateTransform();
			OnSetOffset( in value );
		}
	}

	protected Offset _offset;

	/// <summary>
	/// Where this view model should be moved towards over time.
	/// </summary>
	[Sync]
	[Property, ReadOnly, InlineEditor]
	[Feature( VIEW ), Group( GROUP_OFFSETS )]
	public Offset TargetOffset { get; set; }

	protected override void OnEnabled()
	{
		base.OnEnabled();

		// Snap to the destination.
		Offset = TargetOffset;
	}

	public virtual void UpdateTransform()
	{
		this.SetOffset( Offset );
	}

	/// <summary>
	/// Sets <see cref="Offset"/> to <see cref="TargetOffset"/>.
	/// </summary>
	public virtual void UpdateOffset( in float deltaTime )
	{
		Offset = Offset.LerpTo( TargetOffset, deltaTime * Speed );
	}

	public virtual void OnSetOffset( in Offset newOffset )
	{
	}
}
