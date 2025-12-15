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
		if ( Input.Down( "Attack1" ) )
			Wind( deltaTime, Force );
		else if ( Input.Down( "Attack2" ) )
			Wind( deltaTime, -Force );
	}

	public void Wind( in float deltaTime, in float force )
	{
		if ( !Editor.TryTrace( Scene, out var tr ) )
			return;

		if ( !tr.GameObject.IsValid() )
			return;

		var vel = tr.Direction * force * deltaTime;

		if ( Editor.TryGetInstance( out var e ) )
			e.BroadcastImpulse( tr.GameObject, vel );
	}
}
