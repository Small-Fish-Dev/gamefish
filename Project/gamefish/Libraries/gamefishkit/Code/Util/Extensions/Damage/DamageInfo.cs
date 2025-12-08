namespace GameFish;

partial class Library
{
	public static void AddTag( this DamageInfo dmgInfo, in string tag )
		=> dmgInfo.Tags = dmgInfo.Tags.With( tag );

	public static void AddTags( this DamageInfo dmgInfo, params string[] tags )
	{
		foreach ( var tag in tags )
			dmgInfo.Tags.Add( tag );
	}

	public static void AddTags( this DamageInfo dmgInfo, ITagSet tagSet )
		=> dmgInfo.Tags.Add( tagSet );

	public static void RemoveTag( this DamageInfo dmgInfo, in string tag )
		=> dmgInfo.Tags.Remove( tag );

	public static void RemoveTags( this DamageInfo dmgInfo, params string[] tags )
	{
		foreach ( var tag in tags )
			dmgInfo.Tags.Remove( tag );
	}

	public static void ClearTags( this DamageInfo dmgInfo )
		=> dmgInfo.Tags.RemoveAll();

	public static DamageInfo WithTag( this DamageInfo dmgInfo, in string tag )
	{
		dmgInfo.AddTag( tag );
		return dmgInfo;
	}

	public static DamageInfo WithTags( this DamageInfo dmgInfo, params string[] tags )
	{
		dmgInfo.AddTags( tags );
		return dmgInfo;
	}

	public static DamageInfo WithTags( this DamageInfo dmgInfo, ITagSet tagSet )
	{
		dmgInfo.AddTags( tagSet );
		return dmgInfo;
	}

	public static DamageInfo WithoutTag( this DamageInfo dmgInfo, in string tag )
	{
		dmgInfo.RemoveTag( tag );
		return dmgInfo;
	}

	public static DamageInfo WithoutTags( this DamageInfo dmgInfo, params string[] tags )
	{
		dmgInfo.Tags = dmgInfo.Tags.Without( tags );
		return dmgInfo;
	}

	public static DamageInfo WithoutTags( this DamageInfo dmgInfo, ITagSet tagSet )
	{
		dmgInfo.Tags = dmgInfo.Tags.Without( tagSet );
		return dmgInfo;
	}
}
