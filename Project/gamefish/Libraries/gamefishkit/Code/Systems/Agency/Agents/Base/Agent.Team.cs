namespace GameFish;

partial class Agent : ITeam
{
	

	[Property]
	[Sync( SyncFlags.FromHost )]
	[Feature( AGENT ), Group( TEAM ), Order( AGENT_ORDER )]
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

	public virtual bool TrySetTeam( Team team )
	{
		if ( !Networking.IsHost )
			return false;

		// Must be explicitly null if clearing team.
		if ( !team.IsValid() && team is not null )
			return false;

		Team = team;
		return true;
	}

	public virtual void OnSetTeam( Team newTeam, Team oldTeam = null )
		=> Team.UpdateTags( GameObject, newTeam?.Tag );
}
