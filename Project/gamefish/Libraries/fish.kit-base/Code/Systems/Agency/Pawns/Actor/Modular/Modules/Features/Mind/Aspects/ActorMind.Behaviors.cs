namespace GameFish;

partial class ActorMind
{
	public IEnumerable<ActorBehavior> Behaviors
		=> Actor?.GetModules<ActorBehavior>() ?? [];

	/// <summary>
	/// The actively selected behavior(if any).
	/// </summary>
	[Sync]
	public new ActorBehavior Behavior { get; set; }

	public virtual void PerformBehavior( in float deltaTime )
	{
		if ( IsProxy || Behaviors is null )
			return;

		// Select the default behavior if we don't have one.
		if ( !Behavior.IsValid() && !SelectDefaultBehavior( out _ ) )
			return;

		if ( Behavior.IsValid() && Behavior.CanPerform() )
			Behavior.Perform( in deltaTime );
	}

	public virtual bool SelectDefaultBehavior( out ActorBehavior result )
	{
		if ( TrySelectBehavior( result = Mind?.DefaultBehavior ) )
			return true;

		var defaults = Behaviors
			.Where( act => act.IsDefault );

		if ( !defaults.Any() )
			return false;

		return TrySelectBehavior( defaults.PickRandom() );
	}

	public virtual bool TrySelectBehavior( ActorBehavior behavior )
	{
		if ( !behavior.IsValid() || IsProxy )
			return false;

		if ( Behavior == behavior )
			return true;

		Behavior = behavior;

		return true;
	}
}
