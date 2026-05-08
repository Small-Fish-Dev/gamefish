namespace Fishbox;

public partial class FishboxPlayer : Player
{
	public IEnumerable<FishboxPlayerModule> FishboxModules => GetModules<FishboxPlayerModule>();

	protected override void UpdateInput( in float deltaTime )
	{
		base.UpdateInput( deltaTime );

		foreach ( var mod in FishboxModules )
			mod.UpdateInput( in deltaTime );
	}

	public override void Move( in float deltaTime, in bool isFixedUpdate )
	{
		var modules = FishboxModules;

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
