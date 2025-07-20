namespace GameFish;

partial class BaseEntity : ITransform
{
    public Vector3 GetPosition() => WorldPosition;
    public Rotation GetRotation() => WorldRotation;
    public Vector3 GetScale() => WorldScale;

    public Vector3 Center => GetPosition();

    public void SetPosition( in Vector3 newPos ) => WorldPosition = newPos;
    public void SetRotation( in Rotation rNew ) => WorldRotation = rNew;
    public void SetScale( in Vector3 newScale ) => WorldScale = newScale;
}
