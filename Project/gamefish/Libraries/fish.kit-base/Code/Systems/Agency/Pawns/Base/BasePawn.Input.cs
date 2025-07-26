namespace GameFish;

partial class BasePawn : ISimulate
{
	public const string INPUT = "🕹 Input";

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
