using System;

namespace GameFish;

[Hide, Obsolete( $"Use {nameof( ActivationTrigger )} instead." )]
public partial class ActivationTrigger : LogicTrigger;

/// <summary>
/// Executes logic upon being entered, exited or while inside.
/// <code> trigger_once </code>
/// <code> trigger_multiple </code>
/// </summary>
[Icon( "electric_bolt" )]
[EditorHandle( Icon = "⚡" )]
public partial class LogicTrigger : FilterTrigger
{
	protected const int LOGIC_ORDER = TRIGGER_ORDER - 1000;

	protected const int LOGIC_DEBUG_ORDER = LOGIC_ORDER - 100;

	protected const int LOGIC_TRIGGER_ORDER = LOGIC_ORDER + 10;
	protected const int LOGIC_FUNCTIONS_ORDER = LOGIC_ORDER + 50;

	/// <summary>
	/// If true: destroys itself upon successfully doing its job.
	/// </summary>
	[Property]
	[Title( "Once" )]
	[Order( LOGIC_TRIGGER_ORDER )]
	[Feature( LOGIC ), Group( TRIGGER )]
	public virtual bool SelfDestruct { get; protected set; } = false;

	/// <summary>
	/// What should trigger this?
	/// </summary>
	[Property]
	[Order( LOGIC_TRIGGER_ORDER )]
	[Feature( LOGIC ), Group( TRIGGER )]
	public virtual TriggerPhase Conditions { get; protected set; } = TriggerPhase.Enter | TriggerPhase.Exit;

	protected virtual bool HasInsideCondition => Conditions.HasInside();
	protected virtual bool HasEnterCondition => Conditions.HasEnter();
	protected virtual bool HasExitCondition => Conditions.HasExit();

	/// <summary>
	/// How frequently should logic be executed while something is inside of this trigger?
	/// </summary>
	[Property]
	[Order( LOGIC_TRIGGER_ORDER )]
	[Range( 0.1f, 2f, clamped: false )]
	[Feature( LOGIC ), Group( TRIGGER )]
	[ShowIf( nameof( HasInsideCondition ), true )]
	public virtual float InsideDelay { get; protected set; } = 0.5f;

	/// <summary>
	/// Logic that is ran when something enters the trigger.
	/// </summary>
	[Property]
	[InlineEditor]
	[Title( "On Enter" )]
	[WideMode( HasLabel = true )]
	[Order( LOGIC_FUNCTIONS_ORDER )]
	[Feature( LOGIC ), Group( FUNCTIONS )]
	[ShowIf( nameof( HasEnterCondition ), true )]
	protected virtual List<LogicAction> OnEnterLogic { get; set; }

	/// <summary>
	/// Logic that is ran when something leaves the trigger.
	/// </summary>
	[Property]
	[InlineEditor]
	[Title( "On Exit" )]
	[WideMode( HasLabel = true )]
	[Order( LOGIC_FUNCTIONS_ORDER )]
	[Feature( LOGIC ), Group( FUNCTIONS )]
	[ShowIf( nameof( HasExitCondition ), true )]
	protected virtual List<LogicAction> OnExitLogic { get; set; }

	/// <summary>
	/// Logic that is ran continuously while something is inside.
	/// </summary>
	[Property]
	[InlineEditor]
	[Title( "On Inside" )]
	[WideMode( HasLabel = true )]
	[Order( LOGIC_FUNCTIONS_ORDER )]
	[Feature( LOGIC ), Group( FUNCTIONS )]
	[ShowIf( nameof( HasInsideCondition ), true )]
	protected virtual List<LogicAction> OnInsideLogic { get; set; }

	/// <summary>
	/// If enabled: draw arrows towards targets while in play mode.
	/// </summary>
	[Property]
	[Order( LOGIC_DEBUG_ORDER )]
	[Title( "Targets (ingame)" )]
	[Feature( LOGIC ), Group( DEBUG )]
	protected bool DebugRenderTargetsInGame { get; set; }

