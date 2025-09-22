namespace GameFish;

/// <summary>
/// Add this to things that should identify with a team.
/// </summary>
public partial interface ITeam
{
	/// <summary>
	/// What team this is aligned to(if any). <br />
	/// You should use <see cref="SetTeam"/> to set this.
	/// </summary>
	public Team Team { get; }

	public void SetTeam( Team team );
}
