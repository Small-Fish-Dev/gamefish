using System;
using GameFish;

namespace Playground.Razor;

partial class SpectatorHUD
{
	protected static Pawn Pawn => Client.Local?.Pawn;
	protected static Spectator Spectator => Pawn as Spectator;

	protected static Pawn Target => Spectator?.Spectating;

	protected static bool IsSpectating => Spectator.IsValid() && Target.IsValid();

	protected override int BuildHash()
		=> HashCode.Combine( Spectator, Target );
}
