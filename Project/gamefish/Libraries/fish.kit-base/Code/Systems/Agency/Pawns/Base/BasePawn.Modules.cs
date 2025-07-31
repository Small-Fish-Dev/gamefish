namespace GameFish;

partial class BasePawn : IModules<BasePawn>
{
	public BasePawn Component => this;
	public IModules<BasePawn> Modules => this;

	[Property, ReadOnly]
	[Feature( ENTITY ), Group( MODULES )]
	public List<Module<BasePawn>> ModuleList { get; set; }
}
