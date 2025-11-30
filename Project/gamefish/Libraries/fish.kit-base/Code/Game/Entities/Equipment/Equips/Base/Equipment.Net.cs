using System.Text.Json.Serialization;

namespace GameFish;

partial class Equipment : Component.INetworkSpawn
{
	protected override bool? IsNetworkedOverride => true;

	protected override NetworkMode NetworkingModeDefault => NetworkMode.Object;
	protected override OwnerTransfer NetworkTransferModeDefault => OwnerTransfer.Fixed;
	protected override NetworkOrphaned NetworkOrphanedModeDefault => NetworkOrphaned.ClearOwner;

	public override Connection DefaultNetworkOwner => Pawn?.Network?.Owner;

	[Title( "Owner" )]
	[Property, JsonIgnore]
	[Feature( EQUIP ), Group( DEBUG )]
	protected BasePawn InspectorPawn => Pawn;

	[Sync( SyncFlags.FromHost )]
	public BasePawn Pawn
	{
		get => _owner;
		protected set
		{
			_owner = value;
			OnOwnerSet( _owner );
		}
	}

	protected BasePawn _owner;

	public override bool TrySetNetworkOwner( Connection cn, bool allowProxy = false )
		=> base.TrySetNetworkOwner( cn, allowProxy: allowProxy || Networking.IsHost );

	protected override void OnParentChanged( GameObject oldParent, GameObject newParent )
	{
		base.OnParentChanged( oldParent, newParent );

		if ( !this.InGame() )
			return;

		// Auto-attach to new parent pawns.
		if ( BasePawn.TryGet( newParent, out var newOwner ) )
			if ( Pawn != newOwner )
				SetOwner( newOwner );
	}

	public void OnNetworkSpawn( Connection owner )
	{
		if ( !Networking.IsHost || !GameObject.IsValid() )
			return;

		var pawn = Server.FindPawn( owner );

		if ( pawn.IsValid() && GameObject.IsAncestor( pawn.GameObject ) )
			SetOwner( pawn );
	}

	/// <summary>
	/// Allows the host to set assign the owner of the equipment.
	/// </summary>
	public void SetOwner( BasePawn owner )
	{
		if ( !Networking.IsHost )
			return;

		if ( !GameObject.IsValid() || this.InEditor() )
			return;

		var prevOwner = Network.Owner;

		if ( !owner.IsValid() && prevOwner is not null && prevOwner != owner.Network.Owner )
			Network.DropOwnership();

		if ( !owner.IsValid() )
		{
			// TODO: ditch that shit on the ground
			Network.DropOwnership();

			// Make sure we ain't under some guy still.
			if ( BasePawn.TryGet( GameObject, out _ ) )
				GameObject.SetParent( null );
		}
		else
		{
			var cn = owner?.Network?.Owner;

			// Can't change the parent if it's owned by a client.
			if ( Network.Owner is not null && cn != Network.Owner )
				Network.DropOwnership();

			// Keep that thang on 'em.. or not.
			if ( !GameObject.IsAncestor( owner.GameObject ) )
				GameObject.SetParent( owner.GameObject );

			// Force the owner to be a parent.
			Network.AssignOwnership( cn ?? Connection.Host );
		}

		Pawn = owner;
	}

	protected void OnOwnerSet( BasePawn owner )
	{
		if ( !this.InGame() || !GameObject.IsValid() )
			return;

		// Set it as dropped if we don't have an owner.
		if ( !owner.IsValid() )
		{
			EquipState = EquipState.Dropped;
			return;
		}

		var inv = owner.GetModule<PawnEquipment>();

		if ( IsProxy )
			goto OnEquip;

		if ( inv.IsValid() )
		{
			if ( inv.ActiveEquip.IsValid() )
			{
				// Always holstered if not the active equip.
				EquipState = inv.ActiveEquip == this
					? EquipState.Deployed
					: EquipState.Holstered;
			}
			else
			{
				// Auto-deploy if we have no active equip.
				// TODO: Have auto-equip be an option.
				if ( !inv.TryDeploy( this ) )
					EquipState = EquipState.Holstered;
			}
		}
		else
		{
			// If no inventory then it's dropped.
			EquipState = EquipState.Dropped;
		}

		// Call OnEquip on all clients.
		OnEquip:

		if ( Networking.IsHost && inv.IsValid() )
			inv.RefreshList();

		OnEquip( owner );
	}
}
