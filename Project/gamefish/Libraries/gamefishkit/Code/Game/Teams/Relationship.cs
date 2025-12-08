namespace GameFish;

public enum Relationship
{
	/// <summary>
	/// They won't pick fights.
	/// </summary>
	[Icon( "😐" )]
	Neutral,

	/// <summary>
	/// They might defend each other.
	/// </summary>
	[Icon( "💖" )]
	Ally,

	/// <summary>
	/// They may attack on sight.
	/// </summary>
	[Icon( "⚔" )]
	Enemy
}
