using System.Text.Json.Serialization;

namespace GameFish;

/// <summary>
/// Lets you choose from a dictionary with the likelihood being greater the higher the number is.
/// <br />
/// <b> NOTE: </b> Inherit this class for inspector support.
/// </summary>
[Group( Library.NAME )]
public class WeightedList<TObject>
{
	protected const float WEIGHT_MIN = 0.001f;
	protected const float WEIGHT_MAX = 1000f;
	protected const float WEIGHT_STEP = 5f;

	[Hide, JsonIgnore]
	[Range( WEIGHT_MIN, WEIGHT_MAX, clamped: true ), Step( WEIGHT_STEP )]
	public virtual Dictionary<TObject, float> Entries { get; protected set; }

	[Hide]
	[Group( WEIGHT )]
	[DefaultValue( null )]
	[ReadOnly, JsonIgnore]
	[Title( "Weight (cached)" )]
	protected double? TotalWeight { get; set; }

	[Hide, JsonIgnore]
	public double Weight => GetTotalWeightCached();

	public WeightedList() { }

	public WeightedList( Dictionary<TObject, float> weights )
	{
		Entries = new( weights ?? [] );
	}

	public WeightedList( in WeightedList<TObject> wl )
	{
		Entries = new( wl.Entries ?? [] );
	}

	/// <summary>
	/// Allows you to convert your own weight dictionary.
	/// This is automatically cached if the returned value is not null.
	/// </summary>
	/// <returns> The weight dictionary(or null). </returns>
	protected virtual Dictionary<TObject, float> GetEntries()
		=> Entries;

	/// <summary>
	/// Forces the weight to be (re)calculated.
	/// </summary>
	[Hide]
	[Group( WEIGHT )]
	[Button( "Refresh" ), WideMode]
	public void Refresh()
	{
		Entries = GetEntries();
		CalculateWeight();
	}

	protected double GetTotalWeightCached() =>
		TotalWeight ?? CalculateWeight();

	/// <returns> The resulting weight(or <c>0</c>). </returns>
	protected double CalculateWeight()
	{
		Entries ??= GetEntries();

		if ( Entries is null )
			return 0d;

		double weight = 0d;

		foreach ( float wgt in Entries.Values )
			weight += wgt.Positive();

		TotalWeight = weight;

		return weight;
	}

	/// <returns> A result weighted by randomness(or <paramref name="default"/>). </returns>
	public TObject Pick( TObject @default = default )
		=> TryPick( out TObject result ) ? result : @default;

	/// <summary>
	/// Tries to get a result weighted by randomness.
	/// </summary>
	/// <returns> If a valid result was found. </returns>
	public bool TryPick( out TObject result, TObject @default = default )
	{
		Entries ??= GetEntries();

		if ( Random.TryGetWeighted( Entries, GetTotalWeightCached(), out var obj ) )
			return (result = obj) is not null && (result is not IValid v || v.IsValid());

		result = @default;
		return false;
	}

	/// <summary>
	/// Tries to get a result, removing it from its list if successful.
	/// </summary>
	/// <returns> If a valid result was found and removed. </returns>
	public bool TryTake( out TObject result )
	{
		if ( !TryPick( out result ) )
			return false;

		return Entries?.Remove( result ) ?? false;
	}
}
