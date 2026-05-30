namespace GameFish;

partial class Library
{
	/// <summary>
	/// Draws a slightly fancy arrow that better indicates depth.
	/// </summary>
	/// <param name="g"></param>
	/// <param name="from"> The local origin of the arrow. </param>
	/// <param name="to"> The local end point of the arrow. </param>
	/// <param name="th"> The base line thickness. </param>
	/// <param name="len"> The length of the arrow's head. </param>
	/// <param name="w"> The width of the arrow's head. </param>
	/// <param name="c"> The default color. </param>
	/// <param name="tWorld"> The world transform to use. Defaults to the object's world transform. </param>
	/// <returns> If the arrow could be drawn. </returns>
	public static bool DepthArrow( this Gizmo.GizmoDraw g, Vector3 from, Vector3 to, Color c, float th = 1f, float? len = null, float? w = null, Transform? tWorld = null )
	{
		if ( g is null )
			return false;

		using ( Gizmo.Scope() )
		{
			if ( tWorld is Transform t )
				Gizmo.Transform = t;

			var arrowLen = len ?? 16f;
			var arrowWidth = w ?? (arrowLen * 0.3f);

			var isSelected = IsHighlighting;

			g.LineThickness = isSelected ? (th * 1.3f).CeilToInt() : th;

			// Depthless pass(see slightly through walls).
			g.IgnoreDepth = true;
			g.Color = c.WithAlphaMultiplied( isSelected ? 0.2f : 0.1f );

			g.Arrow( from, to, arrowLen, arrowWidth );

			// Depth pass(more directly visible).
			g.IgnoreDepth = false;
			g.Color = c.WithAlphaMultiplied( isSelected ? 1f : 0.5f );

			g.Arrow( from, to, arrowLen, arrowWidth );
		}

		return true;
	}

	/// <summary>
	/// Draws a slightly fancy arrow that better indicates depth.
	/// </summary>
	/// <param name="obj"></param>
	/// <param name="from"> The local origin of the arrow. </param>
	/// <param name="to"> The local end point of the arrow. </param>
	/// <param name="th"> The base line thickness. </param>
	/// <param name="len"> The length of the arrow's head. </param>
	/// <param name="w"> The width of the arrow's head. </param>
	/// <param name="c"> The default color. </param>
	/// <param name="tWorld"> The world transform to use. Defaults to the object's world transform. </param>
	/// <returns> If the arrow could be drawn. </returns>
	public static bool DrawArrow( this GameObject obj, Vector3 from, Vector3 to, Color c, float th = 1f, float? len = null, float? w = null, Transform? tWorld = null )
	{
		if ( !obj.IsValid() )
			return false;

		return Gizmo.Draw.DepthArrow( from, to, c, th, len, w, tWorld ?? obj.WorldTransform );
	}

	/// <summary>
	/// Draws a slightly fancy arrow that better indicates depth.
	/// </summary>
	/// <param name="comp"></param>
	/// <param name="from"> The local origin of the arrow. </param>
	/// <param name="to"> The local end point of the arrow. </param>
	/// <param name="th"> The base line thickness. </param>
	/// <param name="len"> The length of the arrow's head. </param>
	/// <param name="w"> The width of the arrow's head. </param>
	/// <param name="c"> The default color. </param>
	/// <param name="tWorld"> The world transform to use. Defaults to the component's world transform. </param>
	/// <returns> If the arrow could be drawn. </returns>
	public static bool DrawArrow( this Component comp, Vector3 from, Vector3 to, Color c, float th = 1f, float? len = null, float? w = null, Transform? tWorld = null )
		=> comp?.GameObject.DrawArrow( from, to, c, th, len, w, tWorld ?? comp.WorldTransform ) ?? false;
}
