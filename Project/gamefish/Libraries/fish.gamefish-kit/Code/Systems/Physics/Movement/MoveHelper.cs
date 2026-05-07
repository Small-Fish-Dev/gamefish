namespace GameFish;

/// <summary>
/// 🏃‍♂️💨 <br />
/// Uses traces to test and resolve collisions and sliding. <br />
/// Stores the results on itself.
/// <br /> <br />
/// <b> NOTE: </b> This utility is a work in progress!
/// </summary>
[Icon( "directions_run" )]
public sealed class MoveHelper
{
	/*
	/// <summary>
	/// The associated controller(if any).
	/// Defining this allows
	/// </summary>
	public BaseController Controller { get; set; }
	*/


	/// <summary>
	/// The trace to modify before running.
	/// </summary>
	public SceneTrace Trace { get; set; }


	/// <summary>
	/// Is this unable to move properly, such as being inside of something?
	/// </summary>
	public bool IsStuck { get; set; }


	/// <summary> The maximum number of algorithm iterations possible. </summary>
	public int Limit { get; set; } = 10;

	/// <summary> The number of iterations remaining before cancellation. </summary>
	public int Budget { get; set; }


	/// <summary>
	/// Movement/collision logic tries to stay this far away
	/// from surfaces to prevent getting stuck in them.
	/// </summary>
	public float SkinWidth { get; set; } = 0.2f;


	/// <summary> The position where movement started at. </summary>
	public Vector3 Origin { get; set; }

	/// <summary> The current/resulting position. </summary>
	public Vector3 Position { get; set; }

	/// <summary> The direction last moving towards. </summary>
	public Vector3 Direction { get; set; }

	/// <summary> The distance left to travel. </summary>
	public float Distance
	{
		get => _dist;
		set => _dist = value.Positive();
	}

	private float _dist = 0f;

	/// <summary> The current/resulting velocity. </summary>
	public Vector3 Velocity { get; set; }


	/// <summary> Allow projecting momentum along surfaces? </summary>
	public bool SlidingEnabled { get; set; } = true;


	/// <summary> Stick to floors? Also prevents slipping down them. </summary>
	public bool GroundingEnabled { get; set; } = true;

	/// <summary> The angle in which a surface is considered ground. </summary>
	public float GroundAngle { get; set; } = 45f;

	/// <summary> The maximum distance to stick to ground. </summary>
	public float GroundDistance { get; set; } = 16f;

	/// <summary> The surface normal of a perfectly flat foor. </summary>
	public Vector3 Up { get; set; } = Vector3.Up;
	public Vector3 Down => -Up;

	/// <summary> Are we standing on a floor? </summary>
	public bool IsGrounded { get; set; } = false;

	/// <summary> Are we standing on a floor? </summary>
	public Vector3 GroundNormal { get; set; } = Vector3.Up;


	public MoveHelper() { }

	public MoveHelper( in SceneTrace tr, in Vector3 pos, in Vector3 dir, in float dist, in Vector3 vel )
	{
		Trace = tr;

		Position = pos;
		Direction = dir;
		Distance = dist;
		Velocity = vel;
	}


	public MoveHelper WithTrace( in SceneTrace tr )
	{
		Trace = tr;

		return this;
	}

	/// <summary>
	/// Sets the hypothetical position the mover is currently at.
	/// </summary>
	public MoveHelper WithPosition( in Vector3 pos )
	{
		Position = pos;

		return this;
	}

	/// <summary>
	/// Sets the mover's hypothetical movement direction.
	/// </summary>
	public MoveHelper WithDirection( in Vector3 dir )
	{
		Direction = dir;

		return this;
	}

	/// <summary>
	/// Sets the total lifetime distance the mover is trying to go.
	/// </summary>
	public MoveHelper WithDistance( in float distance )
	{
		Distance = distance;

		return this;
	}

	public MoveHelper WithVelocity( in Vector3 vel )
	{
		Velocity = vel;

		return this;
	}

	public MoveHelper WithSliding( in bool bSliding )
	{
		SlidingEnabled = bSliding;

		return this;
	}

	/// <summary>
	/// Sets if we should stick to ground.
	/// </summary>
	public MoveHelper WithGroundSticking( in bool bGroundSticking )
	{
		GroundingEnabled = bGroundSticking;

		return this;
	}

	/// <summary>
	/// Enables grounding with this angle.
	/// </summary>
	public MoveHelper WithGrounding( in float fAngle, in float dist )
	{
		GroundingEnabled = true;

		GroundAngle = fAngle;

		return this;
	}

	/// <summary>
	/// Enables grounding with this angle, stick distance and flat floor surface normal.
	/// </summary>
	public MoveHelper WithGrounding( in float fAngle, in float stickDist, in Vector3 vUp )
	{
		GroundingEnabled = true;

		GroundAngle = fAngle;
		GroundDistance = stickDist;

		Up = vUp;

		return this;
	}

