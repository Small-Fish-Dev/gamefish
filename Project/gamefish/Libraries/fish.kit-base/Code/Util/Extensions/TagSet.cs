namespace GameFish;

public static partial class GameFish
{
	public static TagSet With( this TagSet set, in string tag )
	{
		set?.Add( tag );
		return set;
	}

	public static TagSet With( this TagSet set, params string[] tags )
	{
		if ( set is not null )
			foreach ( var tag in tags )
				set.Add( tag );

		return set;
	}

	public static TagSet Without( this TagSet set, in string tag )
	{
		set?.Remove( tag );
		return set;
	}

	public static TagSet Without( this TagSet set, params string[] tags )
	{
		if ( set is not null )
			foreach ( var tag in tags )
				set.Remove( tag );

		return set;
	}

	public static TagSet Cleared( this TagSet set )
	{
		set?.RemoveAll();
		return set;
	}
}
