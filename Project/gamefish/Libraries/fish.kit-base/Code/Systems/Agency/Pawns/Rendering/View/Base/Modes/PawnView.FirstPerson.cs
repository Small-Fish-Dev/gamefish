namespace GameFish;

partial class PawnView
{
	public bool HasFirstPersonMode => IsModeEnabled( Perspective.FirstPerson );

	[Property]
	[Feature( MODES )]
	[Group( FIRST_PERSON ), Order( FIRST_PERSON_ORDER )]
	public bool ShowViewModel { get; set; } = true;

	/// <summary>
	/// Used to manage pawn model fade and view model visibility.
	/// </summary>
	[Property]
	[Feature( MODES )]
	[Title( "Fade Range" )]
	[Group( FIRST_PERSON ), Order( FIRST_PERSON_ORDER )]
	public FloatRange FirstPersonRange { get; set; } = new( 15f, 30f );

	protected virtual void OnFirstPersonModeSet()
	{
	}

	protected virtual void SetFirstPersonModeTransform()
	{
		SetRelativeTransform();
	}

	protected virtual void OnFirstPersonModeUpdate( in float deltaTime )
	{
		var pawn = Pawn;

		if ( !pawn.IsValid() )
			return;

		Relative = new();

		UpdateViewModel( in deltaTime );
	}

	protected virtual void ToggleViewModel( bool isEnabled )
	{
		var vm = ViewModel;

		if ( vm.IsValid() )
			vm.GameObject.Enabled = isEnabled;
	}

	protected virtual void UpdateViewModel( in float deltaTime )
	{
		if ( Mode != Perspective.FirstPerson )
			return;

		var vm = ViewModel;

		if ( !vm.IsValid() )
			return;

		var isFirstPerson = IsFirstPerson();

		ToggleViewModel( isFirstPerson );

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

		return DistanceFromEye <= FirstPersonRange.Min;
	}
}
