using System;
using System.ComponentModel;

namespace GameFish;

/// <summary>
/// Cycles through a list of pawns.
/// </summary>
public partial class ClientPawnCyclerModule : Module
{
	[Property]
	[InputAction]
	[Feature( INPUT )]
	public string TogglePawnButton { get; set; } = "Info";
	public bool HasTogglePawnButton => !string.IsNullOrWhiteSpace( TogglePawnButton );

	/// <summary>
	/// These will be cycled through when the relevant button is pressed.
	/// </summary>
	[Feature( PAWN )]
	[Property, WideMode]
	[Title( "Toggle List" )]
	protected List<PrefabFile> PawnToggleList { get; set; }

	[Sync( SyncFlags.FromHost )]
	protected int PawnIndex { get; set; } = -1;

	public Agent ParentAgent => Parent as Agent;

	public override bool IsParent( ModuleEntity comp )
		=> comp.IsValid() && comp is Agent;

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( !this.IsOwner() )
			return;

		if ( HasTogglePawnButton && Input.Pressed( TogglePawnButton ) )
			CyclePawn();
	}

	[Rpc.Host( NetFlags.Reliable | NetFlags.OwnerOnly )]
	public void CyclePawn()
	{
		if ( PawnToggleList is null || PawnToggleList.Count <= 0 )
		{
			this.Warn( "No pawns to cycle through." );
			return;
		}

		PawnIndex = (PawnIndex + 1).UnsignedMod( PawnToggleList.Count );

		var prefab = PawnToggleList.ElementAtOrDefault( PawnIndex );

		if ( ParentAgent is not Client cl || !cl.IsValid() )
		{
			this.Warn( "No valid client parent." );
			return;
		}

		var oldPawn = cl.Pawn;
		var pawn = cl.CreatePawn( prefab );

		if ( !pawn.IsValid() )
		{
			this.Warn( $"failed to set pawn prefab of index {PawnIndex}" );
			return;
		}

		var vPrev = oldPawn?.WorldPosition;
		var rPrev = oldPawn?.WorldRotation;
		var viewPrev = oldPawn?.View?.WorldTransform;

		if ( vPrev.HasValue && ITransform.IsValid( vPrev.Value ) )
			pawn.WorldPosition = vPrev.Value;

		if ( rPrev.HasValue && ITransform.IsValid( rPrev.Value ) )
			pawn.WorldRotation = Rotation.FromYaw( rPrev.Value.Yaw() );

		if ( viewPrev is Transform tView && pawn.View is PawnView pv )
		{
			pv.ViewPosition = tView.Position;
			pv.ViewRotation = tView.Rotation;
		}

		this.Log( "Cycling to pawn:" + pawn );

		oldPawn?.GameObject?.Destroy();
	}
}
