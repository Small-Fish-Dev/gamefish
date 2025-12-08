namespace GameFish;

partial class Library
{
	public const FindMode DefaultFindMode = FindMode.EnabledInSelf | FindMode.InAncestors;

	/// <summary>
	/// Looks for <see cref="IHealth"/> to use <see cref="IHealth.TrySendDamage(in DamageData)"/>.
	/// </summary>
	/// <returns> If we were able to find and bother sending our damage. </returns>
	public static bool TryDamage( this GameObject obj, in DamageData data, in FindMode findMode = DefaultFindMode )
	{
		if ( !obj.IsValid() )
			return false;

		foreach ( var hp in obj.Components.GetAll<IHealth>( findMode ) )
			if ( hp.TrySendDamage( data ) )
				return true;

		return false;
	}
}
