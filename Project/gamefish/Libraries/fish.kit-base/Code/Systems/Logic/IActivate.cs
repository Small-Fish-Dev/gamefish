using System;

namespace GameFish;

/// <summary>
/// Allows firing a basic activation signal that can pass in a source and/or value.
/// </summary>
[Icon( "touch_app" )]
public interface IActivate
{
	/// <summary>
	/// Allows filtering what can activate this and when.
	/// Might be ran very often(such as in UI) so optimization is warranted.
	/// </summary>
	/// <param name="source"> Could be a player, a logic entity, or <c>null</c>. </param>
	/// <returns> If this could be activated. </returns>
	public virtual bool CanActivate( object source = null )
		=> true;

	/// <summary>
	/// Attempts activation with optional context.
	/// </summary>
	/// <returns> If activation was successful. </returns>
	public virtual bool TryActivate( object source = null, object value = null )
	{
		try
		{
			if ( !CanActivate( source ) )
				return false;

			Activate( source, value );
			return true;
		}
		catch ( Exception e )
		{
			Print.WarnFrom( this, $"{nameof( TryActivate )} exception: {e}" );
			return false;
		}
	}

	public void Activate( object source = null, object value = null );
}
