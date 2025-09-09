using System.Runtime.CompilerServices;

namespace GameFish;

/// <summary>
/// Add this to things that should identify with a team.
/// </summary>
public partial interface ITeam
{
	public const string TEAM = "🚩 Team";
	public const string TAG_PREFIX = "team_";

	/// <summary>
	/// What team this is aligned to(if any).
	/// </summary>
	public string Team { get; protected set; }

	public void SetTeam( in string team )
	{
		Team = team;

		// Update the objects's tags to only have this team.
		if ( this is Component c && c.IsValid() )
		{
			if ( c.Tags is var tags )
			{
				foreach ( var teamTag in tags.Where( tag => tag.StartsWith( TAG_PREFIX ) ) )
					tags.Remove( teamTag );

				tags.Add( TAG_PREFIX + team );
			}
		}

		OnSetTeam( in team );
	}

	public virtual void OnSetTeam( in string Team )
	{
	}
}

public partial class GameFish
{
	/// <summary>
	/// Set the team of each component on this object(which updates each object's tags).
	/// </summary>
	public static void SetTeam( this GameObject obj, in string team, in FindMode findMode = FindMode.EverythingInSelfAndDescendants )
	{
		if ( !obj.IsValid() )
			return;

		foreach ( var i in obj.Components.GetAll<ITeam>( findMode ) )
			i?.SetTeam( team );
	}

	/// <summary>
	/// Set the team of each component on this object(which updates each object's tags).
	/// </summary>
	public static void SetTeam<T>( this T c, in string team, in FindMode findMode = FindMode.EverythingInSelfAndDescendants ) where T : Component
	{
		if ( !c.IsValid() )
			return;

		c.GameObject?.SetTeam( team, in findMode );
	}
}
