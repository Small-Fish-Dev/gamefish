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

	protected const int SWAY_ORDER = VIEW_ORDER + 20;

	public override bool IsParent( ModuleEntity comp )
		=> comp is PawnView;

	public PawnView View => Parent as PawnView;

	[Property]
	[Order( VIEW_ORDER )]
	[Title( "Renderer" )]
	[Feature( VIEW ), Group( MODEL )]
	public virtual SkinnedModelRenderer ModelRenderer
	{
		// Auto-cache the component.
		get => _wr.IsValid() ? _wr
			: _wr = Components?.Get<SkinnedModelRenderer>( FindMode.EverythingInDescendants );

		set { _wr = value; }
	}

	protected SkinnedModelRenderer _wr;

	SkinnedModelRenderer ISkinned.SkinRenderer { get => ModelRenderer; set => _wr = value; }

	/// <summary>
	/// The default speed of offset target transitioning.
	/// </summary>
	[Property]
	[Feature( VIEW )]
	[Order( VIEW_ORDER )]
	[Title( "Offset Speed" )]
	public virtual float OffsetSpeed { get; set; } = 15f;

	/// <summary>
	/// Should the view be swayed?
	/// </summary>
	[Property]
	[Feature( VIEW )]
	[Order( SWAY_ORDER )]
	[ToggleGroup( nameof( ViewSwayEnabled ), Label = SWAY )]
	public virtual bool ViewSwayEnabled { get; set; } = true;

	/// <summary>
	/// If true: velocity will sway the offset of the view renderer.
	/// </summary>
	[Property]
	[Feature( VIEW )]
	[Title( "Velocity" )]
	[Order( SWAY_ORDER )]
	[Range( 0.5f, 1f, clamped: false )]
	[ToggleGroup( nameof( ViewSwayEnabled ) )]
	public virtual bool ViewSwayVelocityEnabled { get; set; } = true;

	/// <summary>
	/// The min/max of movement speed used to calculate sway.
	/// </summary>
	[Property]
	[Feature( VIEW )]
	[Order( SWAY_ORDER )]
	[Title( "Velocity Range" )]
	[Range( 0.5f, 1f, clamped: false )]
	[ToggleGroup( nameof( ViewSwayEnabled ) )]
	[ShowIf( nameof( ViewSwayVelocityEnabled ), true )]
	public virtual FloatRange ViewSwayVelocityRange { get; set; } = new( 10f, 1000f );

	/// <summary>
	/// The maximum distance that velocity can sway the view offset.
	/// </summary>
	[Property]
	[Feature( VIEW )]
	[Order( SWAY_ORDER )]
	[Title( "Velocity Length" )]
	[Range( 0f, 300f, clamped: false )]
	[ToggleGroup( nameof( ViewSwayEnabled ) )]
	[ShowIf( nameof( ViewSwayVelocityEnabled ), true )]
	public virtual float ViewSwayVelocityLength { get; set; } = 150f;

	/// <summary>
	/// The power to raise velocity length by when calculating sway.
	/// </summary>
	[Property]
	[Feature( VIEW )]
	[Order( SWAY_ORDER )]
	[Title( "Velocity Exponent" )]
	[Range( 0.4f, 1f, clamped: false )]
	[ToggleGroup( nameof( ViewSwayEnabled ) )]
	[ShowIf( nameof( ViewSwayVelocityEnabled ), true )]
	public virtual float ViewSwayVelocityExponent { get; set; } = 0.7f;

	protected Pawn Pawn => View?.ParentPawn;
	protected Equipment ActiveEquip => Pawn?.ActiveEquip;

	/// <summary>
	/// If true: the view should be rendered.
	/// </summary>
	public bool IsVisible
	{
		get => _isVisible;
		set
		{
			if ( _isVisible == value )
				return;

			_isVisible = value;
			OnSetIsVisible( in value );
		}
	}

	protected bool _isVisible;

	protected virtual void OnSetIsVisible( in bool isVisible )
	{
		if ( ModelRenderer.IsValid() )
			ModelRenderer.Enabled = isVisible;
	}

	public virtual void SetRendererOffset( in Offset offset )
		=> this.SetOffset( in offset );

	/// <summary>
	/// The pawn's velocity relative to their perspective.
	/// </summary>
	public virtual Vector3 RelativeVelocity => (Pawn?.EyeRotation.Inverse * Pawn?.Velocity) ?? default;

	/// <summary>
	/// Determines the target offset and transitions the current offset to it.
	/// </summary>
	public virtual void UpdateOffset( in float deltaTime )
	{
		// What's the current equipment's view offset?
		var equip = ActiveEquip;

		if ( !equip.IsValid() )
			return;

		equip.UpdateViewOffset( OffsetSpeed, in deltaTime );

		// Apply it.
		SetRendererOffset( equip.ViewOffset );
	}

	public virtual void ApplyOffsetEffects( ref Transform t, in float scale, in float deltaTime )
	{
		var vel = RelativeVelocity * scale;

		if ( ViewSwayEnabled )
		{
			// TODO: Aim sway.
			if ( ViewSwayVelocityEnabled )
				ApplyVelocitySway( ref t, in vel, in deltaTime );
		}
	}

	public virtual void ApplyVelocitySway( ref Transform t, in Vector3 vel, in float deltaTime )
	{
		if ( vel.AlmostEqual( 0f ) )
			return;

		var min = ViewSwayVelocityRange.Min;
		var max = ViewSwayVelocityRange.Max;

		var len = vel.Length.Pow( ViewSwayVelocityExponent );
		var sway = len.Remap( min, max, 0f, ViewSwayVelocityLength );

		t.Position -= vel.Normal * sway * deltaTime;
	}
}
