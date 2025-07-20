namespace GameFish;

public static class DamageExtensions
{
    public const FindMode DefaultFindMode = FindMode.EnabledInSelf | FindMode.InAncestors;

    public static bool TryDamage( this GameObject obj, in DamageInfo dmgInfo, in FindMode findMode = DefaultFindMode )
    {
        if ( !obj.IsValid() )
            return false;

        var hasDamaged = false;

        foreach ( var hp in obj.Components.GetAll<IHealth>( findMode ) )
            if ( hp.TryDamage( dmgInfo ) )
                hasDamaged = true;

        return hasDamaged;
    }
}
