using System;
using GameFish;
using Sandbox.UI;

namespace Playground.Razor;

partial class EditorPicker
{
	protected static Editor Editor => Editor.Instance;

	public static bool ShowCursor => Editor?.ShowCursor is true;

	public override bool WantsMouseInput()
		=> ShowCursor;

	public override bool WantsDrag => true;
	protected override bool WantsDragScrolling => false;

	protected override void OnClick( MousePanelEvent e )
	{
		base.OnClick( e );

		if ( Editor.IsValid() )
			Editor.OnLeftClick();
	}

	protected override void OnRightClick( MousePanelEvent e )
	{
		base.OnRightClick( e );

		if ( Editor.IsValid() )
			Editor.OnRightClick();
	}

	protected override void OnMiddleClick( MousePanelEvent e )
	{
		base.OnMiddleClick( e );

		if ( Editor.IsValid() )
			Editor.OnMiddleClick();
	}

	protected override void OnMouseUp( MousePanelEvent e )
	{
		base.OnMouseUp( e );

		if ( Editor.IsValid() )
			Editor.OnMouseUp( e.MouseButton );
	}

	public override void OnMouseWheel( Vector2 value )
	{
		base.OnMouseWheel( value );

		if ( Editor.IsValid() )
			Editor.OnMouseWheel( in value );
	}

	protected override void OnDrag( DragEvent e )
	{
		base.OnDrag( e );

		if ( Editor.IsValid() )
			Editor.OnMouseDrag( e.MouseDelta );
	}

	protected override void OnDragEnd( DragEvent e )
	{
		base.OnDragEnd( e );

		if ( Editor.IsValid() )
			Editor.OnMouseDragEnd();
	}

	protected override int BuildHash()
		=> HashCode.Combine( Editor, ShowCursor );
}
