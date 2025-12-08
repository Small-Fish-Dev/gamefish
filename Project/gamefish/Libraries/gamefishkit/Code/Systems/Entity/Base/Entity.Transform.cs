namespace GameFish;

partial class Entity : ITransform
{
	public virtual Vector3 Center => WorldPosition;
}
