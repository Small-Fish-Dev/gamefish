using System;
using GameFish;

namespace Playground.Razor;

partial class HUD
{
	protected static Pawn Pawn => Client.Local?.Pawn;
	protected static Pawn Spectator => Pawn as Spectator;

	protected static bool IsAlive => Pawn?.IsAlive is true;

	protected override int BuildHash()
		=> HashCode.Combine( Pawn, Spectator, IsAlive );
}
