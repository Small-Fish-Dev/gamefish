namespace GameFish;

/// <summary>
/// Handles direct pawn interaction.
/// </summary>
public interface IUsable
{
	/// <summary>
	/// Lets this be used over other possible usables.
	/// Lower numbers should indicate a higher priority(useful with distance/angle).
	/// </summary>
	public float UsablePriority { get; }

	/// <returns> If that pawn is allowed to use this. </returns>
	public bool IsUsable( Pawn pawn );

	/// <summary>
	/// Networks the request to use this.
	/// </summary>
	public void RpcUse();

	/// <returns> If it's usable and we tried to do so. </returns>
	public virtual bool TryUse( Pawn pawn )
	{
		if ( !IsUsable( pawn ) )
			return false;

		RpcUse();
		return true;
	}
}
