namespace GameFish;

partial class ControllerPhysics
{
	/// <summary>
	/// The current movement data/utility.
	/// Used to manually move stuff with collision.
	/// </summary>
	public virtual MoveHelper Mover { get; set; }

	/// <summary>
	/// Moves using a relative vector for the destination.
	/// Basically adds <paramref name="delta"/> to the current position.
	/// </summary>
	public void MoveDelta( in Vector3 delta )
		=> MoveTo( WorldPosition + delta );

	/// <summary>
	/// Moves from the current position towards the destination.
	/// </summary>
	public void MoveTo( in Vector3 to )
		=> Move( WorldPosition, in to );

	/// <summary>
	/// Moves from one position to another.
	/// </summary>
	public void Move( in Vector3 from, in Vector3 to )
		=> Move( Origin.WithPosition( from ), in to );

	/// <summary>
	/// Moves from a transform to a position.
	/// </summary>
	public virtual void Move( in Transform tFrom, in Vector3 to )
	{
		var move = Mover ??= new();

		move.Trace = Trace();
		move.Run( in tFrom.Position, in to, Velocity );

		Velocity = move.Velocity;
		WorldPosition = move.Position;

		IsGrounded = move.IsGrounded;
		GroundNormal = move.GroundNormal;
	}
}
