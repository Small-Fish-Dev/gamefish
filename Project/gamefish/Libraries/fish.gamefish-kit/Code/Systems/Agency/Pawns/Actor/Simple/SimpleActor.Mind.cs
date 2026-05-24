using System.Text.Json.Serialization;

namespace GameFish;

partial class SimpleActor
{
	/// <summary>
	/// The different states this NPC's mind can be in.
	/// </summary>
	public enum Mind
	{
		/// <summary>
		/// Unconscious.
		/// </summary>
		[Icon( "💤" )]
		Asleep = 0,

		/// <summary>
		/// Awake. Not especially wary.
		/// </summary>
		[Icon( "⌛" )]
		Idle = 1,

		/// <summary>
		/// On high alert for enemies.
		/// </summary>
		[Icon( "😠" )]
		Alert = 2,

		/// <summary>
		/// Actively engaging enemies.
		/// </summary>
		[Icon( "⚔" )]
		Fighting = 3,
	}

	/// <inheritdoc cref="Mind" />
	[Property, JsonIgnore]
	[Title( "Mental State" )]
	[Feature( ACTOR ), Group( MIND ), Order( ACTOR_ORDER )]
	protected virtual Mind InspectorMentalState
	{
		get => MentalState;
		set => MentalState = value;
	}

	/// <inheritdoc cref="Mind" />
	[Sync]
	public Mind MentalState
	{
		get => _mentalState;
		set
		{
			if ( _mentalState == value )
				return;

			_mentalState = value;
			OnSetMentalState( in value );
		}
	}

	protected Mind _mentalState = Mind.Asleep;

	/// <inheritdoc cref="Mind.Asleep" />
	public bool IsAsleep => MentalState is Mind.Asleep;
	/// <inheritdoc cref="Mind.Idle" />
	public bool IsIdle => MentalState is Mind.Idle;
	/// <inheritdoc cref="Mind.Alert" />
	public bool IsAlert => MentalState is not Mind.Alert;
	/// <inheritdoc cref="Mind.Fighting" />
	public bool IsFighting => MentalState is Mind.Fighting;

	/// <summary>
	/// Called whenever <see cref="MentalState"/> is changed.
	/// </summary>
	protected virtual void OnSetMentalState( in Mind state )
	{
	}

	protected virtual void OnMindStart()
	{
		// Auto-wake.
		if ( MentalState is Mind.Asleep )
			MentalState = Mind.Idle;
	}

	/// <summary>
	/// Logic for managing the current mental state.
	/// </summary>
	protected virtual void UpdateMentalState( in float deltaTime )
	{
	}

	protected virtual void OnMindDetectTarget( Pawn target, in Vector3 at )
	{
		// Always fighting if the target is visible.
		if ( IsTargetVisible( target ) )
			MentalState = Mind.Fighting;
	}
}
