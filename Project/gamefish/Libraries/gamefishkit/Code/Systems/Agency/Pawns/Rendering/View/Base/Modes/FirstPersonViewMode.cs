namespace GameFish;

public partial class FirstPersonViewMode : ViewMode
{
	[Property]
	[Feature( VIEW )]
	public override bool AllowFirstPerson => true;

	public override void OnModeUpdate( in float deltaTime )
	{
		base.OnModeUpdate( deltaTime );

		if ( !TargetPawn.IsValid() )
			return;

		Relative = new();

		UpdateViewRenderer( in deltaTime );
	}
}
