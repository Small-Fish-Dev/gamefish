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
	public bool Sliding { get; set; } = true;


	/// <summary> The maximum number of algorithm iterations possible. </summary>
	public int Limit { get; set; } = 5;

	/// <summary> The number of iterations remaining before cancellation. </summary>
	public int Budget { get; set; }


	public MoveHelper() { }

	public MoveHelper( BaseController c )
	{
		// Controller = c;
		Trace = c.BuildTrace();
	}

	public MoveHelper( in SceneTrace tr, in Vector3 pos, in Vector3 dir, in float dist, in Vector3 vel )
	{
		Trace = tr;

		Position = pos;
		Direction = dir;
		Distance = dist;
		Velocity = vel;
	}


	public MoveHelper WithController( BaseController c )
	{
		// Controller = c;
		Trace = c?.BuildTrace() ?? default;
		return this;
	}

	public MoveHelper WithTrace( in SceneTrace tr )
	{
		Trace = tr;
		return this;
	}

	public MoveHelper WithPosition( in Vector3 pos )
	{
		Position = pos;
		return this;
	}

	public MoveHelper WithDirection( in Vector3 dir )
	{
		Direction = dir;
		return this;
	}

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
		Sliding = bSliding;
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
		Origin = Position;

		Budget = Limit;

		Move( Direction, Distance );

		return this;
	}

	private void Move( in Vector3 dir, in float dist )
	{
		// Prevent infinite loops.
		if ( Distance <= 0f || Budget <= 0f )
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
			return;
		}

		var vHitSkin = trMove.Normal * SkinWidth;

		// Stuck inside of something.
		if ( trMove.StartedSolid )
		{
			// TODO: Proper unstuck algorithm.
			IsStuck = true;

			var skinPos = trMove.EndPosition + vHitSkin;

			if ( !IsEmpty( skinPos, out _ ) )
			{
				Position = skinPos;
				Velocity = default;
				return;
			}
		}

		// Hit something.
		Distance -= trMove.Distance;
		Position = trMove.EndPosition + vHitSkin;

		if ( Sliding )
			Slide( in dir, in trMove.Normal );

		// Keep moving if we have distance to go.
		Move( Direction, Distance );
	}

	/// <summary>
	/// Redirects momentum along the hit surface.
	/// </summary>
	/// <param name="moveDir"> The direction of movement. </param>
	/// <param name="normal"> The direction of the surface to slide along. </param>
	private void Slide( in Vector3 moveDir, in Vector3 normal )
	{
		if ( Distance <= 0f )
			return;

		Velocity = Vector3.VectorPlaneProject( Velocity, normal );

		var vProject = Vector3.VectorPlaneProject( moveDir, normal );
		var dot = 1f - moveDir.Dot( normal ).Abs();

		Direction = vProject.Normal;
		Distance *= dot;
	}

	public bool IsEmpty( in Vector3 pos, out SceneTraceResult trEmpty )
	{
		trEmpty = Trace
			.FromTo( pos, pos )
			.Run();

		return !trEmpty.StartedSolid;
	}
}
