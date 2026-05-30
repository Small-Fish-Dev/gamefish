namespace GameFish;

/// <summary>
/// An entity meant for logical processing.
/// </summary>
[Icon( "code" )]
public abstract partial class LogicEntity : Entity
{
	protected const int LOGIC_ORDER = DEFAULT_ORDER - 1000;

	protected const int LOGIC_DEBUG_ORDER = LOGIC_ORDER - 50;
	protected const int LOGIC_FUNCTIONS_ORDER = LOGIC_ORDER + 200;

	protected override bool? IsNetworkedOverride => true;
	protected override bool IsNetworkedAutomatically => true;

	protected override NetworkMode NetworkingModeDefault => NetworkMode.Object;
	protected override OwnerTransfer NetworkTransferModeDefault => OwnerTransfer.Fixed;
	protected override NetworkOrphaned NetworkOrphanedModeDefault => NetworkOrphaned.ClearOwner;
}
