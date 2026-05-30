namespace GameFish;

partial class Breakable : IActivate
{
	/// <summary>
	/// Executed when this breaks.
	/// </summary>
	[Property]
	[Title( "On Break" )]
	[InlineEditor, WideMode]
	[Feature( LOGIC ), Order( LOGIC_ORDER )]
	protected List<LogicAction> OnBreakLogic { get; set; }

	public bool TryActivate( object source = null, object value = null )
	{
		if ( !IsAlive )
			return false;

		RpcActivate();
		return true;
	}

	[Rpc.Owner]
	protected void RpcActivate()
		=> OnActivated();

	protected virtual void OnActivated()
		=> TryKill();
}
