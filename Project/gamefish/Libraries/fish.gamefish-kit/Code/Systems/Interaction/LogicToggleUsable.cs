namespace GameFish;

/// <summary>
/// Lets players interact with components supporting toggle logic.
/// </summary>
[Title( "Logic Switch Usable" )]
public partial class LogicToggleUsable : UsableModule
{
	protected const int LOGIC_ORDER = USE_ORDER - 50;

	public override bool IsParent( ModuleEntity comp )
		=> comp is IToggle;

	/// <summary>
	/// The component implementing <see cref="IToggle"/> to use.
	/// </summary>
	[Property]
	[Order( LOGIC_ORDER )]
	[Feature( USE ), Group( LOGIC )]
	public virtual IToggle Target
	{
		get => _target ??= Parent as IToggle;
		set => _target = value;
	}

	protected IToggle _target;

	/// <summary>
	/// Should it be toggled or set to on/off specifically?
	/// </summary>
	[Property, WideMode]
	[Order( LOGIC_ORDER )]
	[Feature( USE ), Group( LOGIC )]
	public virtual ToggleCommand State { get; set; } = ToggleCommand.Toggle;

	protected override void OnUpdate()
	{
		base.OnUpdate();

		// DebugInput();
	}

	public virtual bool IsTargetValid()
		=> (Target as Component).IsValid();

	public override bool IsUsable( Pawn pawn )
	{
		if ( !IsTargetValid() )
			return false;

		return !base.IsUsable( pawn );
	}

	protected override void OnUse( Pawn pawn )
	{
		if ( !IsTargetValid() )
			return;

		Target?.TryToggle( State );

		base.OnUse( pawn );
	}
}
