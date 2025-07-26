namespace GameFish;

partial class BasePawn : ISimulate
{
	public const string INPUT = "🕹 Input";

	public virtual Vector3 EyePosition { get => WorldPosition; set => WorldPosition = value; }
	public virtual Rotation EyeRotation { get => WorldRotation; set => WorldRotation = value; }
	public Transform EyeTransform => new( EyePosition, EyeRotation, 1f );
	public Vector3 EyeForward => EyeRotation.Forward;

	public virtual Vector3 WishVelocity
	{
		get => _wishVelocity;
		set => _wishVelocity = value;
	}

	protected Vector3 _wishVelocity;

	/// <returns> If this pawn should listen to the local client's input. </returns>
	public virtual bool AllowInput()
	{
		if ( !Agent.IsValid() )
			return false;

		if ( Agent.IsPlayer )
			return Agent == Client.Local;

		return false;
	}

	public virtual bool CanSimulate()
		=> AllowInput();

	public virtual void FrameSimulate( in float deltaTime )
	{
		View?.FrameSimulate( Time.Delta );
	}

	public virtual void FixedSimulate( in float deltaTime )
	{

	}
}
