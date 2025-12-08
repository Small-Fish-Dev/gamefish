namespace GameFish;

partial class BaseController
{
	protected override bool IsNetworkSetupAllowed() => false;
	protected override bool? IsNetworkedOverride => false;

	protected override void SetupNetworking( bool force = false ) { }
}
