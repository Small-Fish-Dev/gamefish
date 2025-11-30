namespace GameFish;

partial class BasePawn : ITeam
{
	[Property]
	[Sync( SyncFlags.FromHost )]
	[Feature( PAWN ), Group( TEAM ), Order( -69 )]
	public Team Team
	{
		get => _team;
		protected set
		{
			var oldTeam = _team;
			_team = value;
			OnSetTeam( value, oldTeam );
		}
	}

	protected Team _team;

	/// <summary>
	/// The tag assigned to this object for their team(or null).
	/// </summary>
	public string TeamTag => Team?.Tag;

	/// <summary>
	/// Assigns this pawn's team.
	/// </summary>
	public virtual void SetTeam( Team team )
		=> Team = team;

	public virtual void OnSetTeam( Team newTeam, Team oldTeam = null )
		=> Team.UpdateTags( GameObject, newTeam?.Tag );

	/// <returns> If they are neutral, a friend or an enemy. </returns>
	public virtual Relationship GetRelationship( ITeam other )
	{
		if ( Team is null )
		{
			if ( other?.Team is not null )
				return other.Team.DefaultRelationship;

			return Relationship.Neutral;
		}

		if ( other is null || other.Team is null )
			return Team.DefaultRelationship;

		return Team.GetRelationship( other.Team );
	}

	public bool IsNeutral( ITeam pawn )
		=> GetRelationship( pawn ) is Relationship.Neutral;

	public bool IsEnemy( ITeam pawn )
		=> GetRelationship( pawn ) is Relationship.Enemy;

	public bool IsAlly( ITeam pawn )
		=> GetRelationship( pawn ) is Relationship.Ally;

	/// <returns></returns>
	public virtual Relationship GetRelationship( GameObject obj, FindMode findMode = FindMode.EverythingInSelf | FindMode.InAncestors )
	{
		if ( !obj.IsValid() )
			return Team.IsValid() ? Team.DefaultRelationship : Relationship.Neutral;

		if ( obj.Components.TryGet<ITeam>( out var t, findMode ) )
			return GetRelationship( t );

		return Relationship.Neutral;
	}
}
