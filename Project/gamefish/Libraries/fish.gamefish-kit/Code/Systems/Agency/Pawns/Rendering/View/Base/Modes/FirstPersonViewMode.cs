namespace GameFish;

public partial class FirstPersonViewMode : ViewMode
{
	[Property]
	[Feature( VIEW )]
	public override bool AllowFirstPerson => true;

	public override void OnModeUpdate( in float deltaTime )
	{
		if ( !TargetPawn.IsValid() )
			return;

		Relative = new();

		base.OnModeUpdate( deltaTime );
	}
}
