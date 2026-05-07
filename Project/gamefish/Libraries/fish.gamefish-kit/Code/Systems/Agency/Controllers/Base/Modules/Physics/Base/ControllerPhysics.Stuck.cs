namespace GameFish;

partial class ControllerPhysics
{
	/// <summary>
	/// The maximum distance that we'll try to get unstuck from.
	/// <br /> <br />
	/// <b> WARNING: </b> Potentially expensive at higher values.
	/// </summary>
	[Property]
	[Range( 8f, 64f, clamped: false )]
	[Feature( PAWN ), Group( PHYSICS ), Order( PHYSICS_ORDER )]
	public float MaxUnstuckLength { get; set; } = 16f;

	/// <summary>
	/// The limit of attempts(after checking length) to get us unstuck before giving up.
	/// <br /> <br />
	/// <b> WARNING: </b> Potentially expensive at higher values.
	/// </summary>
	[Property]
	[Range( 1, 64, clamped: false )]
	[Feature( PAWN ), Group( PHYSICS ), Order( PHYSICS_ORDER )]
	public int MaxUnstuckTries { get; set; } = 32;

	protected virtual bool TrySnapTo( in SceneTraceResult trSnap, ProjectedMovement move )
	{
		if ( trSnap.StartedSolid )
		{
			move.IsStuck = true;
			return false;
		}

		/*
		var endPos = trSnap.EndPosition;

		// If there was no hit then cool.
		if ( !trSnap.Hit )
		{
			move.Position = endPos;
			return true;
		}

		// Apply a bit of skin if possible.
		var skinDist = SkinWidth.Positive();
		var vSkin = trSnap.Normal * skinDist;

		var skinPos = endPos + vSkin;

		if ( IsEmpty( skinPos, skin: true, move, out _ ) )
		{
			move.IsStuck = false;
			move.Position = skinPos;
			return true;
		}
		*/

		move.Position = trSnap.EndPosition;

		return false;
	}

	protected virtual bool TryUnstuck( in SceneTraceResult trStuck, ProjectedMovement move )
	{
		if ( trStuck.StartedSolid )
			move.IsStuck = true;

		Vector3 vSkin;

		var startPos = trStuck.EndPosition;
		Vector3 freePos = startPos;

		if ( trStuck.Hit )
		{
			for ( var len = 1f; len <= MaxUnstuckLength; len += 1f )
			{
				vSkin = trStuck.Normal * len;
				freePos = startPos + vSkin;

				if ( IsEmpty( freePos, skin: false, move, out _ ) )
					goto Unstuck;

				freePos = startPos - vSkin;

				if ( IsEmpty( freePos, skin: false, move, out _ ) )
					goto Unstuck;
			}
		}

		for ( var i = 1; i <= MaxUnstuckTries; i++ )
		{
			// Where were we coming from?
			if ( LastVelocity is Vector3 lastVel && lastVel != default )
			{
				vSkin = -lastVel.Normal * i;
				freePos = startPos + vSkin;

				if ( IsEmpty( freePos, false, move, out _ ) )
					goto Unstuck;
			}

			// Pick a random place.
			vSkin = Vector3.Random.Normal * i;
			freePos = startPos + vSkin;

			if ( IsEmpty( freePos, false, move, out _ ) )
				goto Unstuck;
		}

		if ( move.IsStuck )
			return false;

		Unstuck:

		move.IsStuck = false;
		move.Position = freePos;

		// var trSnap = move.Trace( move.WithPosition( freePos ), endPos: in startPos, skin: false ).Run();
		// TrySnapTo( in trSnap, move );

		return !move.IsStuck;
	}
}
