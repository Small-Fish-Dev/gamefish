using System;
using System.Text.Json.Serialization;

namespace GameFish;

/// <summary>
/// A helper for interacting with objects and logical components.
/// <br /> <br />
/// <b> NOTE: </b> Make a list of this type on a component for really easy callbacks.
/// </summary>
public struct LogicAction
{
	[DefaultValue( Activate )]
	public enum ActionType
	{
		/// <summary>
		/// Triggers <see cref="IActivate"/>(s).
		/// </summary>
		[Icon( "⚡" )] Activate,

		/// <summary>
		/// Flips <see cref="IToggle"/>(s).
		/// </summary>
		[Icon( "♻" )] Toggle,

		/// <summary>
		/// Erases object(s).
		/// </summary>
		[Icon( "💥" )] Destroy,

		/// <summary>
		/// Runs custom actions.
		/// </summary>
		[Icon( "👨‍💻" )] Script,
	}

	/// <inheritdoc cref="NetworkRealm" />
	[EnumButtonGroup]
	[WideMode( HasLabel = false )]
	public NetworkRealm Realm { get; set; } = NetworkRealm.Owner;

	[EnumButtonGroup]
	[WideMode( HasLabel = false )]
	public ActionType Type { get; set; } = ActionType.Activate;

	[Group( TARGETS )]
	[Title( "Targets" )]
	[WideMode( HasLabel = false )]
	[ShowIf( nameof( IsActivating ), true )]
	public List<IActivate> ActivationTargets { get; set; } = [null];

	[WideMode( HasLabel = false )]
	[ShowIf( nameof( IsToggling ), true )]
	public ToggleCommand ToggleCommand { get; set; } = ToggleCommand.Disable;

	[Group( TARGETS )]
	[Title( "Targets" )]
	[WideMode( HasLabel = false )]
	[ShowIf( nameof( IsToggling ), true )]
	public List<IToggle> ToggleTargets { get; set; } = [null];

	[Group( TARGETS )]
	[Title( "Targets" )]
	[WideMode( HasLabel = false )]
	[ShowIf( nameof( IsObjecting ), true )]
	public List<GameObject> TargetObjects { get; set; } = [null];

	[WideMode( HasLabel = true )]
	[ShowIf( nameof( IsScripting ), true )]
	public Action<GameObject> Actions { get; set; }

	[InlineEditor]
	[WideMode( HasLabel = true )]
	[Doo.ArgumentHint<object>( "obj" )]
	[ShowIf( nameof( IsScripting ), true )]
	public Doo Doo { get; set; }

	[Hide, JsonIgnore]
	private readonly bool IsActivating => Type is ActionType.Activate;

	[Hide, JsonIgnore]
	private readonly bool IsToggling => Type is ActionType.Toggle;

	[Hide, JsonIgnore] // buh
	private readonly bool IsObjecting => Type is ActionType.Destroy or ActionType.Script;

	[Hide, JsonIgnore]
	private readonly bool IsScripting => Type is ActionType.Script;

	public LogicAction() { }

	public LogicAction( in ActionType type )
	{
		Type = type;
	}

	/// <returns> If this logic is meant to execute for this system. </returns>
	public readonly bool InRealm( object source )
		=> Realm.InRealm( source );

	/// <summary>
	/// Safely executes every action in a set. Logs from <paramref name="source"/> upon any exception.
	/// </summary>
	public static bool TryExecute( IEnumerable<LogicAction> list, object source, object value = null )
	{
		if ( list is null )
			return false;

		var isEffective = false;

		foreach ( var logic in list )
			if ( logic.TryExecute( source: source ?? typeof( LogicAction ), value ) )
				isEffective = true;

		return isEffective;
	}

	public readonly bool TryExecute( object source, object value = null )
	{
		if ( !InRealm( source ) )
			return false;

		switch ( Type )
		{
			case ActionType.Activate:
				TriggerActivation( source, value );
				break;

			case ActionType.Toggle:
				TriggerToggling( source );
				break;

			case ActionType.Destroy:
				TriggerDestruction( source );
				break;

			case ActionType.Script:
				TriggerScripting( source );
				TriggerDoo( source );
				break;

			default:
				return false;
		}

		return true;
	}

	private readonly void TriggerActivation( object source = null, object value = null )
	{
		if ( ActivationTargets is null )
			return;

		foreach ( var tgt in ActivationTargets )
		{
			try
			{
				if ( tgt is null )
					continue;

				if ( tgt == source )
				{
					Print.WarnFrom( source ?? this, $"{nameof( ActionType.Activate )} tried to self-target! Potential infinite loop prevented." );
					continue;
				}

				if ( tgt is Component c && c.IsValid() )
					if ( tgt.CanActivate( source: source ) )
						tgt.TryActivate( source: source, value: value );
			}
			catch ( Exception e )
			{
				Print.WarnFrom( source ?? this, $"{nameof( ActionType.Activate )} exception: {e}" );
			}
		}
	}

	private readonly void TriggerToggling( object source = null )
	{
		if ( ToggleTargets is null )
			return;

		foreach ( var tgt in ToggleTargets )
		{
			try
			{
				if ( tgt is null )
					continue;

				if ( tgt == source )
				{
					Print.WarnFrom( source ?? this, $"{nameof( ActionType.Toggle )} tried to self-target! Potential infinite loop prevented." );
					continue;
				}

				if ( tgt is Component c && c.IsValid() )
					tgt.TryToggle( ToggleCommand );
			}
			catch ( Exception e )
			{
				Print.WarnFrom( source ?? this, $"{nameof( ActionType.Toggle )} exception: {e}" );
			}
		}
	}

	private readonly void TriggerDestruction( object source = null )
	{
		if ( TargetObjects is null )
			return;

		foreach ( var obj in TargetObjects )
		{
			try
			{
				if ( !obj.IsValid() )
					continue;

				obj.Destroy();
			}
			catch ( Exception e )
			{
				Print.WarnFrom( source ?? this, $"{nameof( ActionType.Destroy )} exception: {e}" );
			}
		}
	}

	private readonly void TriggerScripting( object source = null )
	{
		if ( Actions is null )
			return;

		if ( TargetObjects is null )
			return;

		foreach ( var obj in TargetObjects )
		{
			try
			{
				if ( obj.IsValid() )
					Actions.Invoke( obj );
			}
			catch ( Exception e )
			{
				Print.WarnFrom( source ?? this, $"{nameof( ActionType.Script )} exception: {e}" );
			}
		}
	}

	private readonly void TriggerDoo( object source = null )
	{
		if ( Doo is null || Doo.IsEmpty() )
			return;

		if ( source is not Component c || !c.IsValid() )
			return;

		try
		{
			c.RunDoo( Doo, a => a.SetArgument( "obj", source ) );
		}
		catch ( Exception e )
		{
			Print.WarnFrom( source ?? this, $"{nameof( ActionType.Script )} exception: {e}" );
		}
	}
}
