namespace GameFish;

/// <summary>
/// A trigger that specifically looks for players.
/// </summary>
public abstract class PlayerTrigger : Trigger
{
	public override Color DefaultGizmoColor => Color.Parse( "#df58fa" ) ?? Color.Magenta;

	/// <summary>
	/// The players that are actively inside of this.
	/// </summary>
	public List<Player> PlayersInside { get; set; }

	public virtual bool IsValidPlayer( Player pl )
		=> pl.IsValid() && pl.IsAlive;

	protected override void OnTouchStart( GameObject obj )
	{
		base.OnTouchStart( obj );

		if ( !Pawn.TryGet<Player>( obj, out var player ) )
			return;

		if ( !IsValidPlayer( player ) )
			return;

		PlayersInside ??= [];

		if ( !PlayersInside.Contains( player ) )
			PlayersInside.Add( player );

		OnPlayerEnter( player );
	}

	protected override void OnTouchStop( GameObject obj )
	{
		base.OnTouchStop( obj );

		if ( !Pawn.TryGet<Player>( obj, out var player ) )
			return;

		PlayersInside?.RemoveAll( pl => !IsValidPlayer( pl ) || pl == player );

		OnPlayerExit( player );
	}

	protected virtual void OnPlayerEnter( Player pl ) { }
	protected virtual void OnPlayerExit( Player pl ) { }
}
