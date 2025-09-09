namespace GameFish;

partial class BasePawn : ITeam
{
	[Property]
	[Feature( PAWN ), Group( ITeam.TEAM ), Order( -69 )]
	public string Team
	{
		get => _team;
		set
		{
			_team = value;
			OnSetTeam( in value );
		}
	}

	protected string _team;

	public virtual void OnSetTeam( in string team )
	{
	}
}
