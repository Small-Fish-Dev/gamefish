namespace GameFish;

/// <summary>
/// Lets players use a grappling hook.
/// </summary>
[Title( "Grappling Hook" )]
public partial class PlayerGrappleModule : PlayerModule
{
	protected const int HOOK_ORDER = PLAYER_ORDER - 500;

	/// <summary>
	/// The speed to add when retracting.
	/// </summary>
	[Property]
	[Range( 0f, 1000f, clamped: false )]
	[Feature( HOOK ), Group( FORCES )]
	public float PullSpeed { get; set; } = 500f;

	/// <summary>
	/// Can only pull towards the hook point this fast.
	/// </summary>
	[Property]
	[Range( 0f, 2000f, clamped: false )]
	[Feature( HOOK ), Group( FORCES )]
	public float PullSpeedLimit { get; set; } = 500f;

	/// <summary>
	/// The maximum extra speed to add towards the point depending on slack length.
	/// </summary>
	[Property]
	[Range( 1f, 1000f, clamped: false )]
	[Feature( HOOK ), Group( FORCES )]
	public float Elasticity { get; set; } = 500f;

	[Property]
	[Range( 0f, 2f, clamped: false )]
	[Feature( HOOK ), Group( FORCES )]
	public float SwingSpeed { get; set; } = 0.75f;

	[Property]
	[Range( 0f, 1000f, clamped: false )]
	[Feature( HOOK ), Order( HOOK_ORDER )]
	public float RetractSpeed { get; set; } = 500f;

	[Property]
	[Range( 0f, 1000f, clamped: false )]
	[Feature( HOOK ), Order( HOOK_ORDER )]
	public float ExtendSpeed { get; set; } = 500f;

	[Property]
	[Feature( HOOK ), Order( HOOK_ORDER )]
	public FloatRange LengthRange { get; set; } = new( 16f, 4096f );

	[Property]
	[Range( 1f, 128f, clamped: false )]
	[Feature( HOOK ), Order( HOOK_ORDER )]
	public float SlackLimit { get; set; } = 64f;

	[Property]
	[InputAction]
	[Feature( HOOK ), Group( INPUT )]
	public string HookButton { get; set; } = "Hook";

	[Property]
	[InputAction]
	[Feature( HOOK ), Group( INPUT )]
	public string RetractButton { get; set; } = "Jump";

	[Property]
	[InputAction]
	[Feature( HOOK ), Group( INPUT )]
	public string ExtendButton { get; set; } = "Duck";

	public virtual Vector3 HookOrigin => Player.IsValid() ? Player.Center : WorldPosition;

	public virtual bool IsHooking
	{
		get => HitObject.IsValid();
		set
		{
			if ( value is false )
				HitObject = null;
		}
	}

	[Sync]
	public GameObject HitObject { get; set; }

	[Sync]
	public Vector3 LocalPoint { get; set; }

	[Sync]
	public virtual float Length
	{
		get => _length.Positive();
		set => _length = value.Clamp( LengthRange );
	}

	protected float _length;

	[Sync]
	public virtual bool IsRetracting { get; protected set; }

	[Sync]
	public virtual bool IsExtending { get; protected set; }

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( !IsProxy )
			Simulate( Time.Delta );
	}

	protected override void OnPreRender()
	{
		base.OnPreRender();

		if ( !HitObject.IsValid() || !Player.IsValid() )
			return;

		var origin = HookOrigin;
		var tWorld = HitObject.WorldTransform;
		var worldPoint = tWorld.PointToWorld( LocalPoint );

		this.DrawArrow( origin + Player.EyeForward * 10f, worldPoint,
			Color.Black, th: 4f, tWorld: global::Transform.Zero );
	}

	public virtual void Simulate( in float deltaTime )
	{
		if ( Input.Down( HookButton ) )
			TryHook();
		else
			IsHooking = false;

		if ( !IsHooking )
			return;

		if ( !TryGetHook( out var origin, out var worldPoint ) )
			return;

		IsRetracting = Input.Down( RetractButton );
		IsExtending = Input.Down( ExtendButton );

		if ( IsRetracting && !IsExtending )
		{
			Length -= RetractSpeed * deltaTime;
			Length = Length.Min( origin.Distance( worldPoint ) );
		}
		else if ( IsExtending && !IsRetracting )
		{
			Length += ExtendSpeed * deltaTime;
		}

		HookMove( in deltaTime );
	}

	public virtual bool IsValidTarget( in SceneTraceResult tr )
	{
		if ( !tr.Hit || !tr.GameObject.IsValid() )
			return false;

		return true;
	}

	public virtual bool TryGetHook( out Vector3 origin, out Vector3 worldPoint )
	{
		if ( !HitObject.IsValid() )
		{
			origin = default;
			worldPoint = default;
			return false;
		}

		origin = HookOrigin;

		var tWorld = HitObject.WorldTransform;
		worldPoint = tWorld.PointToWorld( LocalPoint );

		return true;
	}

	protected virtual bool TryHook()
	{
		if ( IsHooking )
			return true;

		if ( !Player.IsValid() )
			return false;

		var tr = Player.GetEyeTrace( dir: Player.EyeForward, distance: LengthRange.Max ).Run();

		if ( !IsValidTarget( in tr ) )
			return false;

		var hookPoint = tr.HitPosition;

		HitObject = tr.GameObject;
		LocalPoint = tr.GameObject.WorldTransform.PointToLocal( hookPoint );
		Length = HookOrigin.Distance( hookPoint );

		return true;
	}

	protected virtual void HookMove( in float deltaTime )
	{
		if ( !HitObject.IsValid() || !Player.IsValid() )
			return;

		var tWorld = HitObject.WorldTransform;

		var origin = HookOrigin;
		var hookPoint = tWorld.PointToWorld( LocalPoint );

		var pointDist = origin.Distance( hookPoint );

		if ( pointDist < Length )
			return;

		// We're at or outside of our length.
		var dirToPoint = origin.Direction( hookPoint );

		var vel = Player.Velocity;

		vel.Separate( dirToPoint, out var fwdVel, out var hVel );

		// Swing along the radius.
		if ( vel.Dot( dirToPoint ) <= 0f )
		{
			var vRight = Rotation.LookAt( fwdVel ).Right;
			var cross = Vector3.Cross( fwdVel.Normal, vRight );

			var swing = cross * fwdVel.Length * deltaTime;
			hVel += swing * SwingSpeed;
		}

		// How much slack is there?
		var slack = (pointDist - Length).Positive();

		// Negate all exiting velocity past the limit.
		if ( slack > SlackLimit )
		{
			var fwdDot = fwdVel.Dot( dirToPoint );

			if ( fwdDot <= 0f )
				fwdVel = default;
		}

		// Add elasticity.
		var elasticity = slack.Remap( 0f, SlackLimit, 0f, Elasticity );
		fwdVel += dirToPoint * elasticity * deltaTime;

		// Retract pull speed.
		if ( IsRetracting )
		{
			var oldSpeed = vel.Dot( dirToPoint );

			fwdVel += dirToPoint * PullSpeed * deltaTime;

			var newSpeed = fwdVel.Dot( dirToPoint );

			if ( newSpeed > PullSpeedLimit )
				fwdVel = fwdVel.Normal * oldSpeed.Max( PullSpeedLimit );
		}

		Player.Velocity = hVel + fwdVel;
	}
}
