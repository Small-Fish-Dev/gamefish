namespace GameFish;

partial class Library
{
	/// <summary>
	/// Draws a slightly fancy sphere that better indicates depth.
	/// </summary>
	/// <param name="g"></param>
	/// <param name="r"> The radius of the sphere. </param>
	/// <param name="center"> The offset of the sphere from world transform. </param>
	/// <param name="cLines"> The default lines color. </param>
	/// <param name="cSolid"> The default solid color. </param>
	/// <param name="tWorld"> The world transform to use. Defaults to the object's world transform. </param>
	/// <returns> If the sphere could be drawn. </returns>
	public static bool DepthSphere( this Gizmo.GizmoDraw g, in float r, in Vector3 center, in Color cLines, in Color cSolid, Transform? tWorld = null )
	{
		if ( g is null || r == 0f )
			return false;

		using ( Gizmo.Scope() )
		{
			if ( tWorld is Transform t )
				Gizmo.Transform = t;

			var isSelected = IsHighlighting;

			if ( cSolid.a > 0f )
			{
				const int solidSegments = 32;

				// Depthless pass(see slightly through walls).
				g.IgnoreDepth = true;
				g.Color = cSolid.WithAlphaMultiplied( isSelected ? 0.2f : 0.1f );

				g.SolidSphere( center, r, hSegments: solidSegments, vSegments: solidSegments );

				// Depth pass(more directly visible).
				g.IgnoreDepth = false;
				g.Color = cSolid.WithAlphaMultiplied( isSelected ? 1f : 0.1f );

				g.SolidSphere( center, r, hSegments: solidSegments, vSegments: solidSegments );
			}

			const int lineRings = 6;

			g.LineThickness = isSelected ? 2f : 1f;

			// Depthless pass(see slightly through walls).
			g.IgnoreDepth = true;
			g.Color = cLines.WithAlphaMultiplied( isSelected ? 0.2f : 0.1f );

			g.LineSphere( center, r, rings: lineRings );

			// Depth pass(more directly visible).
			g.IgnoreDepth = false;
			g.Color = cLines.WithAlphaMultiplied( isSelected ? 1f : 0.2f );

			g.LineSphere( center, r, rings: lineRings );
		}

		return true;
	}

	/// <summary>
	/// Draws a slightly fancy sphere that better indicates depth.
	/// </summary>
	/// <param name="obj"></param>
	/// <param name="r"> The radius of the sphere. </param>
	/// <param name="center"> The offset of the sphere from world transform. </param>
	/// <param name="cLines"> The default lines color. </param>
	/// <param name="cSolid"> The default solid color. </param>
	/// <param name="tWorld"> The world transform to use. Defaults to the object's world transform. </param>
	/// <returns> If the sphere could be drawn. </returns>
	public static bool DrawSphere( this GameObject obj, in float r, in Vector3 center, in Color cLines, in Color cSolid, Transform? tWorld = null )
	{
		if ( !obj.IsValid() )
			return false;

		return Gizmo.Draw.DepthSphere( r, center, cLines, cSolid, tWorld ?? obj.WorldTransform );
	}

	/// <summary>
	/// Draws a slightly fancy sphere that better indicates depth.
	/// </summary>
	/// <param name="comp"></param>
	/// <param name="r"> The radius of the sphere. </param>
	/// <param name="center"> The offset of the sphere from world transform. </param>
	/// <param name="cLines"> The default lines color. </param>
	/// <param name="cSolid"> The default solid color. </param>
	/// <param name="tWorld"> The world transform to use. Defaults to the component's world transform. </param>
	/// <returns> If the sphere could be drawn. </returns>
	public static bool DrawSphere( this Component comp, in float r, in Vector3 center, in Color cLines, in Color cSolid, Transform? tWorld = null )
		=> comp?.GameObject.DrawSphere( r, center, cLines, cSolid, tWorld ?? comp.WorldTransform ) ?? false;
}
