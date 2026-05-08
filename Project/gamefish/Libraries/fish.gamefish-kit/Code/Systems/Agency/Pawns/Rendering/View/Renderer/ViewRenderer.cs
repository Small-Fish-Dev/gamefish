using System.Text.Json.Serialization;

namespace GameFish;

/// <summary>
/// This should be on a child object of the pawn's viewing object.
/// </summary>
[Icon( "sports_mma" )]
public partial class ViewRenderer : Module, ISkinned
{
	public const string OFFSETS = "Offsets";

	protected const int VIEW_ORDER = DEFAULT_ORDER - 1000;

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
	/// The default offset transition speed.
	/// </summary>
	[Property]
	[Title( "Speed" )]
	[Order( VIEW_ORDER )]
	[Feature( VIEW ), Group( OFFSETS )]
	public virtual float OffsetSpeed { get; set; } = 15f;

	protected Pawn Pawn => View?.ParentPawn;
	protected Equipment ActiveEquip => Pawn?.ActiveEquip;

	public virtual void UpdateOffset( in Offset offset )
		=> this.SetOffset( in offset );

	/// <summary>
	/// Determines the target offset and transitions the current offset to it.
	/// </summary>
	public virtual void UpdateOffset( in float deltaTime )
	{
		// What's the current equipment's view offset?
		var equip = ActiveEquip;

		if ( !equip.IsValid() )
			return;

		equip.UpdateOffset( OffsetSpeed, in deltaTime );

		// Apply it.
		UpdateOffset( equip.Offset );
	}
}
