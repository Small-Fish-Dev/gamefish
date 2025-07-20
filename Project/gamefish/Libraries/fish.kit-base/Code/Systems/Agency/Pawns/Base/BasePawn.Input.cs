namespace GameFish;

partial class BasePawn : IOperate
{
	/// <returns> If this pawn should listen to the local client's input. </returns>
	public virtual bool AllowInput()
	{
		if ( !Agent.IsValid() )
			return false;

		if ( Agent.IsPlayer )
			return Agent == Client.Local;

		return false;
	}

	public virtual bool CanOperate()
		=> AllowInput();

	public virtual void FrameOperate( in float deltaTime ) { }
	public virtual void FixedOperate( in float deltaTime ) { }
}
