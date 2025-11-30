using System.Text.Json.Serialization;

namespace GameFish;

/// <summary>
/// Used to identify and store information about a team.
/// </summary>
[AssetType( Name = "Team Definition", Extension = "team", Category = Library.NAME )]
public partial class Team : GameResource, ITeam
{
	protected override Bitmap CreateAssetTypeIcon( int width, int height )
		=> CreateSimpleAssetTypeIcon( "flag", width, height, Color.White, Color.Black );

	Team ITeam.Team => this;

	public const string RELATIONSHIPS = "Relationships";

	/// <summary> The prefix to use for finding and assigning tags to objects. </summary>
	public const string PREFIX = "team_";

	[JsonIgnore, ReadOnly]
	public string Tag => PREFIX + ResourceName;

	public string Name { get; set; } = "???";

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
		if ( team is null )
			return DefaultRelationship;

		if ( team == this )
			return SelfRelationship;

		if ( Relationships is null )
			return DefaultRelationship;

		return Relationships
			.Where( r => r.Team == team )
			.Select( r => r.Relationship )
			.FirstOrDefault( DefaultRelationship );
	}

	public bool IsAlly( Team team )
		=> GetRelationship( team ) is Relationship.Ally;

	public bool IsEnemy( Team team )
		=> GetRelationship( team ) is Relationship.Enemy;

	public bool IsAlly( GameObject obj )
		=> obj.IsValid() && TryGetTeam( obj, out var team ) && IsAlly( team );

	public bool IsEnemy( GameObject obj )
		=> obj.IsValid() && TryGetTeam( obj, out var team ) && IsEnemy( team );

	public static bool TryGetTeam( GameObject obj, out Team team )
	{
		if ( obj.IsValid() && obj.Components.TryGet<ITeam>( out var iTeam, FindMode.EnabledInSelf | FindMode.InAncestors ) )
			return (team = iTeam.Team).IsValid();

		team = null;
		return false;
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