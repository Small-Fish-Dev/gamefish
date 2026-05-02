namespace GameFish;

public partial struct CitizenOutfit
{
	[WideMode]
	public List<Element> Clothing { get; set; }

	[DefaultValue( true )]
	public bool? Human { get; set; }

	[Range( 0f, 1f )]
	[DefaultValue( 0.5f )]
	public float? Height { get; set; }

	[Range( 0f, 1f )]
	[DefaultValue( 0.5f )]
	public float? Age { get; set; }

	public CitizenOutfit() { }

	public void Add( ClothingContainer.ClothingEntry entry )
	{
		Clothing ??= [];
		Clothing.Add( new( entry ) );
	}
}
