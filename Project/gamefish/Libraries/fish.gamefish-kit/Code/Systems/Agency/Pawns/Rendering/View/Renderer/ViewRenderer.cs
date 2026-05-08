using System.Text.Json.Serialization;

namespace GameFish;

/// <summary>
/// This should be on a child object of the pawn's viewing object.
/// </summary>
[Icon( "sports_mma" )]
public partial class ViewRenderer : Module, ISkinned
{
	protected const int VIEW_ORDER = DEFAULT_ORDER - 1000;

	public const string GROUP_OFFSETS = "Offsets";

	public override bool IsParent( ModuleEntity comp )
		=> comp is PawnView;

	[Feature( VIEW )]
	[Property, ReadOnly, JsonIgnore]
	public PawnView View => Parent as PawnView;

	[Property]
	[Feature( VIEW )]
	[Order( VIEW_ORDER )]
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
	[Order( VIEW_ORDER )]
	[Feature( VIEW ), Group( GROUP_OFFSETS )]
	public virtual float Speed { get; set; } = 15f;

	/// <summary>
	/// The current orientation. <br />
	/// Setting this automatically sets the transform.
	/// </summary>
	[Order( VIEW_ORDER )]
	[Property, ReadOnly, InlineEditor]
	[Feature( VIEW ), Group( GROUP_OFFSETS )]
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

	/// <summary>
	/// Where this view model should be moved towards over time.
	/// </summary>
	[Property]
	[InlineEditor]
	[Order( VIEW_ORDER )]
	[JsonIgnore, ReadOnly]
	[Title( "Target Offset" )]
	[Feature( VIEW ), Group( GROUP_OFFSETS )]
	protected Offset InspectorTargetOffset => TargetOffset;

	[Sync]
	public Offset TargetOffset { get; protected set; } = new();

	protected Pawn Pawn => View?.ParentPawn;
	protected Equipment ActiveEquip => Pawn?.ActiveEquip;

	protected override void OnEnabled()
	{
		base.OnEnabled();

		// Snap to the destination.
		Offset = TargetOffset;
	}

	public virtual void OnSetOffset( in Offset newOffset )
	{
		UpdateTransform();
	}

	/// <summary>
	/// Determines the target offset and transitions the current offset to it.
	/// </summary>
	public virtual void UpdateOffset( in float deltaTime )
	{
		// What's the current equipment's offset?
		var equip = ActiveEquip;

		if ( equip.IsValid() )
		{
			var target = equip.GetViewRendererOffset();

			if ( ITransform.IsValid( target ) )
				TargetOffset = target;
		}

		// Transition to it.
		Offset = Offset.LerpTo( TargetOffset, Speed * deltaTime );
	}

	public virtual void UpdateTransform()
	{
		this.SetOffset( Offset );
	}
}
