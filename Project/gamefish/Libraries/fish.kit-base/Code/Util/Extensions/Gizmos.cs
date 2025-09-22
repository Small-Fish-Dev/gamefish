namespace GameFish;

public static partial class GizmoExtensions
{
	/// <summary>
	/// Draws a slightly fancy arrow that better indicates depth.
	/// </summary>
	/// <param name="g"></param>
	/// <param name="from"> The local origin of the arrow. </param>
	/// <param name="to"> The local end point of the arrow. </param>
	/// <param name="len"> The length of the arrow's head. </param>
	/// <param name="w"> The width of the arrow's head. </param>
	/// <param name="c"> The default color. </param>
	/// <param name="tWorld"> The world transform to use. Defaults to the object's world transform. </param>
	/// <returns> If the arrow could be drawn. </returns>
	public static bool DepthArrow( this Gizmo.GizmoDraw g, Vector3 from, Vector3 to, Color c, float? len = null, float? w = null, Transform? tWorld = null )
	{
		if ( g is null )
			return false;

		using ( Gizmo.Scope() )
		{
			if ( tWorld is Transform t )
				Gizmo.Transform = t;

			var arrowLen = len ?? 16f;
			var arrowWidth = w ?? (arrowLen * 0.3f);

			var isSelected = Gizmo.IsSelected;

			// Depthless pass(see slightly through walls).
			Gizmo.Draw.IgnoreDepth = true;
			Gizmo.Draw.LineThickness = 1f;
			Gizmo.Draw.Color = c.WithAlphaMultiplied( isSelected ? 0.2f : 0.1f );

			// Gizmo.Draw.Line( from, to );
			Gizmo.Draw.Arrow( from, to, arrowLen, arrowWidth );

			// Depth pass(more directly visible).
			Gizmo.Draw.IgnoreDepth = false;
			Gizmo.Draw.LineThickness = isSelected ? 2f : 1f;
			Gizmo.Draw.Color = c;

			Gizmo.Draw.Line( from, to );
			Gizmo.Draw.Arrow( from, to, arrowLen, arrowWidth );
		}

		return true;
	}

	/// <summary>
	/// Draws a slightly fancy arrow that better indicates depth.
	/// </summary>
	/// <param name="obj"></param>
	/// <param name="from"> The local origin of the arrow. </param>
	/// <param name="to"> The local end point of the arrow. </param>
	/// <param name="len"> The length of the arrow's head. </param>
	/// <param name="w"> The width of the arrow's head. </param>
	/// <param name="c"> The default color. </param>
	/// <param name="tWorld"> The world transform to use. Defaults to the object's world transform. </param>
	/// <returns> If the arrow could be drawn. </returns>
	public static bool DrawArrow( this GameObject obj, Vector3 from, Vector3 to, Color c, float? len = null, float? w = null, Transform? tWorld = null )
	{
		if ( !obj.IsValid() )
			return false;

		return Gizmo.Draw.DepthArrow( from, to, c, len, w, tWorld ?? obj.WorldTransform );
	}

	/// <summary>
	/// Draws a slightly fancy arrow that better indicates depth.
	/// </summary>
	/// <param name="comp"></param>
	/// <param name="from"> The local origin of the arrow. </param>
	/// <param name="to"> The local end point of the arrow. </param>
	/// <param name="len"> The length of the arrow's head. </param>
	/// <param name="w"> The width of the arrow's head. </param>
	/// <param name="c"> The default color. </param>
	/// <param name="tWorld"> The world transform to use. Defaults to the component's world transform. </param>
	/// <returns> If the arrow could be drawn. </returns>
	public static bool DrawArrow( this Component comp, Vector3 from, Vector3 to, Color c, float? len = null, float? w = null, Transform? tWorld = null )
		=> comp?.GameObject.DrawArrow( from, to, c, len, w, tWorld ?? comp.WorldTransform ) ?? false;

