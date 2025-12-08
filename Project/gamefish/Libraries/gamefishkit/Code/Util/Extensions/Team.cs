namespace GameFish;

partial class Library
{
	/// <summary>
	/// Set the team of each component on this object(which updates each object's tags).
	/// </summary>
	public static void SetTeam( this GameObject obj, in Team team, in FindMode findMode = FindMode.EverythingInSelf )
		=> Team.Set( obj, team, findMode );
}
