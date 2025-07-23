namespace GameFish;

/// <summary>
/// A simple pawn that uses the built-in <see cref="PlayerController"/>.
/// </summary>
public abstract partial class ControllerPawn : BasePawn
{
	/// <summary>
	/// A player-intended mobile extension of will.
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

	public virtual Vector3 WishVelocity
	{
		get => _pc.IsValid() ? _pc.WishVelocity : default;
		set { if ( _pc.IsValid() ) _pc.WishVelocity = value; }
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		var allowInput = AllowInput();

		if ( Controller.IsValid() )
			Controller.UseInputControls = allowInput;
	}

	protected override void OnSetOwner( Agent old, Agent agent )
	{
		base.OnSetOwner( old, agent );

		if ( !agent.IsValid() )
			WishVelocity = default;
	}

	public override void FrameOperate( in float deltaTime )
	{
		base.FrameOperate( deltaTime );
	}

	public override void FixedOperate( in float deltaTime )
	{
		base.FixedOperate( deltaTime );
	}
}
