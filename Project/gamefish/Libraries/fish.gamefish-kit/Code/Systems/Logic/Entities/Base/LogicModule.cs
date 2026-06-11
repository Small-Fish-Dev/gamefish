namespace GameFish;

/// <summary>
/// An entity meant for logical processing.
/// </summary>
[Icon( "code" )]
public abstract partial class LogicModule : Module
{
	protected const int LOGIC_ORDER = DEFAULT_ORDER - 1000;

	protected const int LOGIC_DEBUG_ORDER = LOGIC_ORDER - 50;
	protected const int LOGIC_FUNCTIONS_ORDER = LOGIC_ORDER + 200;

	public override bool IsParent( ModuleEntity comp )
		=> comp is LogicEntity;

	public LogicEntity Logic => Parent as LogicEntity;
}
