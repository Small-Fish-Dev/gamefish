namespace GameFish;

/// <summary>
/// A list of <typeparamref name="TResource"/> and their chance of spawning(higher is more likely).
/// </summary>
public abstract class WeightedResourceList<TResource> : WeightedList<TResource> where TResource : Resource
{
	[KeyProperty, WideMode]
	[Range( WEIGHT_MIN, WEIGHT_MAX, clamped: true ), Step( WEIGHT_STEP )]
	public abstract Dictionary<string, float> Resources { get; set; }

	public bool TryGetResource( string filePath, out TResource res )
	{
		if ( !filePath.IsBlank() && ResourceLibrary.TryGet( filePath, out res ) )
			return true;

		res = null;
		return false;
	}

	protected override Dictionary<TResource, float> GetEntries()
	{
		Dictionary<TResource, float> dict = [];

		foreach ( var (str, weight) in Resources ?? [] )
			if ( TryGetResource( str, out var res ) )
				dict[res] = weight;

		return dict;
	}
}
