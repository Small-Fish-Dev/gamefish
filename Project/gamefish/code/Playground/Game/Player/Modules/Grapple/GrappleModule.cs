
namespace Fishbox;

public partial class PlayerGrappleModule : PlayerModule
{
	protected const int HOOK_ORDER = PLAYER_ORDER - 500;

	[Property]
	[Feature( HOOK ), Order( HOOK_ORDER )]
	[Range( 1f, 100f, clamped: false )]
	public float Elasticity { get; set; } = 20f;

	[Property]
	[Feature( HOOK ), Order( HOOK_ORDER )]
	[Range( 1f, 128f, clamped: false )]
	public float SlackLimit { get; set; } = 64f;

	[Property]
	[Feature( HOOK ), Order( HOOK_ORDER )]
	[Range( 0f, 1000f, clamped: false )]
	public float RetractSpeed { get; set; } = 500f;

	[Property]
	[Feature( HOOK ), Order( HOOK_ORDER )]
	[Range( 0f, 1000f, clamped: false )]
	public float ExtendSpeed { get; set; } = 500f;

	[Property]
	[Range( 0f, 2f, clamped: false )]
	[Feature( HOOK ), Order( HOOK_ORDER )]
	public float SwingSpeed { get; set; } = 0.5f;

	[Property]
	[Feature( HOOK ), Order( HOOK_ORDER )]
	public FloatRange LengthRange { get; set; } = new( 16f, 4096f );

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

	[Property]
	[Feature( HOOK ), Order( HOOK_ORDER )]
	public LineRenderer Line { get; set; }

	public Vector3 HookOrigin => Player.IsValid()
		? Player.WorldPosition.LerpTo( Player.EyePosition, 0.5f )
		: WorldPosition;

	public bool IsHooking
	{
		get => HitObject.IsValid();
		set
		{
			if ( value is false )
				HitObject = null;
		}
	}

	[Sync] public GameObject HitObject { get; set; }
	[Sync] public Vector3 LocalPoint { get; set; }

	[Sync]
	public float Length
	{
		get => _length.Positive();
		set => _length = value.Clamp( LengthRange );
	}

	protected float _length;

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

	public override void UpdateInput( in float deltaTime )
	{
		base.UpdateInput( deltaTime );

		if ( Input.Down( HookButton ) )
			Hook();
		else
			IsHooking = false;

		if ( IsHooking )
		{
			var isRetracting = Input.Down( RetractButton );
			var isExtending = Input.Down( ExtendButton );

			if ( !isRetracting && !isExtending )
				return;

			if ( !TryGetHook( out var origin, out var worldPoint ) )
				return;

			if ( isRetracting && !isExtending )
			{
				Length = (Length - RetractSpeed * deltaTime)
					.Min( origin.Distance( worldPoint ) );
			}
			else if ( isExtending && !isRetracting )
			{
				Length += ExtendSpeed * deltaTime;
			}
		}
	}

	public static bool IsValidTarget( in SceneTraceResult tr )
	{
		if ( !tr.Hit || !tr.GameObject.IsValid() )
			return false;

		return true;
	}

	[Button( "Force Activation" )]
	[Feature( HOOK ), Group( DEBUG )]
	[ShowIf( nameof( InGame ), true )]
	protected void Hook()
	{
		if ( IsHooking )
			return;

		if ( !Player.IsValid() )
			return;

		var tr = Player.GetEyeTrace( dir: Player.EyeForward, distance: LengthRange.Max ).Run();

		if ( !IsValidTarget( in tr ) )
			return;

		var hookPoint = tr.HitPosition;

		HitObject = tr.GameObject;
		LocalPoint = tr.GameObject.WorldTransform.PointToLocal( hookPoint );
		Length = HookOrigin.Distance( hookPoint );
	}

	public override void PostMove( in float deltaTime, in bool isFixedUpdate )
	{
		if ( !HitObject.IsValid() || !Player.IsValid() )
			return;

		var tWorld = HitObject.WorldTransform;

		var origin = HookOrigin;
		var worldPoint = tWorld.PointToWorld( LocalPoint );

		var pointDist = origin.Distance( worldPoint );

		if ( pointDist < Length )
			return;

		// We're at or outside of our length.
		var dirToPoint = origin.Direction( worldPoint );

		Player.Velocity.Separate( dirToPoint, out var fwdVel, out var hVel );

		// Swing along the radius.
		var vRight = Rotation.LookAt( fwdVel ).Right;
		var cross = Vector3.Cross( fwdVel.Normal, vRight );

		var swing = cross * fwdVel.Length * deltaTime;
		hVel += swing * SwingSpeed;

		// Elasticity towards the point.
		var slack = (pointDist - Length).Positive();
		fwdVel += dirToPoint * slack * Elasticity * deltaTime;

		// Negate all exiting velocity past the limit.
		if ( slack > SlackLimit )
			fwdVel *= fwdVel.Dot( dirToPoint ).Direction().Positive();

		Player.Velocity = hVel + fwdVel;
	}
}
