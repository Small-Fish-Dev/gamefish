using System.Text.Json.Serialization;

namespace GameFish;

/// <summary>
/// A configuration for playing sounds.
/// </summary>
public sealed class SoundTable : IValid
{
	[Hide, JsonIgnore]
	public bool IsValid => Selection is SelectionType.Weighted
		? Weighted?.Count( w => w.IsValid() ) > 0
		: Single.IsValid();

	[Hide]
	public enum SelectionType
	{
		Single,
		Weighted
	}

	public SelectionType Selection { get; set; } = SelectionType.Single;

	[KeyProperty]
	[Title( "Sound" )]
	[WideMode( HasLabel = false )]
	[InlineEditor( Label = false )]
	[ShowIf( nameof( Selection ), SelectionType.Single )]
	public SoundEntry Single { get; set; } = new();

	[KeyProperty]
	[Title( "Sounds" )]
	[WideMode( HasLabel = false )]
	[InlineEditor( Label = false )]
	[ShowIf( nameof( Selection ), SelectionType.Weighted )]
	public List<WeightedSoundEntry> Weighted { get; set; } = [new()];

	public SoundEntry Pick()
	{
		if ( Selection is SelectionType.Single )
			return Single;

		if ( Selection is SelectionType.Weighted )
		{
			var dict = new Dictionary<WeightedSoundEntry, float>();

			foreach ( var e in Weighted ?? [] )
				if ( e.IsValid() )
					dict[e] = e.Weight;

			if ( Random.TryGetWeighted( dict, out var entry ) )
				return entry;
		}

		return null;
	}

	public bool TryPick( out SoundEntry entry )
	{
		entry = Pick();
		return entry.IsValid();
	}
}
