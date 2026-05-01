namespace GameFish;

partial class PawnController
{
	protected override bool IsNetworkSetupAllowed() => false;
	protected override bool? IsNetworkedOverride => false;

	public override void SetupNetworking( bool force = false ) { }
}
