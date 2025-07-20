namespace GameFish;

public static partial class GizmoExtensions
{
	/// <summary>
	/// Draws a slightly fancy arrow that better indicates depth.
	/// </summary>
	/// <param name="comp"> The component drawing this. </param>
	/// <param name="from"> The local origin of the arrow. </param>
	/// <param name="to"> The local end point of the arrow. </param>
	/// <param name="len"> The length of the arrow's head. </param>
	/// <param name="w"> The width of the arrow's head. </param>
	/// <param name="c"> The default color. </param>
	/// <param name="tWorld"> The world transform to use. Defaults to the component's world transform. </param>
	/// <returns> If the arrow could be drawn. </returns>
	public static bool DrawArrow( this Component comp, Vector3 from, Vector3 to, Color c, float? len = null, float? w = null, Transform? tWorld = null )
	{
		if ( !comp.IsValid() )
			return false;

		using ( Gizmo.Scope( comp.Id + "-box" ) )
		{
			var t = tWorld ??= comp.WorldTransform;

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
	/// Draws a slightly fancy box that better indicates depth.
	/// </summary>
	/// <param name="comp"> The component drawing this. </param>
	/// <param name="box"> The local bounds of the box. </param>
	/// <param name="c"> The default color. </param>
	/// <param name="tWorld"> The world transform to use. Defaults to the component's world transform. </param>
	/// <returns> If the box could be drawn. </returns>
	public static bool DrawBox( this Component comp, in BBox box, Color c, Transform? tWorld = null )
	{
		if ( !comp.IsValid() )
			return false;

		using ( Gizmo.Scope( comp.Id + "-box", Transform.Zero ) )
		{
			Gizmo.Transform = tWorld ??= comp.WorldTransform;

			var isSelected = Gizmo.IsSelected;

			// Depthless pass(see slightly through walls).
			Gizmo.Draw.IgnoreDepth = true;
			Gizmo.Draw.LineThickness = 1f;
			Gizmo.Draw.Color = c.WithAlphaMultiplied( isSelected ? 0.2f : 0.1f );

			Gizmo.Draw.LineBBox( box );

			// Depth pass(more directly visible).
			Gizmo.Draw.IgnoreDepth = false;
			Gizmo.Draw.LineThickness = isSelected ? 2f : 1f;
			Gizmo.Draw.Color = c;

			Gizmo.Draw.LineBBox( box );
		}

		return true;
	}

	/// <summary>
	/// Draws a slightly fancy sphere that better indicates depth.
	/// </summary>
	/// <param name="comp"> The component drawing this. </param>
	/// <param name="radius"> The radius of the sphere. </param>
	/// <param name="center"> The offset of the sphere from world transform. </param>
	/// <param name="c"> The default color. </param>
	/// <param name="tWorld"> The world transform to use. Defaults to the component's world transform. </param>
	/// <returns> If the sphere could be drawn. </returns>
	public static bool DrawSphere( this Component comp, in float radius, in Vector3 center, Color c, Transform? tWorld = null )
	{
		if ( !comp.IsValid() || radius == 0f )
			return false;

		using ( Gizmo.Scope( comp.Id + "-sphere", Transform.Zero ) )
		{
			Gizmo.Transform = tWorld ??= comp.WorldTransform;

			var isSelected = Gizmo.IsSelected;

			// Depthless pass(see slightly through walls).
			Gizmo.Draw.IgnoreDepth = true;
			Gizmo.Draw.LineThickness = 1f;
			Gizmo.Draw.Color = c.WithAlphaMultiplied( isSelected ? 0.2f : 0.1f );

			Gizmo.Draw.LineSphere( center, radius );

			// Depth pass(more directly visible).
			Gizmo.Draw.IgnoreDepth = false;
			Gizmo.Draw.LineThickness = isSelected ? 2f : 1f;
			Gizmo.Draw.Color = c;

			Gizmo.Draw.LineSphere( center, radius );
		}

		return true;
	}
}
