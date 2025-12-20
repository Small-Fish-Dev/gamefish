using System;
using GameFish;
using Sandbox.UI;

namespace Playground.Razor;

partial class WorldPicker
{
	protected static Editor Editor => Editor.Instance;

	protected static EditorTool ActiveTool => Editor?.Tool;

	public override bool WantsDrag => true;
	protected override bool WantsDragScrolling => false;

	protected override void OnClick( MousePanelEvent e )
	{
		base.OnClick( e );

		if ( ActiveTool.IsValid() )
			ActiveTool.OnLeftClick();
	}

	protected override void OnRightClick( MousePanelEvent e )
	{
		base.OnRightClick( e );

		if ( ActiveTool.IsValid() )
			ActiveTool.OnRightClick();
	}

	protected override void OnMiddleClick( MousePanelEvent e )
	{
		base.OnMiddleClick( e );

		if ( ActiveTool.IsValid() )
			ActiveTool.OnMiddleClick();
	}

	protected override void OnMouseUp( MousePanelEvent e )
	{
		base.OnMouseUp( e );

		if ( ActiveTool.IsValid() )
			ActiveTool.OnMouseUp( e.MouseButton );
	}

	public override void OnMouseWheel( Vector2 value )
	{
		base.OnMouseWheel( value );

		if ( ActiveTool.IsValid() )
			ActiveTool.OnMouseWheel( in value );
	}

	protected override void OnDrag( DragEvent e )
	{
		base.OnDrag( e );

		if ( ActiveTool.IsValid() )
			ActiveTool.OnMouseDrag( e.MouseDelta );
	}

	protected override void OnDragEnd( DragEvent e )
	{
		base.OnDragEnd( e );

		if ( ActiveTool.IsValid() )
			ActiveTool.OnMouseDragEnd();
	}

	protected override int BuildHash()
		=> HashCode.Combine( Editor );
}
