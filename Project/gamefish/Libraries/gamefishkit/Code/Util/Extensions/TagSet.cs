namespace GameFish;

partial class Library
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

	public static TagSet With( this TagSet set, ITagSet tagSet )
	{
		if ( set is not null )
			foreach ( var tag in tagSet )
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

	public static TagSet Without( this TagSet set, ITagSet tagSet )
	{
		if ( set is not null )
			foreach ( var tag in tagSet )
				set.Remove( tag );

		return set;
	}

	public static TagSet Cleared( this TagSet set )
	{
		set?.RemoveAll();
		return set;
	}
}
