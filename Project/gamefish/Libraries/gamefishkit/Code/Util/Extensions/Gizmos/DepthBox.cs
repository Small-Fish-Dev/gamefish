namespace GameFish;

partial class Library
{
	/// <summary>
	/// Draws a slightly fancy box that better indicates depth.
	/// </summary>
	/// <param name="g"></param>
	/// <param name="box"> The local bounds of the box. </param>
	/// <param name="cLines"> The line box(if not null)'s color. </param>
	/// <param name="cSolid"> The solid box(if not null)'s color. </param>
	/// <param name="tWorld"> The world transform to use. </param>
	/// <returns> If the box could be drawn. </returns>
	public static bool DepthBox( this Gizmo.GizmoDraw g, in BBox box, in Color cLines, in Color cSolid, Transform? tWorld = null )
	{
		if ( g is null )
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

				g.SolidBox( box );

				// Depth pass(more directly visible).
				g.IgnoreDepth = false;
				g.Color = cSolid.WithAlphaMultiplied( isSelected ? 1f : 0.1f );

				g.SolidBox( box );
			}

			g.LineThickness = isSelected ? 2f : 1f;

			// Depthless pass(see slightly through walls).
			g.IgnoreDepth = true;
			g.Color = cLines.WithAlphaMultiplied( isSelected ? 0.2f : 0.1f );

			g.LineBBox( box );

			// Depth pass(more directly visible).
			g.IgnoreDepth = false;
			g.Color = cLines.WithAlphaMultiplied( isSelected ? 1f : 0.2f );

			g.LineBBox( box );
		}

		return true;
	}

	/// <summary>
	/// Draws a slightly fancy box that better indicates depth.
	/// </summary>
	/// <param name="obj"></param>
	/// <param name="box"> The local bounds of the box. </param>
	/// <param name="cLines"> The line box(if not null)'s color. </param>
	/// <param name="cSolid"> The solid box(if not null)'s color. </param>
	/// <param name="tWorld"> The world transform to use. Defaults to the object's world transform. </param>
	/// <returns> If the box could be drawn. </returns>
	public static bool DrawBox( this GameObject obj, in BBox box, in Color cLines, in Color cSolid, Transform? tWorld = null )
	{
		if ( !obj.IsValid() )
			return false;

		return Gizmo.Draw.DepthBox( box, cLines, cSolid, tWorld ?? obj.WorldTransform );
	}

	/// <summary>
	/// Draws a slightly fancy box that better indicates depth.
	/// </summary>
	/// <param name="comp"></param>
	/// <param name="box"> The local bounds of the box. </param>
	/// <param name="cLines"> The line box(if not null)'s color. </param>
	/// <param name="cSolid"> The solid box(if not null)'s color. </param>
	/// <param name="tWorld"> The world transform to use. Defaults to the component's world transform. </param>
	/// <returns> If the box could be drawn. </returns>
	public static bool DrawBox( this Component comp, in BBox box, in Color cLines, in Color cSolid, Transform? tWorld = null )
		=> comp?.GameObject.DrawBox( box, cLines, cSolid, tWorld ) ?? false;
}
