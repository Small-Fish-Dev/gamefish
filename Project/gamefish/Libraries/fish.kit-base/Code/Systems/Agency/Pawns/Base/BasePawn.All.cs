namespace GameFish;

partial class BasePawn
{
	/// <returns> Every valid <typeparamref name="TPawn"/> regardless of enabled state. (expensive!) </returns>
	public static IEnumerable<TPawn> GetAll<TPawn>() where TPawn : BasePawn
		=> Game.ActiveScene?.Components?.GetAll<TPawn>( FindMode.EverythingInSelfAndDescendants )
			?.Where( p => p.IsValid() )
			?? [];

	/// <returns> Every valid and active <typeparamref name="TPawn"/>. (might be expensive) </returns>
	public static IEnumerable<TPawn> GetAllActive<TPawn>() where TPawn : BasePawn
		=> Game.ActiveScene?.GetAll<TPawn>()
			?.Where( p => p.IsValid() )
			?? [];

	/// <returns> Every pawn owned by the <typeparamref name="TPawn"/>(or empty if null). </returns>
	public static IEnumerable<TPawn> GetAllOwnedBy<TPawn>( Agent owner, bool isActive = false ) where TPawn : BasePawn
	{
		if ( !owner.IsValid() )
			return [];

		return isActive
			? GetAllActive<TPawn>().Where( p => p.Agent == owner )
			: GetAll<TPawn>().Where( p => p.Agent == owner );
	}
}
