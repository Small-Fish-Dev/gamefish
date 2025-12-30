namespace Playground;

partial class EditorTool
{
	protected virtual Color ColorOutline => Color.Black.WithAlpha( 0.5f );
	protected virtual Color ColorFilled => Color.White.WithAlpha( 0.1f );
	protected virtual Color ColorArrow => Color.Black.WithAlpha( 0.8f );

	protected virtual void RenderHelpers()
	{
		RenderCursor();
	}

	protected virtual bool TryGetCursorPosition( out Vector3 cursorPos )
	{
		if ( !TryTrace( out var tr ) )
		{
			cursorPos = Scene?.Camera?.WorldPosition ?? default;
			return false;
		}

		cursorPos = tr.EndPosition;
		return false;
	}

	protected virtual void RenderCursor()
	{
		if ( TryGetCursorPosition( out var cursorPos ) )
			RenderCursor( cursorPos );
	}

	protected virtual void RenderCursor( in Vector3 pos )
	{
		var cSphere = Color.White.WithAlpha( 0.4f );

		this.DrawSphere( 2.5f, pos, Color.Transparent, cSphere, global::Transform.Zero );
	}

	/*
	protected virtual void RenderGrid( in Transform tWorld, in float size, in int rows, in int columns )
	{
		void DrawLine( in float)

		for ( var iRow = 0; iRow < rows; iRow++ )
			for ( var iColumn = 0; iColumn < columns; iColumn++ )
				DrawLine( tWorld, );
	}
	*/
}