	protected virtual Color ValidTargetColor => Color.White.WithAlpha( 0.3f );
	protected virtual Color InvalidTargetColor => Color.Red.Desaturate( 0.4f ).Darken( 0.1f ).WithAlpha( 0.3f );

	[Sync]
	public TimeUntil NextActivation { get; set; }

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( DebugRenderTargetsInGame )
			RenderTargets();

		if ( InGame )
			UpdateLogic();
	}

	protected override void DrawGizmos()
	{
		base.DrawGizmos();

		if ( !Gizmo.IsSelected )
			return;

		if ( !DebugRenderTargetsInGame )
			RenderTargets();
	}

	protected virtual void UpdateLogic()
	{
		if ( !HasInsideCondition )
			return;

		if ( !NextActivation )
			return;

		OnLogicInside();
	}

	protected override void OnTouchStart( GameObject obj )
	{
		base.OnTouchStart( obj );

		if ( Conditions.HasEnter() )
			OnLogicEnter();
	}

	protected override void OnTouchStop( GameObject obj )
	{
		base.OnTouchStop( obj );

		if ( Conditions.HasExit() )
			OnLogicExit();
	}

	/// <summary>
	/// Called when any logic has been activated at all.
	/// </summary>
	protected virtual void OnLogicActivated()
	{
		if ( SelfDestruct )
			GameObject.Destroy();
	}

	protected virtual void OnLogicEnter()
	{
		if ( LogicAction.TryExecute( OnEnterLogic, this ) )
			OnLogicActivated();
	}

	protected virtual void OnLogicExit()
	{
		if ( LogicAction.TryExecute( OnExitLogic, this ) )
			OnLogicActivated();
	}

	protected virtual void OnLogicInside()
	{
		if ( Touching is null || Touching.Count <= 0 )
			return;

		NextActivation = InsideDelay;

		if ( LogicAction.TryExecute( OnInsideLogic, this ) )
			OnLogicActivated();
	}

	// TODO: Debug rendering in the LogicAction struct.
	protected virtual void RenderTargets()
	{
		if ( Conditions.HasEnter() )
			RenderLogic( OnEnterLogic );

		if ( Conditions.HasExit() )
			RenderLogic( OnExitLogic );
	}

	protected virtual void RenderLogic( IEnumerable<LogicAction> list )
	{
		if ( list is null )
			return;

		foreach ( var logic in list )
		{
			// Logic activation.
			if ( logic.Type is LogicAction.ActionType.Activate )
			{
				if ( logic.ActivationTargets is null )
					continue;

				foreach ( var i in logic.ActivationTargets )
				{
					if ( i == this )
						continue;

					var iPos = (i as Entity)?.Center
						?? (i as Component)?.WorldPosition
						?? (i as GameObject)?.WorldPosition;

					if ( iPos is not Vector3 targetPos )
						continue;

					var isValid = i?.CanActivate( this ) is true;

					var color = isValid
						? ValidTargetColor
						: InvalidTargetColor;

					this.DrawArrow( Center, targetPos, color, len: 12f, w: 4f, tWorld: global::Transform.Zero );
				}
			}

			// Logic activation.
			if ( logic.Type is LogicAction.ActionType.Toggle )
			{
				if ( logic.ToggleTargets is null )
					continue;

				foreach ( var i in logic.ToggleTargets )
				{
					if ( i == this )
						continue;

					var iPos = (i as Entity)?.Center
						?? (i as Component)?.WorldPosition
						?? (i as GameObject)?.WorldPosition;

					if ( iPos is not Vector3 targetPos )
						continue;

					var b = logic.ToggleCommand.Apply( i.IsOn );
					var isValid = i?.CanToggle( b ) is true;

					var color = isValid
						? ValidTargetColor
						: InvalidTargetColor;

					this.DrawArrow( Center, targetPos, color, len: 12f, w: 4f, tWorld: global::Transform.Zero );
				}
			}
		}
	}
}
