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
	public bool TryImpulse( in Vector3 vel, in Vector3? point = null );
}
