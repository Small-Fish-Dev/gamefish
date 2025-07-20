namespace GameFish;

public interface ITransform
{
    public Vector3 Position { get => GetPosition(); set => TrySetPosition( in value ); }
    public Rotation Rotation { get => GetRotation(); set => TrySetRotation( in value ); }
    public Vector3 Scale { get => GetScale(); set => TrySetScale( in value ); }

    /// <returns> The transform's world position. </returns>
    public Vector3 GetPosition();
    public Rotation GetRotation();
    public Vector3 GetScale();

    /// <summary>
    /// The center of the object, such as its mass center or hull center. <br />
    /// Can be used to avoid calculating its bounds every time(expensive!).
    /// </summary>
    public Vector3 Center => GetPosition();

    /// <summary> Sets position directly. Use <see cref="TrySetPosition"/> to do so safely. </summary>
    protected void SetPosition( in Vector3 newPos );
    /// <summary> Sets rotation directly. Use <see cref="TrySetRotation"/> to do so safely. </summary>
    protected void SetRotation( in Rotation rNew );
    /// <summary> Sets scale directly. Use <see cref="TrySetScale"/> to do so safely. </summary>
    protected void SetScale( in Vector3 newScale );

    private static bool Valid( in float n )
        => !float.IsNaN( n ) && !float.IsInfinity( n );

    public static bool ValidVector( in Vector3 v ) => !v.IsNaN && !v.IsInfinity;
    public static bool ValidRotation( in Rotation r ) => Valid( r.x ) && Valid( r.y ) && Valid( r.z ) && Valid( r.w );

    /// <summary>
    /// Safely sets the position.
    /// </summary>
    /// <returns> If the position wasn't a NaN or infinity. </returns>
    public bool TrySetPosition( in Vector3 newPos )
    {
        if ( !ValidVector( newPos ) )
            return false;

        SetPosition( newPos );
        return true;
    }

    /// <summary>
    /// Safely sets the rotation.
    /// </summary>
    /// <returns> If the rotation wasn't a NaN or infinity. </returns>
    public bool TrySetRotation( in Rotation rNew )
    {
        if ( !ValidRotation( rNew ) )
            return false;

        SetRotation( rNew );
        return true;
    }

    /// <summary>
    /// Safely sets the scale.
    /// </summary>
    /// <returns> If the scale wasn't a NaN or infinity. </returns>
    public bool TrySetScale( in Vector3 newScale )
    {
        if ( !ValidVector( newScale ) )
            return false;

        SetScale( newScale );
        return true;
    }
}
