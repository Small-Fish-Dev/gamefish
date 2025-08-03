namespace GameFish;

partial class PawnView
{
	public bool HasFirstPersonMode => IsModeAllowed( Perspective.FirstPerson );

	[Property]
	[Feature( MODES )]
	[Group( FIRST_PERSON ), Order( FIRST_PERSON_ORDER )]
	public bool ShowViewRenderer { get; set; } = true;

	protected virtual void OnFirstPersonModeSet()
	{
	}

	protected virtual void SetFirstPersonModeTransform()
	{
		SetTransformFromRelative();
	}

	protected virtual void OnFirstPersonModeUpdate( in float deltaTime )
	{
		var pawn = TargetPawn;

		if ( !pawn.IsValid() )
			return;

		Relative = new();

		UpdateViewRenderer( in deltaTime );
	}

	protected virtual void ToggleViewRenderer( bool isEnabled )
	{
		var vm = ViewRenderer;

		if ( vm.IsValid() )
			vm.GameObject.Enabled = isEnabled;
	}

	protected virtual void UpdateViewRenderer( in float deltaTime )
	{
		if ( Mode != Perspective.FirstPerson )
			return;

		var vm = ViewRenderer;

		if ( !vm.IsValid() )
			return;

		var isFirstPerson = IsFirstPerson();

		ToggleViewRenderer( isFirstPerson );

		if ( isFirstPerson )
			vm.UpdateOffset( deltaTime );
	}

	/// <summary>
	/// Determines if we're actually in first person based on
	/// the distance from this view to the eye position.
	/// </summary>
	protected virtual bool IsFirstPerson()
	{
		if ( Mode != Perspective.FirstPerson )
			return false;

		return DistanceFromEye <= 5f;
	}
}
