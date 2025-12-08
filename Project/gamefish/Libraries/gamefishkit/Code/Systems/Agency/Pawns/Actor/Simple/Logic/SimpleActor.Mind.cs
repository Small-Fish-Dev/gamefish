using System.Text.Json.Serialization;

namespace GameFish;

partial class SimpleActor
{
	public enum MentalState
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

	[Property, JsonIgnore]
	[Title( "Mental State" )]
	[Feature( ACTOR ), Group( MIND ), Order( ACTOR_ORDER )]
	public virtual MentalState InspectorMentalState
	{
		get => MindState;
		set => MindState = value;
	}

	[Sync]
	public MentalState MindState
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

	protected MentalState _mentalState = MentalState.Asleep;

	/// <summary>
	/// Logic for managing the current mental state.
	/// </summary>
	protected virtual void UpdateMentalState( in float deltaTime )
	{
	}

	/// <summary>
	/// Called whenever <see cref="MindState"/> is changed.
	/// </summary>
	protected virtual void OnSetMentalState( in MentalState state )
	{
	}
}
