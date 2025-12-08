namespace GameFish;

partial class Pawn
{
	/// <returns> Every valid <typeparamref name="TPawn"/> regardless of enabled state. </returns>
	/// <remarks>
	/// This method is expensive! <br />
	/// Consider using <see cref="GetAllActive"/> instead.
	/// </remarks>
	public static IEnumerable<TPawn> GetAll<TPawn>() where TPawn : Pawn
		=> Game.ActiveScene?.Components?.GetAll<TPawn>( FindMode.EverythingInSelfAndDescendants )
			?.Where( p => p.IsValid() )
			?? [];

	/// <returns> Every valid and active <typeparamref name="TPawn"/>. </returns>
	public static IEnumerable<TPawn> GetAllActive<TPawn>() where TPawn : Pawn
		=> Game.ActiveScene?.GetAll<TPawn>() ?? [];

	/// <returns> Every valid and active pawn. </returns>
	public static IEnumerable<Pawn> GetAllActive()
		=> GetAllActive<Pawn>();

	/// <returns> Every valid and active player-owned pawn. </returns>
	public static IEnumerable<Pawn> GetAllPlayers()
		=> GetAllActive().Where( pawn => pawn.IsPlayer );

	/// <typeparam name="TPawn"></typeparam>
	/// <param name="owner"></param>
	/// <param name="isActive"> If true: gets every pawn even if disabled. </param>
	/// <returns> Every pawn owned by the <paramref name="owner"/>(or empty if null). </returns>
	public static IEnumerable<TPawn> GetAllOwnedBy<TPawn>( Agent owner, bool isActive = true ) where TPawn : Pawn
	{
		if ( !owner.IsValid() )
			return [];

		return (isActive
			? GetAllActive<TPawn>().Where( p => p.Owner == owner )
			: GetAll<TPawn>().Where( p => p.Owner == owner )) ?? [];
	}

	/// <returns> Every pawn owned by the <paramref name="owner"/>(or empty if null). </returns>
	public static IEnumerable<Pawn> GetAllOwnedBy( Agent owner, bool isActive = false )
		=> GetAllOwnedBy<Pawn>( owner, isActive );

	/// <summary>
	/// A consistent way of getting a pawn from a <see cref="GameObject"/>.
	/// </summary>
	/// <returns> If the pawn was found. </returns>
	public static bool TryGet( GameObject obj, out Pawn pawn )
	{
		if ( !obj.IsValid() )
		{
			pawn = null;
			return false;
		}

		return obj.Components.TryGet( out pawn, FindMode.EverythingInSelfAndAncestors );
	}

	/// <summary>
	/// A consistent way of getting a pawn-derived class from a <see cref="GameObject"/>.
	/// </summary>
	/// <returns> If the pawn was found. </returns>
	public static bool TryGet<T>( GameObject obj, out T pawn ) where T : Pawn
	{
		if ( !obj.IsValid() )
		{
			pawn = null;
			return false;
		}

		return obj.Components.TryGet( out pawn, FindMode.EverythingInSelfAndAncestors );
	}
}
