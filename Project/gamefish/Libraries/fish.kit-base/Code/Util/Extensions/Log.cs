using Sandbox.Internal;

namespace GameFish;

public static partial class GameFish
{
	public static void Log( object source, object log )
	{
		GlobalGameNamespace.Log.Info( "[" + source + "] " + log );
	}

	public static void Warn( object source, object log )
	{
		GlobalGameNamespace.Log.Warning( "[" + source + "] " + log );
	}

	public static void Log( this Component c, object log )
	{
		if ( c is null )
			Log( c?.GetType(), log );
		else
			Log( c.ToString(), log );
	}

	public static void Log( this Component c, params object[] log )
		=> Log( c, string.Concat( log ?? [] ) );

	public static void Warn( this Component c, object log )
	{
		if ( c is null )
			Warn( c?.GetType(), log );
		else
			Warn( c.ToString(), log );
	}

	public static void Warn( this Component c, params object[] log )
		=> Warn( c, string.Concat( log ?? [] ) );
}
