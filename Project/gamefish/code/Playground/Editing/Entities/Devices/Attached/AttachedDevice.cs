namespace Fishbox;

/// <summary>
/// A device meant to be attached to an existing body/island.
/// </summary>
[Icon( "attachment" )]
[Title( "Attached Device" )]
public partial class AttachedDevice : Device
{
	protected const int SETTINGS_ORDER = EDITOR_ORDER + 50;

	public override bool IsWorthwhile => false;
	public override bool RefreshPhysicsUponJoin => false;
}
