namespace GameFish;

partial class Library
{
	/// <summary>
	/// Draws a slightly fancy capsule that better indicates depth.
	/// </summary>
	/// <param name="g"></param>
	/// <param name="vStart"> The local start point. </param>
	/// <param name="vEnd"> The local ending point. </param>
	/// <param name="r"> The radius of the capsule. </param>
	/// <param name="cLines"> The default lines color. </param>
	/// <param name="cSolid"> The default solid color. </param>
	/// <param name="sides"> How many faces on the side there are. </param>
	/// <param name="tWorld"> The world transform to use. Defaults to the object's world transform. </param>
	/// <returns> If the capsule could be drawn. </returns>
	public static bool DepthCapsule( this Gizmo.GizmoDraw g, in Vector3 vStart, in Vector3 vEnd, in float r, in Color cLines, in Color cSolid, int sides = 32, Transform? tWorld = null )
	{
		if ( g is null || r == 0f || vStart == vEnd )
			return false;

		using ( Gizmo.Scope() )
		{
			if ( tWorld is Transform t )
				Gizmo.Transform = t;

			var isSelected = IsHighlighting;

			if ( cSolid.a > 0f )
			{
				// Depthless pass(see slightly through walls).
				g.IgnoreDepth = true;
				g.Color = cSolid.WithAlphaMultiplied( isSelected ? 0.2f : 0.1f );

				g.SolidCapsule( vStart, vEnd, r, sides, sides );

				// Depth pass(more directly visible).
				g.IgnoreDepth = false;
				g.Color = cSolid.WithAlphaMultiplied( isSelected ? 1f : 0.1f );

				g.SolidCapsule( vStart, vEnd, r, sides, sides );
			}

			if ( cLines.a > 0f )
			{
				var c = new Capsule( vStart, vEnd, r );

				// Depthless pass(see slightly through walls).
				g.IgnoreDepth = true;
				g.LineThickness = 1f;
				g.Color = cLines.WithAlphaMultiplied( isSelected ? 0.2f : 0.1f );

				g.LineCapsule( c, sides );

				// Depth pass(more directly visible).
				g.IgnoreDepth = false;
				g.LineThickness = isSelected ? 2f : 1f;
				g.Color = cLines.WithAlphaMultiplied( isSelected ? 1f : 0.2f );

				g.LineCapsule( c, sides );
			}
		}

		return true;
	}

	/// <summary>
	/// Draws a slightly fancy capsule that better indicates depth.
	/// </summary>
	/// <param name="g"></param>
	/// <param name="c"> The capsule configuration. </param>
	/// <param name="cLines"> The default lines color. </param>
	/// <param name="cSolid"> The default solid color. </param>
	/// <param name="sides"> How many faces on the side there are. </param>
	/// <param name="tWorld"> The world transform to use. Defaults to the object's world transform. </param>
	/// <returns> If the capsule could be drawn. </returns>
	public static bool DepthCapsule( this Gizmo.GizmoDraw g, in Capsule c, in Color cLines, in Color cSolid, int sides = 32, Transform? tWorld = null )
		=> Gizmo.Draw.DepthCapsule( c.CenterA, c.CenterB, c.Radius, cLines, cSolid, sides, tWorld );

	/// <summary>
	/// Draws a slightly fancy Capsule that better indicates depth.
	/// </summary>
	/// <param name="obj"></param>
	/// <param name="vStart"> The local start point. </param>
	/// <param name="vEnd"> The local ending point. </param>
	/// <param name="r"> The radius of the capsule. </param>
	/// <param name="cLines"> The default lines color. </param>
	/// <param name="cSolid"> The default solid color. </param>
	/// <param name="sides"> How many faces on the side there are. </param>
	/// <param name="tWorld"> The world transform to use. Defaults to the object's world transform. </param>
	/// <returns> If the Capsule could be drawn. </returns>
	public static bool DrawCapsule( this GameObject obj, in Vector3 vStart, in Vector3 vEnd, in float r, in Color cLines, in Color cSolid, int sides = 32, Transform? tWorld = null )
	{
		if ( !obj.IsValid() )
			return false;

		return Gizmo.Draw.DepthCapsule( vStart, vEnd, r, cLines, cSolid, sides, tWorld ?? obj.WorldTransform );
	}

	/// <summary>
	/// Draws a slightly fancy capsule that better indicates depth.
	/// </summary>
	/// <param name="comp"></param>
	/// <param name="vStart"> The local start point. </param>
	/// <param name="vEnd"> The local ending point. </param>
	/// <param name="r"> The radius of the capsule. </param>
	/// <param name="cLines"> The default lines color. </param>
	/// <param name="cSolid"> The default solid color. </param>
	/// <param name="sides"> How many faces on the side there are. </param>
	/// <param name="tWorld"> The world transform to use. Defaults to the component's world transform. </param>
	/// <returns> If the Capsule could be drawn. </returns>
	public static bool DrawCapsule( this Component comp, in Vector3 vStart, in Vector3 vEnd, in float r, in Color cLines, in Color cSolid, int sides = 32, Transform? tWorld = null )
		=> comp?.GameObject.DrawCapsule( vStart, vEnd, r, cLines, cSolid, sides, tWorld: null ) ?? false;
}
