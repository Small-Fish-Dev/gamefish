namespace Playground;

partial class EditorTool
{
	protected virtual Color ColorOutline => Color.Black.WithAlpha( 0.5f );
	protected virtual Color ColorFilled => Color.White.WithAlpha( 0.1f );
	protected virtual Color ColorArrow => Color.Black.WithAlpha( 0.8f );

	protected virtual void RenderHelpers()
	{
		RenderPointer();
	}

	protected virtual bool TryGetPointer( in SceneTraceResult tr, out Transform tCursor )
	{
		tCursor = tr.Hit
			? new( tr.EndPosition, Rotation.LookAt( tr.Normal ) )
			: new( tr.EndPosition );

		return true;
	}

	protected virtual void RenderPointer()
	{
		if ( TryGetPointer( TargetTrace, out var tCursor ) )
			RenderPointer( tCursor );
	}

	protected virtual void RenderPointer( in Transform tCursor )
	{
		var cBlack = Color.Black.WithAlpha( 0.3f );
		var cWhite = Color.White.WithAlpha( 1.2f );

		this.DrawSphere( 2.5f, Vector3.Zero, Color.Transparent, cBlack, tCursor );
		this.DrawSphere( 1.5f, Vector3.Zero, Color.Transparent, cWhite, tCursor );
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
