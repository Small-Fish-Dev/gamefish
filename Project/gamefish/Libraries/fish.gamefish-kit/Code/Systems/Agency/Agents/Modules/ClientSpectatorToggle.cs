using System;

namespace GameFish;

/// <summary>
/// Toggles between spectator mode and the default pawn.
/// </summary>
public partial class ClientSpectatorToggle : Module
{
	[Property]
	[InputAction]
	[Feature( DEBUG ), Group( INPUT ), Order( DEBUG_ORDER )]
	public string TogglePawnButton { get; set; } = "Toggle Spectating";

	public Client Client => Parent as Client;
	public Pawn Pawn => Client?.Pawn;

	public override bool IsParent( ModuleEntity comp )
		=> comp.IsValid() && comp is Client;

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( !this.IsOwner() )
			return;

		if ( !Client.IsValid() || Client.IsBot )
			return;

		if ( Input.Pressed( TogglePawnButton ) )
			Toggle();
	}

	public virtual void Toggle()
	{
		if ( !GameState.TryGetCurrent( out _ ) )
			this.Warn( "No game state found!" );

		RpcToggle();
	}

	[Rpc.Host( NetFlags.Reliable | NetFlags.OwnerOnly )]
	protected void RpcToggle()
	{
		if ( !Client.IsValid() )
			return;

		if ( !GameState.TryGetCurrent( out var s ) )
			return;

		var tPrev = Client.Pawn?.EyeTransform;

		if ( Pawn is Spectator )
			s.TryAssignPlayer( Client, out _, force: true, oldCleanup: true );
		else
			s.TryAssignSpectator( Client, out _, force: true, oldCleanup: true );

		if ( tPrev.HasValue && Pawn.IsValid() )
			Pawn.RpcHostTeleport( tPrev.Value );
	}
}
