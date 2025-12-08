namespace GameFish;

/// <summary>
/// Allows you to and manage forces easily on whatever.
/// </summary>
public interface IVelocity
{
	public Vector3 Velocity { get; set; }

	/// <summary>
	/// Attempts to push this physics object.
	/// </summary>
	/// <returns> If we were allowed to send this impulse. </returns>
	public virtual bool TryImpulse( in Vector3 vel )
	{
		if ( !ITransform.IsValid( in vel ) )
			return false;

		SendImpulse( vel );
		return true;
	}

	/// <summary>
	/// Networks impulse velocity to the owner.
	/// </summary>
	public void SendImpulse( Vector3 vel );
}
