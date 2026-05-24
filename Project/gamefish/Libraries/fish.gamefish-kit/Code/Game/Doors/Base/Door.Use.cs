namespace GameFish;

partial class Door : IUsable
{
	[Rpc.Owner]
	public void RpcUse()
		=> TryToggle();

	public float UseOrder( Pawn pawn )
	{
		if ( !pawn.IsValid() )
			return float.MaxValue;

		// TODO: Move this to the pawn.
		var doorPos = Center;
		var pawnPos = pawn.EyePosition;

		var dist = doorPos.Distance( pawnPos );
		var dir = doorPos.Direction( pawnPos );
		var dot = dir.Dot( pawn.EyeForward );

		return dist * dot;
	}
}
