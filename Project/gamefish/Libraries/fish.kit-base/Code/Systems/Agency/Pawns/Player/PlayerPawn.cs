namespace GameFish;

/// <summary>
/// A pawn that can only be owned by a player.
/// </summary>
[EditorHandle( Icon = "😎" )]
public partial class PlayerPawn : BasePawn
{
	protected override void OnEnabled()
	{
		Tags?.Add( TAG_PLAYER );

		base.OnEnabled();
	}

	/// <summary>
	/// Only player <see cref="Agent"/>s can own a player pawn.
	/// </summary>
	public override bool AllowOwnership( Agent agent )
	{
		if ( !agent.IsValid() || !agent.IsPlayer )
			return false;

		return base.AllowOwnership( agent );
	}
}
