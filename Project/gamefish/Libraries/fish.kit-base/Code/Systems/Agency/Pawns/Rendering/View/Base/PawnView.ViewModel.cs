namespace GameFish;

partial class PawnView
{
	/// <summary>
	/// The <see cref="GameFish.ViewRenderer"/> component. <br />
	/// It's the view model(basically). <br />
	/// Should be on a child of this object.
	/// </summary>
	[Property]
	[Title( "Renderer" )]
	[Feature( VIEW ), Order( VIEW_ORDER )]
	public ViewRenderer ViewRenderer
	{
		get => _vr.GetCached( this );
		set => _vr = value;
	}

	protected ViewRenderer _vr;
}
