using GameFish;

namespace Fishbox;

[Icon( "currency_exchange" )]
public partial class TycoonMode : Gamemode
{
	public override string Name { get; } = "Tycoon";
	public override string Description { get; } = "Produce stuff and sell it to make more stuff better.";
}
