namespace GameFish;

partial class Library
{
	/// <remarks>
	/// Combines both valid and <see cref="GameObject.IsDestroyed"/> checking for convenience.
	/// <br /> <br />
	/// <b> BEFORE: </b> <c> if ( !GameObject.IsValid() || GameObject.IsDestroyed ) return; </c>
	/// <br />
	/// <b> AFTER: </b> <c> if ( GameObject.IsDestroyed() ) return; </c>
	/// </remarks>
	public static bool IsDestroyed( this GameObject obj )
	{
		if ( !obj.IsValid() )
			return true;

		return obj.IsDestroyed;
	}
}
