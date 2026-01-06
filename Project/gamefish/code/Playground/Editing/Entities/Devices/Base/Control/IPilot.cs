namespace Fishbox;

/// <summary>
/// Provides an analogue input reference.
/// </summary>
public partial interface IPilot
{
	public Vector3 DriveInput { get; set; }
}
