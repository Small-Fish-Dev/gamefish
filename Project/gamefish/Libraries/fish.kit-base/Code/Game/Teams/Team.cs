namespace GameFish;

/// <summary>
/// Used to identify and store information about a team.
/// </summary>
[AssetType( Name = "Team Definition", Extension = "team", Category = "Game Fish" )]
public partial class Team : GameResource
{
	protected override Bitmap CreateAssetTypeIcon( int width, int height )
		=> CreateSimpleAssetTypeIcon( "flag", width, height, Color.White, Color.Black );

	/// <summary>
	/// The prefix to use for finding and assigning tags to objects.
	/// </summary>
	public const string PREFIX = "team_";

	public const string TEAM = "🚩 Team";

	public const string RELATIONSHIPS = "Relationships";

	public string Name { get; set; } = "???";
	public string Tag => PREFIX + ResourceName;

	/// <summary>
	/// If true: you can pick this team if the mode allows it.
	/// </summary>
	public bool IsSelectable { get; set; } = false;

	/// <summary>
	/// The relationship members of this team have with each other.
	/// </summary>
	[Title( "Self" )]
	[Group( RELATIONSHIPS )]
	public Relationship SelfRelationship { get; set; } = Relationship.Ally;

	/// <summary>
	/// What relationship to consider with other teams if not otherwise specified.
	/// </summary>
	[Title( "Default" )]
	[Group( RELATIONSHIPS )]
	public Relationship DefaultRelationship { get; set; } = Relationship.Enemy;

	[Group( RELATIONSHIPS )]
	public List<TeamRelationship> Relationships
	{
		get => _relationships;
		set => _relationships = value;
	}

	[Hide] private List<TeamRelationship> _relationships;

	public Relationship GetRelationship( Team team )
	{
		if ( team == this )
			return SelfRelationship;

		return Relationships
			.Where( r => r.Team == team )
			.Select( r => r.Relationship )
			.FirstOrDefault( DefaultRelationship );
	}

	/// <summary>
	/// Sets the team of every component on objects that implement <see cref="ITeam"/> according to the find mode.
	/// </summary>
	public static void Set( GameObject obj, in Team team, in FindMode findMode = FindMode.EverythingInSelf )
	{
		if ( !obj.IsValid() )
			return;

		foreach ( var i in obj.Components.GetAll<ITeam>( findMode ) )
			i?.SetTeam( team );
	}

	public static void UpdateTags( GameObject obj, in string teamTag )
	{
		if ( !obj.IsValid() )
			return;

		if ( obj.Tags is var tags )
		{
			foreach ( var tag in tags.Where( tag => tag.StartsWith( PREFIX ) ) )
				tags.Remove( tag );

			if ( !string.IsNullOrWhiteSpace( teamTag ) )
				tags.Add( teamTag );
		}
	}

	public static void UpdateTags( Component c, Team team )
		=> UpdateTags( c?.GameObject, team?.Tag );
}