using System.Text.Json.Serialization;

namespace GameFish;

/// <summary>
/// A mental state for an <see cref="Actor"/> related to combat.
/// </summary>
public abstract partial class CombatState : MentalState
{
	public enum CombatType
	{
		Idle,
		Alert,
		Fighting
	}

	/// <summary>
	/// If this is for idling, being alerted or fighting.
	/// </summary>
	[Property, ReadOnly, JsonIgnore]
	[Feature( MIND ), Group( COMBAT )]
	protected virtual CombatType InspectorState => Type;

	/// <summary>
	/// If this is for idling, being alerted or fighting.
	/// </summary>
	public abstract CombatType Type { get; }

	/// <summary>
	/// Gets a set of every combat state this <see cref="Actor"/> has.
	/// </summary>
	protected IEnumerable<CombatState> States => Actor?.GetModules<CombatState>() ?? [];

	/// <returns> The first occurence of <typeparamref name="TState"/>. </returns>
	protected TState GetType<TState>() where TState : CombatState
		=> States.Select( m => m as TState )
			.Where( m => m.IsValid() )
			.FirstOrDefault();

	public virtual bool TryIdle()
		=> TrySetState( GetType<IdleCombatState>() );

	public virtual bool TryAlert()
		=> TrySetState( GetType<AlertCombatState>() );

	public virtual bool TryFighting()
		=> TrySetState( GetType<FightingCombatState>() );

	public virtual void OnSetTarget( Pawn target )
	{
	}

	public override void OnTargetVisible( Pawn target, in Vector3? at = null )
	{
		if ( target.IsValid() )
			TryFighting();
	}
}
