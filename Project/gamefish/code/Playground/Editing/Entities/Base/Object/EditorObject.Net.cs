namespace Fishbox;

partial class EditorObject
{
	protected override bool? IsNetworkedOverride => true;
	protected override bool IsNetworkedAutomatically => true;

	protected override NetworkMode NetworkingModeDefault => NetworkMode.Object;
	protected override OwnerTransfer NetworkTransferModeDefault => OwnerTransfer.Takeover;
	protected override NetworkOrphaned NetworkOrphanedModeDefault => NetworkOrphaned.Host;

	public override Connection DefaultNetworkOwner => Connection.Host;
}
