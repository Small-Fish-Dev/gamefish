namespace GameFish;

partial class BasePawn : ITeam
{
	[Property]
	[Sync( SyncFlags.FromHost )]
	[Feature( PAWN ), Group( Team.TEAM ), Order( -69 )]
	public Team Team
	{
		get => _team;
		protected set
		{
			_team = value;
			OnSetTeam( value );
		}
	}

	protected Team _team;

	/// <summary>
	/// The tag assigned to this object for their team(or null).
	/// </summary>
	public string TeamTag => Team?.Tag;

	public virtual void SetTeam( Team team )
		=> Team = team;

	public virtual void OnSetTeam( Team team )
		=> Team.UpdateTags( GameObject, team?.Tag );

	/// <returns> If they are neutral, a friend or an enemy. </returns>
	public virtual Relationship GetRelationship( ITeam t )
	{
		if ( Team is not Team myTeam )
		{
			if ( t.Team is Team defaultTeam )
				return defaultTeam.DefaultRelationship;

			return Relationship.Neutral;
		}

		if ( t.Team is not Team theirTeam )
			return myTeam.DefaultRelationship;

		return myTeam.GetRelationship( theirTeam );
	}

	public bool IsNeutral( ITeam pawn )
		=> GetRelationship( pawn ) is Relationship.Neutral;

	public bool IsEnemy( ITeam pawn )
		=> GetRelationship( pawn ) is Relationship.Enemy;

	public bool IsAlly( ITeam pawn )
		=> GetRelationship( pawn ) is Relationship.Ally;

	/// <returns></returns>
	public virtual Relationship GetRelationship( GameObject obj, FindMode findMode = FindMode.EverythingInSelf )
	{
		if ( !obj.IsValid() )
			return Relationship.Neutral;

		if ( obj.Components.TryGet<ITeam>( out var t, findMode ) )
			return GetRelationship( t );

		return Relationship.Neutral;
	}
}
