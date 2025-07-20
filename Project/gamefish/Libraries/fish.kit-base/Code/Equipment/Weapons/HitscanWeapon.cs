namespace GameFish;

/// <summary>
/// 🚀 Shoots bullets.
/// </summary>
[Icon( "clear_all" )]
public partial class HitscanWeapon : AmmoEquip
{
	public override void OnPrimary()
	{
		base.OnPrimary();

		BulletTrace( AimDirection );
	}

	public virtual SceneTraceResult BulletTrace( Vector3 dir, Vector3? origin = null )
	{
		origin ??= AimPosition;

		return default;
	}
}
