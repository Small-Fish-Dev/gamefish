namespace GameFish;

partial class Pawn : ISimulate
{
	/*
	/// <summary>
	/// The thing we're currently focusing on.
	/// </summary>
	[Title( "Focus" )]
	[Property, JsonIgnore]
	[Feature( PAWN ), Group( INPUT )]
	protected DynamicEntity InspectorFocus
	{
		get => FocusEntity;
		set => FocusEntity = value;
	}

	/// <summary>
	/// The thing we're currently focusing on.
	/// </summary>
	[Sync]
	public DynamicEntity FocusEntity { get; set; }
	*/

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
		if ( !IsPlayer )
			return;

		if ( !Client.TryGetLocalAim( out var aim ) )
			return;

		if ( Controller.IsValid() )
			Controller.TryAim( aim, in deltaTime );
	}
}
