namespace Fishbox;

[Group( NAME )]
public abstract partial class FishboxModule : Module
{
	protected const int GAME_ORDER = DEFAULT_ORDER - 1000;
}
