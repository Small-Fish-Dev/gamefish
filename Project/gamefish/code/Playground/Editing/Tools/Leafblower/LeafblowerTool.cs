using System.Runtime.CompilerServices;
using GameFish;

namespace Playground;

public sealed partial class LeafblowerTool : EditorTool
{
	private const int PHYSICS_ORDER = EDITOR_ORDER + 100;

	[Property]
	[Title( "Force" )]
	[Feature( EDITOR ), Group( PHYSICS ), Order( PHYSICS_ORDER )]
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
