namespace GameFish;

partial class BasePawn : IModules<BasePawn>
{
	public BasePawn Component => this;
	public IModules<BasePawn> Modules => this;
	public List<Module<BasePawn>> ModuleList { get; set; }
}
