namespace GameFish;

partial class PawnView
{
	/// <summary>
	/// The <see cref="global::GameFish.ViewModel"/> component. <br />
	/// Needs to be on a child of this object!
	/// </summary>
	[Property]
	[Feature( VIEW )]
	public ViewModel ViewModel
	{
		// Auto-cache the component.
		get => _vm.IsValid() ? _vm
			: _vm = Components?.Get<ViewModel>( FindMode.EverythingInDescendants );

		set { _vm = value; }
	}

	protected ViewModel _vm;

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
