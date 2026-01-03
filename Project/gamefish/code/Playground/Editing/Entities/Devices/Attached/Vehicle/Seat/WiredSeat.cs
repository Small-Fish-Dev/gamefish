namespace Playground;

public partial class WiredSeat : Seat, IWire, IPilot
{
	[Property]
	[Sync( SyncFlags.FromHost )]
	[Feature( SEAT ), Group( VEHICLE ), Order( SEAT_DEBUG_ORDER + 1 )]
	public Vector3 DriveInput { get; set; }

	public bool CanWire( IWire device )
		=> device is IPilot;

	public void WireSimulate( Device device, in float deltaTime, in bool isFixedUpdate )
	{
		if ( !Networking.IsHost )
			return;

		if ( Sitter.IsValid() && Sitter.Owner is Client cl )
			DriveInput = new( cl.InputHorizontal, cl.InputForward );
		else
			DriveInput = default;
	}
}
