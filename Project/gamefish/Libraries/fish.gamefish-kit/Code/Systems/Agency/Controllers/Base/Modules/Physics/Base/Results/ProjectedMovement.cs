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

	protected SceneTrace _trBase;
	protected SceneTrace _trSkin;

	public ProjectedMovement( ProjectedMovement move )
	{
		_trBase = move._trBase;
		_trSkin = move._trSkin;

		Start = move.Start;
		Point = move.Point;

		Delta = move.Delta;

		Direction = move.Direction;
		Distance = move.Distance;

		Velocity = move.Velocity;

		IsGrounded = move.IsGrounded;
		GroundNormal = move.GroundNormal;
	}

	public ProjectedMovement( in SceneTrace trBase, in SceneTrace trSkin, in Transform tStart, in Offset delta, in Vector3 dir, in float dist )
	{
		_trBase = trBase;
		_trSkin = trSkin;

		Start = tStart;
		Point = tStart;

		Delta = delta;

		Direction = dir;
		Distance = dist;
	}

	public ProjectedMovement( ControllerPhysics phys, in Transform tFrom, in Vector3 to )
	{
		if ( phys is null )
			return;

		_trBase = phys.Trace( skin: 0f );
		_trSkin = phys.Trace( skin: -phys.SkinWidth );

		var dir = tFrom.Position.Direction( in to );
		var dist = tFrom.Position.Distance( in to );

		var tStart = tFrom.WithOffset( phys.TraceOffset );
		Offset delta = tStart.ToLocal( tFrom );

		Start = tStart;
		Point = tStart;

		Delta = delta;

		Direction = dir;
		Distance = dist;

		IsStuck = phys.IsStuck;

		Velocity = phys.Velocity;

		IsGrounded = phys.IsGrounded && phys.GroundingEnabled;
		GroundNormal = phys.GroundNormal;
	}

	protected SceneTrace GetTrace( in bool withSkin )
		=> withSkin ? _trSkin : _trBase;

	/// <returns> A trace from the current position to the projected destination. </returns>
	public SceneTrace ProjectedTrace( in bool skin )
		=> GetTrace( skin ).FromTo( in Point, Position + (Direction * Distance) );

	/// <returns> A trace from the current position. </returns>
	public SceneTrace Trace( in bool skin )
		=> GetTrace( skin ).FromTo( in Point, in Point.Position );

	/// <returns> A trace from the current position to a destination. </returns>
	public SceneTrace Trace( in Vector3 endPos, in bool skin )
		=> GetTrace( skin ).FromTo( in Point, in endPos );

	/// <returns> A trace from a separate transform to a destination. </returns>
	public SceneTrace Trace( in Transform tStart, in Vector3 endPos, in bool skin )
		=> GetTrace( skin ).FromTo( in tStart, in endPos );

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
