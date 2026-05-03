using System;

namespace GameFish;

partial class ControllerPhysics
{
	/// <summary>
	/// The current movement data/utility.
	/// Used to manually move stuff with collision.
	/// </summary>
	public virtual MoveHelper Mover { get; set; }

	/// <summary>
	/// Moves using traces using a relative vector for the destination.
	/// Basically adds <paramref name="delta"/> to the current position.
	/// </summary>
	public void MoveDelta( in Vector3 delta )
		=> MoveTo( WorldPosition + delta );

	/// <summary>
	/// Moves using traces from the current position towards the destination.
	/// </summary>
	public void MoveTo( in Vector3 to )
		=> Move( WorldPosition, in to );

	/// <summary>
	/// Moves using traces from one position to another.
	/// </summary>
	public void Move( in Vector3 from, in Vector3 to )
	{
		if ( from == to )
			return;

		var tWorld = Origin;

		var tFrom = tWorld.WithPosition( from );
		var tDest = tWorld.WithPosition( to );

		Move( in tFrom, in tDest );
	}

	/// <summary>
	/// Moves using traces from one position to another.
	/// </summary>
	public void Move( in Transform tFrom, in Transform tDest )
	{
		if ( tFrom == tDest )
			return;

		var move = Mover ??= new();

		move.Trace = Trace();
		move.Run( tFrom.Position, tDest.Position, Velocity );

		Velocity = move.Velocity;
		WorldPosition = move.Position;

		IsGrounded = move.IsGrounded;
		GroundNormal = move.GroundNormal;
	}
}
