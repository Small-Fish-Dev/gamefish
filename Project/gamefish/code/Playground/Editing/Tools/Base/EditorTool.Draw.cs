namespace Playground;

partial class EditorTool
{
	protected virtual Color ColorOutline => Color.Black.WithAlpha( 0.5f );
	protected virtual Color ColorFilled => Color.White.WithAlpha( 0.1f );
	protected virtual Color ColorArrow => Color.Black.WithAlpha( 0.8f );

	public virtual bool ShowPointerTransform => HasTarget || HasOrigin;

	protected virtual void RenderHelpers()
	{
		RenderPointer();
	}

	protected virtual void RenderPointer()
	{
		if ( !TryGetPointer( TargetTrace, out var tPointer ) )
			return;

		RenderPointerSphere( tPointer );

		if ( ShowPointerTransform )
		{
			var tGizmo = HasOrigin
				? OriginWorldTransform.WithOffset( OriginOffset )
				: tPointer;

			RenderTransform( tGizmo );
		}
	}

	protected virtual void RenderPointerSphere( in Transform tPointer )
	{
		var cBlack = Color.Black.WithAlpha( 0.3f );
		var cWhite = Color.White.WithAlpha( 1.2f );

		this.DrawSphere( 2.5f, Vector3.Zero, Color.Transparent, cBlack, tPointer );
		this.DrawSphere( 1.5f, Vector3.Zero, Color.Transparent, cWhite, tPointer );
	}

	protected virtual void RenderTransform( in Transform tWorld )
	{
		const float lineLen = 22f;
		const float lineThick = 0.8f;
		const float aLen = 5f;
		const float aWidth = 2f;

		const float a = 0.75f;

		var cUp = new ColorHsv( 200, .9f, .9f ).WithAlpha( a );
		var cFwd = new ColorHsv( 7, .62f, .85f ).WithAlpha( a );
		var cRight = new ColorHsv( 103, .6f, .84f ).WithAlpha( a );

		this.DrawArrow( Vector3.Zero, Vector3.Up * lineLen, cUp, tWorld: tWorld, th: lineThick, len: aLen, w: aWidth );
		this.DrawArrow( Vector3.Zero, Vector3.Forward * lineLen, cFwd, tWorld: tWorld, th: lineThick, len: aLen, w: aWidth );
		this.DrawArrow( Vector3.Zero, Vector3.Right * lineLen, cRight, tWorld: tWorld, th: lineThick, len: aLen, w: aWidth );
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
