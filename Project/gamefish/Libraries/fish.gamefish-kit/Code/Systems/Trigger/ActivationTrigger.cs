namespace GameFish;

/// <summary>
/// Sends activation signal(s) to component(s) with <see cref="IActivate"/> when triggered.
/// <code> trigger_multiple </code>
/// </summary>
[Icon( "electric_bolt" )]
[EditorHandle( Icon = "⚡" )]
public partial class ActivationTrigger : FilterTrigger
{
	protected const int LOGIC_ORDER = TRIGGER_ORDER - 1000;
	protected const int LOGIC_DEBUG_ORDER = LOGIC_ORDER - 100;

	[Property]
	[Title( "Render (ingame)" )]
	[Order( LOGIC_DEBUG_ORDER )]
	[Feature( LOGIC ), Group( DEBUG )]
	public bool DebugRenderTargetsInGame { get; set; }

	[Property]
	[Order( LOGIC_ORDER )]
	[WideMode( HasLabel = false )]
	[Feature( LOGIC ), Group( TARGETS )]
	public virtual List<LogicAction> Targets { get; set; } = [];

	protected virtual Color ValidTargetColor => Color.White.WithAlpha( 0.3f );
	protected virtual Color InvalidTargetColor => Color.Red.Desaturate( 0.4f ).Darken( 0.2f ).WithAlpha( 0.3f );

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( DebugRenderTargetsInGame )
			RenderTargets();
	}

	protected override void DrawGizmos()
	{
		base.DrawGizmos();

		if ( !Gizmo.IsSelected )
			return;

		RenderTargets();
	}

	protected override void OnTouchStart( GameObject obj )
	{
		base.OnTouchStart( obj );

		ActivateTargets();
	}

	public virtual void ActivateTargets()
	{
		if ( Targets is null )
			return;

		foreach ( var logic in Targets )
			logic.Execute();
	}

	protected virtual void RenderTargets()
	{
		// TODO: Debug rendering in the LogicAction struct.

		/*
		if ( Targets is null )
			return;

		foreach ( var tgt in Targets )
		{
			if ( tgt is not Component c || !c.IsValid() )
				continue;

			var isValid = IsValidTarget( tgt );

			if ( isValid && InGame )
				isValid = tgt?.CanActivate( this ) is true;

			var color = isValid
				? ValidTargetColor
				: InvalidTargetColor;

			this.DrawArrow( Center, c.WorldPosition, color, len: 24f, w: 9f, tWorld: global::Transform.Zero );
		}
		*/
	}
}
