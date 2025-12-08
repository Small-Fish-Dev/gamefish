namespace GameFish;

/// <summary>
/// Add this to things that should identify with a team.
/// </summary>
public partial interface ITeam
{
	/// <summary>
	/// What team this is aligned to(or null). <br />
	/// You should use <see cref="SetTeam"/> to set this.
	/// </summary>
	public Team Team { get; }

	/// <summary>
	/// Unimplemented by default. <br />
	/// You should make <see cref="Team"/>'s setter protected
	/// and use this to process any team changes.
	/// </summary>
	public virtual void SetTeam( Team team ) { }
}
