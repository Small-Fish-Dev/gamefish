namespace GameFish;

[Icon( "favorite" )]
public partial class HealthPickup : Pickup
{
	/// <summary>
	/// How much health is restored?
	/// </summary>
	[Property]
	[Order( PICKUP_EFFECT_ORDER )]
	[Sync( SyncFlags.FromHost )]
	[Feature( PICKUP ), Group( HEALTH )]
	public float Heal { get; set; } = 50f;

	public override bool CanPickup( Player pl )
	{
		if ( !base.CanPickup( pl ) )
			return false;

		return pl.Health < pl.MaxHealth;
	}

	protected override void OnPickup( Player pl )
	{
		pl.RpcHostModifyHealth( Heal );
	}
}
