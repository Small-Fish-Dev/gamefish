namespace GameFish;

/// <summary>
/// This should be on a child object of the pawn's viewing object.
/// </summary>
[Icon( "sports_mma" )]
public partial class ViewRenderer : BaseEntity, ISkinned
{
	public const string VIEW = PawnView.VIEW;

	public const string GROUP_OFFSETS = "Offsets";

	[Property]
	[Feature( VIEW )]
	public PawnView View
	{
		get => _view.IsValid() ? _view
			: _view = Components?.Get<PawnView>( FindMode.EverythingInAncestors );
	}

	protected PawnView _view;

	[Property]
	[Feature( VIEW )]
	[Title( "Renderer" )]
	public SkinnedModelRenderer ModelRenderer
	{
		// Auto-cache the component.
		get => _wr.IsValid() ? _wr
			: _wr = Components?.Get<SkinnedModelRenderer>( FindMode.EverythingInDescendants );

		set { _wr = value; }
	}

	protected SkinnedModelRenderer _wr;

	public SkinnedModelRenderer SkinRenderer { get => ModelRenderer; set => _wr = value; }

	/// <summary>
	/// How quickly to affect the view model's orientation towards its destination.
	/// </summary>
	[Property]
	[Feature( VIEW ), Group( GROUP_OFFSETS )]
	public virtual float Speed { get; set; } = 15f;

	/// <summary>
	/// The current orientation. <br />
	/// Setting this automatically sets the transform.
	/// </summary>
	[Property, ReadOnly, InlineEditor]
	[Feature( VIEW ), Group( GROUP_OFFSETS )]
	public virtual Offset Offset
	{
		get => _offset;
		set
		{
			_offset = value;

			if ( this.InGame() )
			{
				UpdateTransform();
				OnSetOffset( in value );
			}
		}
	}

	protected Offset _offset;

	/// <summary>
	/// Where this view model should be moved towards over time.
	/// </summary>
	[Sync]
	[Property, ReadOnly, InlineEditor]
	[Feature( VIEW ), Group( GROUP_OFFSETS )]
	public Offset TargetOffset
	{
		get => _targetOffset;
		set => _targetOffset = value;
	}

	protected Offset _targetOffset;

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
		Offset = Offset.LerpTo( in _targetOffset, Speed * deltaTime );
	}

	public virtual void OnSetOffset( in Offset newOffset )
	{
	}
}
