namespace Fishbox;

public partial class FishboxPlayer : Player
{
	public IEnumerable<PlayerModule> PlayerModules => GetModules<PlayerModule>();

	protected override void UpdateInput( in float deltaTime )
	{
		base.UpdateInput( deltaTime );

		foreach ( var mod in PlayerModules )
			mod.UpdateInput( in deltaTime );
	}

	protected override void Move( in float deltaTime, in bool isFixedUpdate )
	{
		var modules = PlayerModules;

		// Pre-Move Event
		foreach ( var mod in modules )
			mod.PreMove( deltaTime, isFixedUpdate );

		// Perform Movement
		base.Move( deltaTime, isFixedUpdate );

		// Post-Move Event
		foreach ( var mod in modules )
			mod.PostMove( deltaTime, isFixedUpdate );
	}
}