	/// <summary>
	/// Enables grounding with this angle and flat floor surface normal.
	/// </summary>
	public MoveHelper WithGrounded( in bool bGrounded )
	{
		IsGrounded = bGrounded;

		return this;
	}


	public MoveHelper Run( in Vector3 from, in Vector3 to, in Vector3 vel )
		=> Run( in from, from.Direction( to ), from.Distance( to ), in vel );

	public MoveHelper Run( in Vector3 start, in Vector3 dir, in float distance, in Vector3 vel )
	{
		Position = start;

		Direction = dir;
		Distance = distance;

		Velocity = vel;

		return Run();
	}

	/// <summary>
	/// Performs movement using previously provided parameters.
	/// <br /> <br />
	/// <b> NOTE: </b> Quickly set parameters with <see cref="Run(in Vector3, in Vector3, in float, in Vector3)"/>.
	/// </summary>
	public MoveHelper Run()
	{
		Budget = Limit;

		Origin = Position;

		if ( !GroundingEnabled )
			IsGrounded = false;

		Move( Direction, Distance );

		return this;
	}

	private void Move( in Vector3 dir, in float dist )
	{
		if ( dir == default || dist < 0f )
			return;

		Budget--;

		var dest = Position + (dir * dist);

		// Add some skin to the trace.
		var skin = SkinWidth;
		dest += dir * skin;

		var trMove = Trace
			.FromTo( Position, dest )
			.Run();

		// No need to resolve collisions if there wasn't one.
		if ( !trMove.Hit )
		{
			Position += dir * dist;

			Finish();

			return;
		}

		// Stuck inside of something.
		if ( trMove.StartedSolid )
		{
			// TODO: Proper unstuck algorithm.
			IsStuck = true;

			var vNormalSkin = trMove.Normal * SkinWidth;
			var vEndSkin = trMove.EndPosition + vNormalSkin;

			Position = vEndSkin;
			// Velocity = default;

			if ( !IsEmpty( vEndSkin, out _ ) )
			{
				Finish();
				return;
			}
		}

		// Hit something.
		Distance -= trMove.Distance;

		SnapTo( in trMove );

		if ( SlidingEnabled )
			Slide( in dir, in trMove.Normal );

		// Prevent infinite loops.
		if ( Distance <= 0f || Budget <= 0f )
		{
			Finish();
			return;
		}

		// Keep moving if we have distance to go.
		Move( Direction, Distance );
	}

	public void SnapTo( in SceneTraceResult trMove )
	{
		var hitGround = IsGround( trMove.Normal );

		if ( hitGround )
		{
			IsGrounded = true;
			Position = trMove.EndPosition + (Up * SkinWidth);
		}
		else
		{
			Position = trMove.EndPosition + (trMove.Normal * SkinWidth);
		}
	}

	/// <summary>
	/// Redirects momentum along the hit surface.
	/// </summary>
	/// <param name="normal"> The direction of the surface to slide along. </param>
	public void Slide( in Vector3 normal )
		=> Slide( Direction, in normal );

	/// <summary>
	/// Redirects momentum along the hit surface.
	/// </summary>
	/// <param name="moveDir"> The direction of movement. </param>
	/// <param name="normal"> The direction of the surface to slide along. </param>
	public void Slide( in Vector3 moveDir, in Vector3 normal )
	{
		if ( moveDir == default || normal == default )
			return;

		var dot = moveDir.Dot( normal );

		if ( dot == 0f )
		{
			Velocity = default;

			Direction = default;
			Distance = 0f;

			return;
		}

		if ( IsGround( normal ) )
			Velocity = Velocity.PlaneProject( Up );
		else
			Velocity = Velocity.PlaneProject( in normal );

		var vProject = moveDir.PlaneProject( in normal );

		Direction = vProject.Normal;
		Distance *= vProject.Length;
	}

	public bool IsEmpty( in Vector3 pos, out SceneTraceResult trEmpty )
	{
		trEmpty = Trace
			.FromTo( pos, pos )
			.Run();

		return !trEmpty.StartedSolid;
	}

	public bool IsGround( in Vector3 normal )
	{
		if ( !GroundingEnabled )
			return false;

		if ( Up.Angle( normal ) > GroundAngle )
			return false;

		var upVel = Velocity.Forward( Up );
		var upSpeed = upVel.Dot( normal );

		return upSpeed < 30f;
	}

	public bool IsTouchingGround( out SceneTraceResult trGround, in float dist )
	{
		trGround = Trace
			.FromTo( Position, Position + (Down * dist) )
			.Run();

		return trGround.Hit && IsGround( in trGround.Normal );
	}

	private void Finish()
	{
		if ( !GroundingEnabled )
			return;

		var stickDist = IsGrounded ? GroundDistance : SkinWidth.Max( 1f );

		IsGrounded = IsTouchingGround( out var trGround, stickDist );

		if ( IsGrounded )
		{
			// Snap to the ground.
			SnapTo( trGround );

			GroundNormal = trGround.Normal;
			Velocity = Velocity.PlaneProject( Up );
		}
	}
}
