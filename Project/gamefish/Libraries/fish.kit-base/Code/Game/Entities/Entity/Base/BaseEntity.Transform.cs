namespace GameFish;

partial class BaseEntity : ITransform
{
	public Vector3 GetPosition() => WorldPosition;
	public Rotation GetRotation() => WorldRotation;
	public Vector3 GetScale() => WorldScale;

	public virtual Vector3 Center => GetPosition();

	public virtual bool TrySetPosition( in Vector3 newPos )
	{
		if ( !ITransform.ValidVector( newPos ) )
			return false;

		WorldPosition = newPos;
		return true;
	}

	public virtual bool TrySetRotation( in Rotation rNew )
	{
		if ( !ITransform.ValidRotation( rNew ) )
			return false;

		WorldRotation = rNew;
		return true;
	}

	public virtual bool TrySetScale( in Vector3 newScale )
	{
		if ( !ITransform.ValidVector( newScale ) )
			return false;

		WorldScale = newScale;
		return true;
	}
}
