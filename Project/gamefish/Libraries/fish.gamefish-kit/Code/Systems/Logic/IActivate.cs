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
	public virtual bool CanActivate( object source )
	{
		// Auto-check validity.
		if ( this is IValid v )
		{
			if ( !v.IsValid() )
				return false;

			// Prevent activating on destroyed objects.
			if ( v is Component c )
			{
				var obj = c?.GameObject;
				return !obj.IsDestroyed();
			}
		}

		// Might be some kind of struct.
		return this is not null;
	}

	/// <summary>
	/// Attempts activation with optional context.
	/// </summary>
	/// <returns> If activation was successful. </returns>
	public bool TryActivate( object source = null, object value = null );
}
