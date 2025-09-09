namespace GameFish;

/// <summary>
/// A shrimple pawn that uses the built-in <see cref="PlayerController"/>.
/// </summary>
public abstract partial class ControllerPawn : BasePawn
{
	/// <summary>
	/// The unfortunately less than ideal built-in controller.
	/// </summary>
	[Property]
	[Feature( PAWN )]
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
		get => Controller?.EyeAngles ?? base.EyeRotation;
		set
		{
			if ( Controller.IsValid() )
				Controller.EyeAngles = value;
		}
	}

	public override Vector3 WishVelocity
	{
		get => Controller.IsValid() ? _pc.WishVelocity : default;
		set { if ( _pc.IsValid() ) _pc.WishVelocity = value; }
	}

	public override void FrameSimulate( in float deltaTime )
	{
		base.FrameSimulate( deltaTime );

		if ( Controller.IsValid() && View.IsValid() )
			Controller.EyeAngles = View.ViewRotation;
	}

	public override void FixedSimulate( in float deltaTime )
	{
		base.FixedSimulate( deltaTime );
	}

	public override void OnSetLookRotation( in Rotation rLook )
	{
		base.OnSetLookRotation( rLook );

		if ( Controller.IsValid() )
			Controller.EyeAngles = rLook;
	}
}
