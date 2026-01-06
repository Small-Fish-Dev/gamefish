using System;
using GameFish;
using Sandbox.UI;

namespace Fishbox.Razor;

partial class EditorPicker
{
	protected static Editor Editor => Editor.Instance;

	public static bool ShowCursor => Editor?.ShowCursor is true;

	public override bool WantsDrag => true;
	protected override bool WantsDragScrolling => false;

	public override bool WantsMouseInput()
		=> ShowCursor;


	protected override void OnParametersSet()
	{
		base.OnParametersSet();

		ButtonInput = PanelInputType.Game;
	}

	protected override void OnMouseOut( MousePanelEvent e )
	{
		base.OnMouseOut( e );

		// Hackfix to hopefully clear the drag state.
		// Sometimes it would act like you had a mouse button still held.

		Blur();
	}

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

	protected override int BuildHash()
		=> HashCode.Combine( Editor, ShowCursor );
}
