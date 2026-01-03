namespace Playground;

/// <summary>
/// Allows wiring this up to devices.
/// </summary>
public partial interface IWire
{
	/// <summary>
	/// Decides if a specific device can be wired to this.
	/// </summary>
	public bool CanWire( IWire wire );

	/// <summary>
	/// Called by a device while it's connected to this.
	/// </summary>
	public void WireSimulate( Device device, in float deltaTime, in bool isFixedUpdate );
}
