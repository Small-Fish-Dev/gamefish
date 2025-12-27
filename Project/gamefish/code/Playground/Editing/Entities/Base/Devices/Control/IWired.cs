namespace Playground;

/// <summary>
/// Allows wiring this up to devices.
/// </summary>
public partial interface IWired
{
	/// <summary>
	/// Called by a device while it's connected to this.
	/// </summary>
	public void WireSimulate( Device device, in float deltaTime, in bool isFixedUpdate );
}
