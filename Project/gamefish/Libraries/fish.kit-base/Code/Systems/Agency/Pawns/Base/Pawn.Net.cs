namespace GameFish;

partial class Pawn
{
	protected override bool? IsNetworkedOverride => true;

	protected override NetworkMode NetworkingModeDefault => NetworkMode.Object;
	protected override OwnerTransfer NetworkTransferModeDefault => OwnerTransfer.Fixed;
	protected override NetworkOrphaned NetworkOrphanedModeDefault => NetworkOrphaned.ClearOwner;

	public override Connection DefaultNetworkOwner => Network?.Owner ?? Owner?.Connection;

	protected override void OnEnabled()
	{
		Tags?.Add( TAG_PAWN );

		base.OnEnabled();

		SetupNetworking();
	}
}
