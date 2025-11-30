namespace GameFish;

partial class Module
{
	protected override bool? IsNetworkedOverride => true;
	protected override bool IsNetworkedAutomatically => true;

	protected override NetworkMode NetworkingModeDefault => NetworkMode.Object;
	protected override OwnerTransfer NetworkTransferModeDefault => OwnerTransfer.Fixed;
	protected override NetworkOrphaned NetworkOrphanedModeDefault => NetworkOrphaned.ClearOwner;

	public override Connection DefaultNetworkOwner => Parent?.Network?.Owner ?? Parent?.DefaultNetworkOwner;
}
