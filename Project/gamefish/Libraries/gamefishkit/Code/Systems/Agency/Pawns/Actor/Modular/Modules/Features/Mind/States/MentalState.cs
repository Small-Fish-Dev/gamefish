using System.Text.Json.Serialization;

namespace GameFish;

/// <summary>
/// Determines what behaviors an <see cref="Actor"/> should be driving and how.
/// <br />
/// <b>Example:</b> a fighting state that uses the chasing behavior.
/// </summary>
public abstract partial class MentalState : ActorModule
{
	/// <summary>
	/// Is this valid as a default/fallback state?
	/// </summary>
	[Property, ReadOnly, JsonIgnore]
	[Feature( MIND ), Order( -1000 )]
	public virtual bool IsDefault { get; } = false;

	[Property, JsonIgnore]
	[Feature( MIND ), Group( DISPLAY ), Order( -900 )]
	public string Name { get; } = "Mental State";

	[Property, JsonIgnore]
	[Feature( MIND ), Group( DISPLAY )]
	public virtual string Description { get; } = "Thinks.";

	/// <summary>
	/// The mental state to default to upon end/expiration.
	/// </summary>
	[Property]
	[Feature( MIND ), Group( LOGIC )]
	public MentalState NextState { get; set; }

	[Sync] public Area? PatrolArea { get; set; }

	public bool IsPatroling => PatrolArea.HasValue; // && ;

	public override bool IsParent( ModuleEntity comp )
		=> comp is ActorMind;

	/// <summary>
	/// This mind state has been selected.
	/// </summary>
	/// <param name="oldState"> The previous state(if any). </param>
	public virtual void OnSelect( MentalState oldState = null )
	{
	}

	/// <summary>
	/// The mind's state has been changed from this one.
	/// </summary>
	/// <param name="newState"> The next state(if any). </param>
	public virtual void OnDeselect( MentalState newState = null )
	{
	}

	/// <summary>
	/// Tries to change this state to something else.
	/// </summary>
	/// <returns> If the state could be changed. </returns>
	protected bool TrySetState( MentalState state )
		=> Mind?.TrySetState( state ) is true;

	public virtual bool TryEnd()
		=> Mind?.TrySetState( NextState.IsValid() ? NextState : Mind?.DefaultState ) is true;


	/// <summary>
	/// Thinks its little thoughts.
	/// </summary>
	public virtual void Think( in float deltaTime, in bool isFixedUpdate )
	{
	}
}
