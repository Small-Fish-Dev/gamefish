namespace GameFish;

public class ProjectedMovement
{
	/// <summary>
	/// What position the movement started at.
	/// </summary>
	public Transform Start = Transform.Zero;

	/// <summary>
	/// The current point the movement is at.
	/// </summary>
	public Transform Point = Transform.Zero;

	/// <summary>
	/// The inverse of the offset from the starting tranform.
	/// </summary>
	public Offset Delta = Transform.Zero;

	/// <summary>
	/// The direction the projection should be moving.
	/// </summary>
	public Vector3 Direction = default;

	/// <summary>
	/// The distance remaining for the projection to travel.
	/// </summary>
	public float Distance
	{
		get => _dist;
		set => _dist = value.Positive();
	}

	private float _dist = 0f;

	public Vector3 Velocity = default;

	/// <summary>
	/// The trace skin width that was used previously.
	/// </summary>
	public float Skin = 0f;

	public bool IsStuck { get; set; }

	public bool IsGrounded { get; set; }
	public Vector3 GroundNormal { get; set; }

	/// <summary>
	/// The position the projection at.
	/// </summary>
	public Vector3 Position
	{
		get => Point.Position;
		set => Point.Position = value;
	}

	/// <summary>
	/// The rotation the projection is at.
	/// </summary>
	public Rotation Rotation
	{
		get => Point.Rotation;
		set => Point.Rotation = value;
	}

	public ProjectedMovement( in Transform tStart, in Offset delta, in Vector3 dir, in float dist, in Vector3 vel )
	{
		Start = tStart;
		Point = tStart;

		Delta = delta;

		Direction = dir;
		Distance = dist;

		Velocity = vel;
	}

	public Transform WithPosition( in Vector3 pos )
		=> Point.WithPosition( in pos );

	public Transform WithPosition( in Vector3 pos, in Rotation r )
		=> Point.WithPosition( in pos, in r );

	public Transform WithRotation( in Rotation r )
		=> Point.WithRotation( in r );

	/// <returns> The position this wants to be given its movement parameters. </returns>
	public Vector3 Destination()
		=> Point.Position + (Direction * Distance);

	/// <returns> Where the projection wants to be given its movement parameters. </returns>
	public Transform Projected()
		=> Project( Direction * Distance );

	/// <returns> Where the projection would be with this position added. </returns>
	public Transform Project( in Vector3 posAdd )
		=> Point.WithPosition( Point.Position + posAdd );

	/// <returns> Where the projection would be with this offset added. </returns>
	public Transform Project( in Vector3 dir, in float dist )
		=> Project( dir * dist );

	/// <returns> Where the projection would be oriented with this rotation added. </returns>
	public Transform Rotate( in Rotation r )
		=> Point.WithRotation( Point.Rotation * r );

	/// <returns> Where the projection would be oriented with this rotation added. </returns>
	public Transform Rotate( in Vector3 axis, in float degrees )
		=> Point.WithRotation( Point.Rotation * Rotation.FromAxis( axis, degrees ) );
}
