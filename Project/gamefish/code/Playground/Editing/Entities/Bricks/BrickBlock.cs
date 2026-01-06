namespace Fishbox;

[Icon( "dataset" )] // haha
public partial class BrickBlock : EditorObject
{
	[Property]
	[Feature( EDITOR ), Group( ART )]
	public ModelRenderer Renderer { get; protected set; }

	[Property]
	[Sync( SyncFlags.FromHost )]
	[Feature( EDITOR ), Group( ART )]
	public Color BrickColor
	{
		get => _brickColor;
		set
		{
			_brickColor = value;
			OnSetBrickColor( in _brickColor );
		}
	}

	protected Color _brickColor = Color.White;

	protected virtual void OnSetBrickColor( in Color c )
		=> SetRendererColor( in c );

	protected override void OnStart()
	{
		base.OnStart();

		SetRendererColor( BrickColor );
	}

	public virtual void SetRendererColor( in Color c )
	{
		if ( Renderer.IsValid() )
			Renderer.Tint = c;
	}

	[Rpc.Host]
	public void RpcHostSetBrickColor( Color c )
	{
		// TODO: Permission check. (IMPORTANT)
		if ( !Server.TryFindClient( Rpc.Caller, out _ ) )
			return;

		BrickColor = c;
	}
}
