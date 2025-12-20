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
		get => _pc.GetCached( GameObject );
		set { _pc = value; }
	}

	protected PlayerController _pc;

	protected override void OnStart()
	{
		base.OnStart();

		if ( !PlayerController.IsValid() )
		{
			this.Warn( "needs a PlayerController to function!" );
			return;
		}

		PlayerController.UseCameraControls = false;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( !PlayerController.IsValid() )
			return;

		var allowMoving = Pawn.IsValid() && Pawn.AllowInput()
			&& Client.TryGetLocalMove( out _ );

		PlayerController.UseInputControls = allowMoving;
		PlayerController.UseCameraControls = false;
	}

	public override void SetLocalEyePosition( Vector3 pos ) { }

	protected override void OnSetLocalEyeRotation( in Rotation r )
	{
		if ( PlayerController.IsValid() )
			PlayerController.EyeAngles = r;
	}

	public override Rotation GetLocalEyeRotation()
	{
		if ( PlayerController.IsValid() )
			return PlayerController.EyeAngles;

		return base.GetLocalEyeRotation();
	}

	public override Vector3 GetLocalEyePosition()
	{
		if ( PlayerController.IsValid() )
			return WorldTransform.PointToLocal( PlayerController.EyePosition );

		return base.GetLocalEyePosition();
	}

	// The engine's controller handles this stuff.
	public override bool TryMove( in float deltaTime, in bool isFixedUpdate, in Vector3 wishVel = default )
		=> false;

	protected override void Move( in float deltaTime ) { }
	protected override void PreMove( in float deltaTime ) { }
	protected override void PostMove( in float deltaTime ) { }
}
