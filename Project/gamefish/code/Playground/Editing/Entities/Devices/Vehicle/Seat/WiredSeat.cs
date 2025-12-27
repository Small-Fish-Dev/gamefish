namespace Playground;

public partial class WiredSeat : Seat, IWired
{
	[Property]
	[Sync( SyncFlags.FromHost )]
	[Feature( SEAT ), Group( DEBUG ), Order( SEAT_DEBUG_ORDER + 1 )]
	public Vector2 DriveInput { get; set; }

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
