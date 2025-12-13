using GameFish;

namespace GameFish.Playground;

[Icon( "sentiment_very_dissatisfied" )]
public partial class DeathmatchMode : Gamemode
{
	public override string Name { get; } = "Deathmatch";
	public override string Description { get; } = "Kill.";
}
