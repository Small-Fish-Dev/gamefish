namespace GameFish;

partial class Settings
{
	[Setting( "gamefish_test_float", "Test Float", 5 )]
	public static float TestFloat { get; set; } = 1;

	[Header( "Settings" )]
	[Property, Title( "Test Float" )]
	[Feature( SAVING ), Group( DEBUG ), Order( SET_ORDER + 1 )]
	public virtual float InspectorTestFloat => TestFloat;

	/// <summary>
	/// Uses the settings system to assign a random value <see cref="TestFloat"/>.
	/// <br /> (NOT IMPLEMENTED)
	/// </summary>
	[Button( "Randomize Float" )]
	[Feature( SAVING ), Group( DEBUG ), Order( SET_ORDER + 1 )]
	protected virtual void RandomizeFloatButton()
	{
		// ...
	}
}
