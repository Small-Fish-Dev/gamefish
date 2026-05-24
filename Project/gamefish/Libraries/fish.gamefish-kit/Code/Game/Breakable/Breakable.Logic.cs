namespace GameFish;

partial class Breakable : IActivate
{
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
