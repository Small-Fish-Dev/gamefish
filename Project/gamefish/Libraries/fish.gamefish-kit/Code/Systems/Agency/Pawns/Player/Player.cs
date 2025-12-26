namespace GameFish;

public partial class PlayerPawn : Player;

/// <summary>
/// A pawn that can only be owned by a player.
/// </summary>
[Icon( "mood" )]
[EditorHandle( Icon = "😎" )]
public partial class Player : Pawn
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

	protected override void UpdateInput( in float deltaTime )
	{
		base.UpdateInput( deltaTime );

		if ( Input.Pressed( "Use" ) )
			TryUse( EyeForward );
	}

	public virtual bool TryUse( in Vector3 dir )
	{
		if ( !TryFindUsable( in dir, out var usable ) )
			return false;

		return usable.TryUse( this );
	}

	public virtual bool TryFindUsable( in Vector3 dir, out IUsable usable )
	{
		usable = null;

		var tr = GetEyeTrace( distance: 150f, dir: dir ).Run();

		if ( !tr.Hit || !tr.GameObject.IsValid() )
			return false;

		usable = tr.GameObject.Components
			.GetAll<IUsable>( FindMode.EnabledInSelf | FindMode.InDescendants | FindMode.InAncestors )
			.Where( u => u.IsUsable( this ) )
			.OrderBy( u => u.UsablePriority )
			.FirstOrDefault();

		return usable is not null;
	}
}
