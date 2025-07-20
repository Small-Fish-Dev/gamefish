namespace GameFish;

partial class Server
{
	public const string GROUP_DEBUG = BaseEntity.DEBUG;

	[Property, ReadOnly]
	[Feature( Agent.FEATURE_AGENT ), Group( GROUP_DEBUG )]
	public List<BasePawn> AllPawns => BasePawn.GetAll<BasePawn>().ToList();

	[Property, ReadOnly]
	[Feature( Agent.FEATURE_AGENT ), Group( GROUP_DEBUG )]
	public List<BasePawn> ActivePawns => BasePawn.GetAllActive<BasePawn>().ToList();
}
