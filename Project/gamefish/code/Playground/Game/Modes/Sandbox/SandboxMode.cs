using GameFish;

namespace GameFish.Playground;

[Icon( "handyman" )]
public partial class SandboxMode : Gamemode
{
	public override string Name { get; } = "Sandbox";
	public override string Description { get; } = "Spawn, edit and play around with stuff.";
}
