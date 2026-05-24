using GameFish;

namespace GameFish;

[Icon( "pest_control_rodent" )]
public class TrapDoorModule : DoorModule
{
	protected const int LOGIC_ORDER = DOOR_ORDER - 100;

	/// <summary>
	/// Destroy these objects.
	/// </summary>
	[Property, WideMode]
	[Feature( DOOR ), Group( LOGIC ), Order( LOGIC_ORDER )]
	public List<GameObject> DestroyTargets { get; set; }

	/// <summary>
	/// Activate these logic-compatible components.
	/// </summary>
	[Property, WideMode]
	[Feature( DOOR ), Group( LOGIC ), Order( LOGIC_ORDER )]
	public List<IActivate> ActivateTargets { get; set; }

	public override void OnOpening()
	{
		base.OnOpening();

		// Destroy Objects
		if ( DestroyTargets is not null )
		{
			foreach ( var obj in DestroyTargets )
				if ( obj.IsValid() )
					obj.Destroy();
		}

		// Logic Activation
		if ( ActivateTargets is not null )
		{
			foreach ( var tgt in ActivateTargets )
			{
				if ( tgt is not Component c || !c.IsValid() )
					continue;

				tgt.TryActivate( this );
			}
		}
	}
}
