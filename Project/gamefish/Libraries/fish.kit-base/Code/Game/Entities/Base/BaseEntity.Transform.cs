namespace GameFish;

partial class BaseEntity : ITransform
{
	public virtual Vector3 Center => WorldPosition;
}
