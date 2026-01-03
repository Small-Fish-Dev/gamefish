namespace Playground;

/// <summary>
/// A device meant to be attached to an existing body/island.
/// </summary>
[Icon( "attachment" )]
[Title( "Attached Device" )]
public partial class AttachDevice : Device
{
	protected const int SETTINGS_ORDER = EDITOR_ORDER + 50;

	public override bool RefreshPhysicsBody => false;
}
