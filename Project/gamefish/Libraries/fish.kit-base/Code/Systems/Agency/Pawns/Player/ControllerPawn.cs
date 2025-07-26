namespace GameFish;

/// <summary>
/// A simple pawn that uses the built-in <see cref="PlayerController"/>.
/// </summary>
public abstract partial class ControllerPawn : BasePawn
{
	/// <summary>
	/// The unfortunately less than ideal built-in controller.
	/// </summary>
	[Property]
	[Feature( FEATURE_PAWN )]
	public PlayerController Controller
	{
		get => _pc.IsValid() ? _pc
			: _pc = Components?.Get<PlayerController>( FindMode.EverythingInSelf );

		set { _pc = value; }
	}

	protected PlayerController _pc;

	public override Vector3 EyePosition
	{
		get => Controller?.EyePosition ?? WorldTransform.PointToWorld( Vector3.Up * 64f );
		set
		{
			// Why the fuck not? WHY NOT? Fuck this.
			// if ( Controller.IsValid() )
			// Controller.EyePosition = value;
		}
	}

	public override Rotation EyeRotation
	{
		get => View?.EyeRotation ?? base.EyeRotation;
		set
		{
			if ( View.IsValid() )
				View.EyeRotation = value;

			if ( Controller.IsValid() )
				Controller.EyeAngles = value;
		}
	}

	public override Vector3 WishVelocity
	{
		get => Controller.IsValid() ? _pc.WishVelocity : default;
		set { if ( _pc.IsValid() ) _pc.WishVelocity = value; }
	}

	protected override void OnSetOwner( Agent old, Agent agent )
	{
		base.OnSetOwner( old, agent );

		// TEMP?
		if ( !agent.IsValid() )
			WishVelocity = default;
	}

	public override void FrameSimulate( in float deltaTime )
	{
		base.FrameSimulate( deltaTime );

		if ( Controller.IsValid() && View.IsValid() )
			Controller.EyeAngles = View.GetViewTransform().Rotation;
	}

	public override void FixedSimulate( in float deltaTime )
	{
		base.FixedSimulate( deltaTime );
	}
}
