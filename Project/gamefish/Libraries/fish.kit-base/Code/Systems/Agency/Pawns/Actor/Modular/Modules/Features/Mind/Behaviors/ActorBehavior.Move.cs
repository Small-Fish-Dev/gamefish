namespace GameFish;

partial class ActorBehavior
{
	/// <summary>
	/// Called just before movement logic is performed to determine the wish velocity.
	/// </summary>
	/// <param name="deltaTime"></param>
	/// <param name="dest"> The place we're trying to move to(or null). </param>
	/// <param name="speed"> The target movement speed. </param>
	/// <param name="wishVel"> The final movement velocity. </param>
	public virtual void PreMove( in float deltaTime, in Vector3? dest, in float speed, ref Vector3 wishVel )
	{
	}

	/// <summary>
	/// Lets the behavior decide where it wants to travel.
	/// </summary>
	/// <param name="equip"> The equipment we'd be moving with(or our active one). </param>
	/// <returns> The place we should be moving to(if any). </returns>
	public virtual Vector3? GetDestination( Equipment equip = null )
		=> null;
}
