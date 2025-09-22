namespace GameFish;

partial class BasePawn : ISimulate
{
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
		UpdateController( in deltaTime );
		UpdateView( in deltaTime );
	}

	public virtual void FixedSimulate( in float deltaTime )
	{
	}
}
