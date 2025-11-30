namespace GameFish;

partial class PawnView
{
	/// <summary>
	/// The <see cref="GameFish.ViewRenderer"/> component.
	/// It's like a more abstract view model, basically.
	/// <br />
	/// Should be on a child of this object.
	/// </summary>
	[Property]
	[Feature( VIEW )]
	[Title( "Renderer" )]
	public ViewRenderer ViewRenderer
	{
		// Auto-cache the component.
		get => _vr.IsValid() ? _vr
			: _vr = Components?.Get<ViewRenderer>( FindMode.EverythingInDescendants );

		set { _vr = value; }
	}

	protected ViewRenderer _vr;
}
