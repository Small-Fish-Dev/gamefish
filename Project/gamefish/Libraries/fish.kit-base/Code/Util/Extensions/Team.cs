namespace GameFish;

public static partial class GameFish
{
	/// <summary>
	/// Set the team of each component on this object(which updates each object's tags).
	/// </summary>
	public static void SetTeam( this GameObject obj, in Team team, in FindMode findMode = FindMode.EverythingInSelfAndDescendants )
		=> Team.Set( obj, team, findMode );
}
