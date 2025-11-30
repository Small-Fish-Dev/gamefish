namespace GameFish;

partial class Library
{
	public const FindMode DefaultFindMode = FindMode.EnabledInSelf | FindMode.InAncestors;

	/// <summary>
	/// Looks for <see cref="IHealth"/> and checks <see cref="IHealth.CanDamage(in DamageInfo)"/>
	/// to see if it can call the <see cref="Component.IDamageable.OnDamage(in DamageInfo)"/>. <br />
	/// </summary>
	/// <returns> If we were able to find and bother sending our damage. </returns>
	public static bool TryDamage( this GameObject obj, in DamageInfo dmgInfo, in FindMode findMode = DefaultFindMode )
	{
		if ( !obj.IsValid() )
			return false;

		foreach ( var hp in obj.Components.GetAll<IHealth>( findMode ) )
			if ( hp.TryDamage( in dmgInfo ) )
				return true;

		return false;
	}
}
