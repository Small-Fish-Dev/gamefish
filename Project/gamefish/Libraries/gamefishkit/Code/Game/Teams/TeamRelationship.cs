namespace GameFish;

public partial class TeamRelationship
{
	[KeyProperty] public Team Team { get; set; }
	[KeyProperty] public Relationship Relationship { get; set; }
}
