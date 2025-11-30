using System;

namespace GameFish;

public partial class FacepunchController : BaseController
{
	/// <summary>
	/// The unfortunately less than ideal built-in controller.
	/// </summary>
	[Property]
	[Feature( PAWN )]
	public PlayerController PlayerController
	{
		get => _pc.IsValid() ? _pc
			: _pc = Components?.Get<PlayerController>( FindMode.EverythingInSelf );

		set { _pc = value; }
	}

	protected PlayerController _pc;

	public override Vector3 GetLocalEyePosition()
	{
		if ( PlayerController.IsValid() )
			return WorldTransform.PointToLocal( PlayerController.EyePosition );

		return base.GetLocalEyePosition();
	}

	protected override void OnStart()
	{
		base.OnStart();

		if ( !PlayerController.IsValid() )
			this.Warn( "needs a PlayerController to function!" );
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		// Toggle the FP controller's input system with respect to our agency system.
		if ( PlayerController is PlayerController c && c.IsValid() )
		{
			c.UseInputControls = Pawn is BasePawn pawn
				&& pawn.IsValid() && pawn.AllowInput();

			LocalEyeRotation = WorldTransform.RotationToLocal( c.EyeAngles );
		}
	}

	// The engine's controller handles this stuff.
	protected override void Move( in float deltaTime ) { }
	protected override void PreMove( in float deltaTime ) { }
	protected override void PostMove( in float deltaTime ) { }
}
