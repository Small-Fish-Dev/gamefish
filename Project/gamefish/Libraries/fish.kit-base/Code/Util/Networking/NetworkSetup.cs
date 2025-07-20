namespace GameFish;

public static partial class GameFish
{
	/// <summary>
	/// Sets the object's network settings, network spawning if necessary and/or trying to set the owner.
	/// </summary>
	/// <param name="go"></param>
	/// <param name="cn"></param>
	/// <param name="orphanMode"></param>
	/// <param name="ownerTransfer"></param>
	/// <param name="netMode"></param>
	/// <param name="ignoreProxy"> Fails if the object is owned by another. </param>
	/// <returns> If the object was valid and we had permission. </returns>
	public static bool NetworkSetup( this GameObject go, Connection cn = null,
		NetworkOrphaned orphanMode = NetworkOrphaned.Host,
		OwnerTransfer ownerTransfer = OwnerTransfer.Fixed,
		NetworkMode netMode = NetworkMode.Object,
		bool ignoreProxy = true )
	{
		if ( !go.IsValid() )
			return false;

		if ( ignoreProxy && go.IsProxy )
			return false;

		go.NetworkMode = netMode;
		go.Network.SetOrphanedMode( orphanMode );
		go.Network.SetOwnerTransfer( ownerTransfer );

		if ( go.Network.Active )
		{
			if ( cn is null )
				return go.Network.DropOwnership();
			else
				return go.Network.AssignOwnership( cn );
		}
		else
		{
			return go.NetworkSpawn( true, cn );
		}
	}

	/// <returns> If this object is explicitly owned by the local client. </returns>
	public static bool IsOwner( this GameObject go ) => go.IsValid() && (go.Network?.IsOwner ?? false);
	/// <returns> If this component's object is explicitly owned by the local client. </returns>
	public static bool IsOwner( this Component comp ) => comp.IsValid() && (comp.Network?.IsOwner ?? false);
}
