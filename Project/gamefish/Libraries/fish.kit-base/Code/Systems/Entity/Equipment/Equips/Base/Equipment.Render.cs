namespace GameFish;

partial class Equipment : ISkinned
{
	protected const string MODELS = "Models";

	[Property]
	[Feature( EQUIP ), Group( MODELS )]
	public Model ViewModel { get; set; }

	[Property]
	[Feature( EQUIP ), Group( MODELS )]
	public Model WorldModel { get => WorldRenderer?.Model; set { if ( WorldRenderer.IsValid() ) WorldRenderer.Model = value; } }

	[Property]
	[Feature( EQUIP ), Group( MODELS )]
	public virtual SkinnedModelRenderer WorldRenderer
	{
		// Auto-cache the component.
		get => _wr.IsValid() ? _wr
			: _wr = Components?.Get<SkinnedModelRenderer>( FindMode.EverythingInDescendants );

		set { _wr = value; }
	}

	protected SkinnedModelRenderer _wr;

	public SkinnedModelRenderer SkinRenderer { get => WorldRenderer; set => WorldRenderer = value; }
}
