namespace GameFish;

public struct TagFilter
{
	[KeyProperty] public bool Enabled { get; set; }
	[KeyProperty] public TagSet Tags { get; set; }

	public readonly bool Has( string tag ) => Enabled && (Tags?.Has( tag ) ?? false);
	public readonly bool HasAny( ITagSet tags ) => Enabled && (Tags?.HasAny( tags ) ?? false);
	public readonly bool HasAll( ITagSet tags ) => Enabled && (Tags?.HasAll( tags ) ?? false);
}
