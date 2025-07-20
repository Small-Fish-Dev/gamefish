namespace GameFish;

/// <summary>
/// Allows you to and manage forces easily on whatever.
/// </summary>
public interface IVelocity
{
    /// <summary>
    /// The velocity this object current has, though <see cref="GetVelocity"/> may give you a modified value.
    /// </summary>
    public virtual Vector3 Velocity { get => GetVelocity(); set => SetVelocity( value ); }

    /// <returns> Gets the velocity and its factors. </returns>
    public Vector3 GetVelocity();

    /// <summary>
    /// Sets the velocity in a way that lets the object process it.
    /// </summary>
    public void SetVelocity( in Vector3 vel );

    /// <summary>
    /// Tries to modify the velocity. Lets the object modify the result.
    /// </summary>
    /// <returns> If this object allows adding of the velocity. </returns>
    public bool TryModifyVelocity( in Vector3 vel, out Vector3 result )
    {
        if ( !ITransform.ValidVector( vel ) )
        {
            result = GetVelocity();
            return false;
        }

        SetVelocity( vel );
        result = GetVelocity();

        return true;
    }

    /// <summary>
    /// Tries to modify the velocity. Lets the object modify the result.
    /// </summary>
    /// <returns> If this object allows adding of the velocity. </returns>
    public bool TryModifyVelocity( in Vector3 vel )
        => TryModifyVelocity( vel, out _ );

    /// <summary>
    /// Put your movement/physics code here.
    /// </summary>
    public void ApplyVelocity( in float deltaTime )
    {
    }
}
