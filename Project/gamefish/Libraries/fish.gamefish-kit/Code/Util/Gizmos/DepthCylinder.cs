namespace GameFish;

partial class Library
{
	/// <summary>
	/// Draws a slightly fancy cylinder that better indicates depth.
	/// </summary>
	/// <param name="g"></param>
	/// <param name="r"> The radius of the cylinder. </param>
	/// <param name="h"> The height of the cylinder. </param>
	/// <param name="cLines"> The default lines color. </param>
	/// <param name="cSolid"> The default solid color. </param>
	/// <param name="sides"> How many faces on the side there are. </param>
	/// <param name="tWorld"> The world transform to use. Defaults to the object's world transform. </param>
	/// <returns> If the cylinder could be drawn. </returns>
	public static bool DepthCylinder( this Gizmo.GizmoDraw g, in float r, in float h, in Color cLines, in Color cSolid, int sides = 32, Transform? tWorld = null )
	{
		if ( g is null || r == 0f || h == 0f )
			return false;

		using ( Gizmo.Scope() )
		{
			if ( tWorld is Transform t )
				Gizmo.Transform = t;

			var isSelected = IsHighlighting;

			var halfHeight = h * .5f;
			var vHeight = Vector3.Up * halfHeight;

			if ( cSolid.a > 0f )
			{
				// Depthless pass(see slightly through walls).
				g.IgnoreDepth = true;
				g.Color = cSolid.WithAlphaMultiplied( isSelected ? 0.2f : 0.1f );

				g.SolidCylinder( -vHeight, vHeight, r, sides );

				// Depth pass(more directly visible).
				g.IgnoreDepth = false;
				g.Color = cSolid.WithAlphaMultiplied( isSelected ? 1f : 0.1f );

				g.SolidCylinder( -vHeight, vHeight, r, sides );
			}

			// Depthless pass(see slightly through walls).
			g.IgnoreDepth = true;
			g.LineThickness = 1f;
			g.Color = cLines.WithAlphaMultiplied( isSelected ? 0.2f : 0.1f );

			g.LineCylinder( -vHeight, vHeight, r, r, sides );

			// Depth pass(more directly visible).
			g.IgnoreDepth = false;
			g.LineThickness = isSelected ? 2f : 1f;
			g.Color = cLines.WithAlphaMultiplied( isSelected ? 1f : 0.2f );

			g.LineCylinder( -vHeight, vHeight, r, r, sides );
		}

		return true;
	}

	/// <summary>
	/// Draws a slightly fancy Cylinder that better indicates depth.
	/// </summary>
	/// <param name="obj"></param>
	/// <param name="r"> The radius of the cylinder. </param>
	/// <param name="h"> The height of the cylinder. </param>
	/// <param name="cLines"> The default lines color. </param>
	/// <param name="cSolid"> The default solid color. </param>
	/// <param name="sides"> How many faces on the side there are. </param>
	/// <param name="tWorld"> The world transform to use. Defaults to the object's world transform. </param>
	/// <returns> If the Cylinder could be drawn. </returns>
	public static bool DrawCylinder( this GameObject obj, in float r, in float h, in Color cLines, in Color cSolid, int sides = 32, Transform? tWorld = null )
	{
		if ( !obj.IsValid() )
			return false;

		return Gizmo.Draw.DepthCylinder( r, h, cLines, cSolid, sides, tWorld ?? obj.WorldTransform );
	}

	/// <summary>
	/// Draws a slightly fancy cylinder that better indicates depth.
	/// </summary>
	/// <param name="comp"></param>
	/// <param name="r"> The radius of the cylinder. </param>
	/// <param name="h"> The height of the cylinder. </param>
	/// <param name="cLines"> The default lines color. </param>
	/// <param name="cSolid"> The default solid color. </param>
	/// <param name="sides"> How many faces on the side there are. </param>
	/// <param name="tWorld"> The world transform to use. Defaults to the component's world transform. </param>
	/// <returns> If the Cylinder could be drawn. </returns>
	public static bool DrawCylinder( this Component comp, in float r, in float h, in Color cLines, in Color cSolid, int sides = 32, Transform? tWorld = null )
		=> comp?.GameObject.DrawCylinder( r, h, cLines, cSolid, sides, tWorld ?? comp.WorldTransform ) ?? false;
}
