namespace GameFish;

partial class DamageTrigger : IActivate
{
	public virtual bool CanActivate( object source = null )
	{
		if ( GameObject.IsDestroyed() )
			return false;

		// if ( IsProxy )
		// 	return false;

		return IsOn;
	}

	public bool TryActivate( object source = null, object value = null )
	{
		if ( !CanActivate( source: source ) )
			return false;

		if ( Touching is null )
			return false;

		var hadEffect = false;

		foreach ( var obj in Touching )
			hadEffect |= TryDamage( obj );

		return hadEffect;
	}
}
