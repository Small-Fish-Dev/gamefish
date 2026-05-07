namespace GameFish;

partial class ControllerPhysics
{
	/*
	public virtual bool TryStep( in Vector3 startPos, in Vector3 vMove, in Vector3 vNormal, float stepHeight, float hDist )
	{
		if ( stepHeight < float.Epsilon || hDist < float.Epsilon )
			return false;

		if ( vMove.AlmostEqual( 0f ) || vNormal.AlmostEqual( 0f ) )
			return false;

		// Align the movement axis along the hit normal.
		var vForward = vMove.PlaneProject( Up ).Normal;
		var vUpward = Up.PlaneProject( vForward ).Normal;
		hDist = vMove.PlaneProject( vUpward ).Length;

		var trUpward = TraceDelta( startPos, vUpward * stepHeight );

		// DebugOverlay.Trace( trUpward.BodyTrace, duration: 3f );

		if ( trUpward.StartedSolid )
			return false;

		var vPos = trUpward.EndPosition;
		var trForward = TraceDelta( vPos, vForward * hDist );
		vPos = trForward.EndPosition;

		// DebugOverlay.Trace( trForward.BodyTrace, duration: 3f );

		if ( trForward.StartedSolid )
			return false;

		var trDown = Trace( vPos, vUpward * (stepHeight * -2f) );

		// DebugOverlay.Trace( trDown.BodyTrace, duration: 3f );

		if ( trDown.StartedSolid || !trDown.Hit )
			return false;

		if ( !TrySnapTo( trDown ) )
			return false;

		Transform.ClearInterpolation();

		return true;
	}
	*/
}