	/// <summary>
	/// Draws a slightly fancy box that better indicates depth.
	/// </summary>
	/// <param name="g"></param>
	/// <param name="box"> The local bounds of the box. </param>
	/// <param name="cLines"> The line box(if not null)'s color. </param>
	/// <param name="cSolid"> The solid box(if not null)'s color. </param>
	/// <param name="tWorld"> The world transform to use. </param>
	/// <returns> If the box could be drawn. </returns>
	public static bool DepthBox( this Gizmo.GizmoDraw g, in BBox box, Color cLines, Color cSolid, Transform? tWorld = null )
	{
		if ( g is null )
			return false;

		using ( Gizmo.Scope() )
		{
			if ( tWorld is Transform t )
				Gizmo.Transform = t;

			var isSelected = Gizmo.IsSelected;

			// Depthless pass(see slightly through walls).
			g.IgnoreDepth = true;
			g.Color = cLines.WithAlphaMultiplied( isSelected ? 0.2f : 0.1f );

			g.LineBBox( box );

			// Depth pass(more directly visible).
			g.IgnoreDepth = false;
			g.Color = cLines.WithAlphaMultiplied( isSelected ? 1f : 0.2f );

			g.LineBBox( box );

			// Depthless pass(see slightly through walls).
			g.IgnoreDepth = true;
			g.LineThickness = 1f;
			g.Color = cSolid.WithAlphaMultiplied( 0.2f );

			g.SolidBox( box );

			// Depth pass(more directly visible).
			g.IgnoreDepth = false;
			g.LineThickness = isSelected ? 2f : 1f;
			g.Color = cSolid.WithAlphaMultiplied( isSelected ? 1f : 0.1f );

			g.SolidBox( box );
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
	public static bool DrawBox( this GameObject obj, in BBox box, Color cLines, Color cSolid, Transform? tWorld = null )
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
	public static bool DrawBox( this Component comp, in BBox box, Color cLines, Color cSolid, Transform? tWorld = null )
		=> comp?.GameObject.DrawBox( box, cLines, cSolid, tWorld ) ?? false;

	/// <summary>
	/// Draws a slightly fancy sphere that better indicates depth.
	/// </summary>
	/// <param name="g"></param>
	/// <param name="radius"> The radius of the sphere. </param>
	/// <param name="center"> The offset of the sphere from world transform. </param>
	/// <param name="c"> The default color. </param>
	/// <param name="tWorld"> The world transform to use. Defaults to the object's world transform. </param>
	/// <returns> If the sphere could be drawn. </returns>
	public static bool DepthSphere( this Gizmo.GizmoDraw g, in float radius, in Vector3 center, Color c, Transform? tWorld = null )
	{
		if ( g is null || radius == 0f )
			return false;

		using ( Gizmo.Scope() )
		{
			if ( tWorld is Transform t )
				Gizmo.Transform = t;

			var isSelected = Gizmo.IsSelected;

			// Depthless pass(see slightly through walls).
			g.IgnoreDepth = true;
			g.LineThickness = 1f;
			g.Color = c.WithAlphaMultiplied( isSelected ? 0.2f : 0.1f );

			g.LineSphere( center, radius );

			// Depth pass(more directly visible).
			g.IgnoreDepth = false;
			g.LineThickness = isSelected ? 2f : 1f;
			g.Color = c;

			g.LineSphere( center, radius );
		}

		return true;
	}

	/// <summary>
	/// Draws a slightly fancy sphere that better indicates depth.
	/// </summary>
	/// <param name="obj"></param>
	/// <param name="radius"> The radius of the sphere. </param>
	/// <param name="center"> The offset of the sphere from world transform. </param>
	/// <param name="c"> The default color. </param>
	/// <param name="tWorld"> The world transform to use. Defaults to the object's world transform. </param>
	/// <returns> If the sphere could be drawn. </returns>
	public static bool DrawSphere( this GameObject obj, in float radius, in Vector3 center, Color c, Transform? tWorld = null )
	{
		if ( !obj.IsValid() )
			return false;

		return Gizmo.Draw.DepthSphere( radius, center, c, tWorld ?? obj.WorldTransform );
	}

	/// <summary>
	/// Draws a slightly fancy sphere that better indicates depth.
	/// </summary>
	/// <param name="comp"></param>
	/// <param name="radius"> The radius of the sphere. </param>
	/// <param name="center"> The offset of the sphere from world transform. </param>
	/// <param name="c"> The default color. </param>
	/// <param name="tWorld"> The world transform to use. Defaults to the component's world transform. </param>
	/// <returns> If the sphere could be drawn. </returns>
	public static bool DrawSphere( this Component comp, in float radius, in Vector3 center, Color c, Transform? tWorld = null )
		=> comp?.GameObject.DrawSphere( radius, center, c, tWorld ?? comp.WorldTransform ) ?? false;
}
