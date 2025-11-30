namespace GameFish;

public partial struct CitizenOutfit
{
	public struct Element : IValid
	{
		public const string GROUP_OFFSET = "Offset";

		public readonly bool IsValid => Clothing.IsValid();

		[KeyProperty]
		public Clothing Clothing { get; set; }

		[KeyProperty, Range( 0f, 1f )]
		public float? Tint { get; set; }

		public bool HasOffset { get; set; }
		public Transform Transform { get; set; }
		public string Bone { get; set; }

		public Element() { }

		public Element( ClothingContainer.ClothingEntry entry )
		{
			if ( entry is null )
				return;

			Clothing = entry.Clothing;

			Tint = entry.Tint;

			HasOffset = entry.Transform.HasValue;
			Transform = entry.Transform.GetValueOrDefault();
			Bone = entry.Bone;
		}

		public readonly ClothingContainer.ClothingEntry GetEntry()
			=> new( Clothing )
			{
				Clothing = Clothing,
				Tint = Tint,
				Bone = Bone,
				Transform = HasOffset ? Transform : null
			};
	}

	public List<Element> Clothing { get; set; } = [];
	public bool? Human { get; set; } = true;
	public float? Height { get; set; } = 0.5f;

	public CitizenOutfit() { }

	public void Add( ClothingContainer.ClothingEntry entry )
	{
		Clothing ??= [];
		Clothing.Add( new( entry ) );
	}
}
