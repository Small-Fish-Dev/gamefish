namespace Playground;

public sealed partial class LeafblowerTool : EditorTool
{
	[Property]
	[Title( "Force" )]
	[Sync( SyncFlags.FromHost )]
	[Feature( EDITOR ), Group( SETTINGS ), Order( SETTINGS_ORDER )]
	public float Force { get; set; } = 400f;

	public override void FixedSimulate( in float deltaTime )
	{
		if ( HoldingPrimary )
			Wind( deltaTime, Force );
		else if ( HoldingSecondary )
			Wind( deltaTime, -Force );
	}

	public void Wind( in float deltaTime, in float force )
	{
		if ( !TryTrace( out var tr ) )
			return;

		if ( !tr.Hit || !tr.GameObject.IsValid() )
			return;

		var vel = tr.Direction * force * deltaTime;

		if ( Editor.TryGetInstance( out var e ) )
			e.BroadcastImpulse( tr.GameObject, vel );
	}
}
