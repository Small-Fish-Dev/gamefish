namespace GameFish;

partial class BaseTrigger
{
	protected override bool? IsNetworkedOverride => true;

	protected override NetworkMode NetworkingModeDefault => NetworkMode.Object;
	protected override OwnerTransfer NetworkTransferModeDefault => OwnerTransfer.Fixed;
	protected override NetworkOrphaned NetworkOrphanedModeDefault => NetworkOrphaned.Host;

	public override Connection DefaultNetworkOwner => Network?.Owner;
}
