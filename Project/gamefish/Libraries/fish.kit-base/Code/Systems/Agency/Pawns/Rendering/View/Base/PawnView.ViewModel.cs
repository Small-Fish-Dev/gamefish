namespace GameFish;

partial class PawnView
{
	/// <summary>
	/// The <see cref="global::GameFish.ViewRenderer"/> component. <br />
	/// Needs to be on a child of this object!
	/// </summary>
	[Property]
	[Feature( VIEW )]
	public ViewRenderer ViewRenderer
	{
		// Auto-cache the component.
		get => _vr.IsValid() ? _vr
			: _vr = Components?.Get<ViewRenderer>( FindMode.EverythingInDescendants );

		set { _vr = value; }
	}

	protected ViewRenderer _vr;
}
