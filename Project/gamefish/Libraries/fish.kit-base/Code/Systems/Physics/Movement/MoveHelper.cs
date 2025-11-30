namespace GameFish;

/// <summary>
/// 🏃‍♂️💨 <br />
/// Uses traces to test and resolve collisions and sliding. <br />
/// Stores the results on itself.
/// <br /> <br />
/// <b> NOTE: </b> This utility is a work in progress!
/// </summary>
[Icon( "directions_run" )]
public partial class MoveHelper
{
	/*
	/// <summary>
	/// The associated controller(if any).
	/// Defining this allows
	/// </summary>
	public virtual BaseController Controller { get; set; }
	*/


	/// <summary>
	/// The trace to modify before running.
	/// </summary>
	public virtual SceneTrace Trace { get; set; }


	/// <summary> The position that the movement started at. </summary>
	public virtual Vector3 StartPosition { get; set; }

	/// <summary> The current/resulting position. </summary>
	public virtual Vector3 Position { get; set; }

	/// <summary> The direction last moving towards. </summary>
	public virtual Vector3 Direction { get; set; }

	/// <summary> The distance left to travel. </summary>
	public virtual float Distance { get; set; }

	/// <summary> The current/resulting velocity. </summary>
	public virtual Vector3 Velocity { get; set; }


	/// <summary> Can this project movement/velocity along surfaces? </summary>
	public virtual bool AllowSliding { get; set; } = true;

	/// <summary> The limit of slide/bounce operations possible per movement. </summary>
	public virtual int CollisionLimit { get; set; } = 5;

	/// <summary> The number of slides/bounces that can still be performed. </summary>
	public virtual int CollisionsRemaining { get; set; }


	/// <summary>
	/// Movement/collision logic tries to stay this far away
	/// from surfaces to prevent getting stuck in them.
	/// </summary>
	public virtual float SkinWidth { get; set; } = 0.2f;


	/// <summary> If the last movement hit. </summary>
	public virtual bool? Hit { get; set; }

	/// <summary> The normal of the last surface hit(or null). </summary>
	public virtual Vector3? HitNormal { get; set; }

	/// <summary> The point of the last surface hit(or null). </summary>
	public virtual Vector3? HitPosition { get; set; }


	public MoveHelper() { }

	public MoveHelper( BaseController c )
	{
		// Controller = c;
		Trace = c.Trace();
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
		Trace = c?.Trace() ?? default;
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


	public MoveHelper Move( in Vector3 from, in Vector3 to, in Vector3 vel )
		=> Move( in from, from.Direction( to ), from.Distance( to ), in vel );

	public MoveHelper Move( in Vector3 start, in Vector3 dir, in float distance, in Vector3 vel )
	{
		Position = start;
		Direction = dir;
		Distance = distance;
		Velocity = vel;

		if ( distance == 0f )
			return this;

		return Move();
	}

	/// <summary>
	/// Performs movement using its already provided properties.
	/// </summary>
	public virtual MoveHelper Move()
	{
		var start = Position;
		var dest = start + (Direction * Distance);

		StartPosition = start;

		var trMove = Trace
			.FromTo( start, dest )
			.Run();

		// No need to resolve collisions if there wasn't one.
		if ( !trMove.Hit )
		{
			Position = trMove.EndPosition;
			return this;
		}

		if ( trMove.StartedSolid )
			Position = start + (trMove.Normal * SkinWidth);

		if ( AllowSliding )
		{
			CollisionsRemaining = CollisionLimit;
			Slide( trMove.Normal );
		}

		return this;
	}

	/// <summary>
	/// Slides this along the hit surface.
	/// </summary>
	/// <param name="surfaceNormal"> The normal of the surface to slide along. </param>
	protected virtual void Slide( in Vector3 surfaceNormal )
	{
		var slideDir = Vector3.VectorPlaneProject( Direction, surfaceNormal );
		var surfaceDot = Direction.Dot( slideDir ).Positive();

		Velocity = slideDir * (Velocity.Length * surfaceDot);

		Direction = slideDir;
		Distance *= surfaceDot;

		var trSlide = Trace
			.FromTo( Position, Position + (Direction * Distance) )
			.Run();

		if ( trSlide.StartedSolid )
		{
			// if ( !TryUnstuck() )
			//	return;

			Position = StartPosition + (trSlide.Normal * SkinWidth);
		}

		Position = trSlide.EndPosition;
		Distance -= trSlide.Distance;

		if ( trSlide.Hit )
		{
			CollisionsRemaining--;

			if ( CollisionsRemaining > 0 )
				Slide( trSlide.Normal );
		}
	}
}
