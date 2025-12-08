namespace GameFish;

/// <summary>
/// Instructs the <see cref="ModularActor"/> how to act at a given moment. <br />
/// Typically only one behavior is simulated(active) at a time.
/// </summary>
public abstract partial class ActorBehavior : ActorModule
{
	public virtual TagSet BehaviorTags { get; }

	/// <summary>
	/// Is this the currently selected behavior?
	/// </summary>
	public bool IsSelected => Mind?.Behavior == this;

	/// <summary>
	/// Is this the currently selected behavior?
	/// </summary>
	[Property]
	[Feature( ACTOR )]
	public virtual bool IsDefault { get; set; }

	/// <summary>
	/// Called when this behavior is set as the active one.
	/// </summary>
	/// <param name="oldBehavior"> The previous behavior(if any). </param>
	public virtual void OnSelect( ActorBehavior oldBehavior = null )
	{
	}

	/// <summary>
	/// Called when this behavior is no longer active.
	/// </summary>
	/// <param name="newBehavior"> The next behavior(if any). </param>
	public virtual void OnDeselect( ActorBehavior newBehavior = null )
	{
	}

	/// <summary>
	/// Tries to end this performance.
	/// </summary>
	/// <returns> If the behavior was changed. </returns>
	public virtual bool TryStop()
		=> Mind?.SelectDefaultBehavior( out _ ) is true;


	/// <returns> If we can perform at this time. </returns>
	public virtual bool CanPerform()
		=> CanSimulate();

	/// <summary>
	/// Executes this behavior.
	/// </summary>
	public abstract void Perform( in float deltaTime );
}
