namespace GameFish;

partial class Pawn : ISimulate
{
	/// <returns> If this pawn should listen to the local client's inputs(like button presses). </returns>
	public virtual bool AllowInput()
		=> Owner.IsValid() && Owner == Client.Local;

	public virtual bool CanSimulate()
		=> AllowInput();

	public virtual void FrameSimulate( in float deltaTime )
	{
		UpdateInput( in deltaTime );

		Move( in deltaTime, isFixedUpdate: false );

		// Update view after parent transforms change.
		UpdateView( in deltaTime );
	}

	public virtual void FixedSimulate( in float deltaTime )
	{
	}

	/// <summary>
	/// Processes inputs.
	/// You should run this before steps like movement.
	/// </summary>
	protected virtual void UpdateInput( in float deltaTime )
	{
		DoAiming( in deltaTime );
	}

	/// <summary>
	/// This is where the pawn's aim should be affected.
	/// </summary>
	protected virtual void DoAiming( in float deltaTime )
	{
		if ( Controller.IsValid() && IsPlayer )
			Controller.TryAim( Input.AnalogLook, in deltaTime );
	}
}
