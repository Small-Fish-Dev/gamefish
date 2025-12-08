using System.Threading.Tasks;

namespace GameFish;

public abstract partial class PredictedEntity : Entity
{
	public override bool IsPredicted { get; set; } = true;

	// Proxies should never be networked.
	protected override bool? IsNetworkedOverride => IsProxyEntity ? false : base.IsNetworkedOverride;

	/// <summary>
	/// Is this a proxy instance of its source entity?
	/// </summary>
	public bool IsProxyEntity { get; set; }

	public PredictedEntity EntityProxy { get; set; }
	public PredictedEntity EntitySource { get; set; }

	protected override void OnUpdate()
	{
		base.OnUpdate();

		OnPredictionPreUpdate( Time.Delta );
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		if ( IsProxyEntity && EntitySource.IsValid() )
			EntitySource.OnProxyEntityDestroyed();
	}

	/// <summary>
	/// Process the ...
	/// </summary>
	protected virtual void OnPredictionPreUpdate( in float deltaTime )
	{
		if ( IsProxyEntity )
			OnEntityProxyPreUpdate( in deltaTime );
		else
			OnEntitySourcePreUpdate( in deltaTime );
	}

	/// <summary>
	/// Called on client-side copies of server-side entities.
	/// </summary>
	protected virtual void OnEntityProxyPreUpdate( in float deltaTime )
	{
	}

	protected virtual void OnEntitySourcePreUpdate( in float deltaTime )
	{
	}

	protected virtual void OnProxyEntityDestroyed()
	{
		// Keep the proxy entity around.
		if ( !IsProxyEntity )
			_ = CreateProxyEntity( delayed: true );
	}

	protected virtual void OnProxyEntityCreated( PredictionRoot root )
	{
	}

	protected virtual async Task<GameObject> CreateProxyEntity( bool delayed = true )
	{
		if ( IsProxy )
			EntityProxy?.DestroyGameObject();

		// Prevent infinite loops by default if destroyed on spawn.
		if ( delayed )
			await Task.FrameEnd();

		var proxy = GameObject.Clone( WorldTransform, startEnabled: GameObject.Enabled );

		if ( !proxy.Components.TryGet<PredictionRoot>( out var root, FindMode.EverythingInSelf ) )
		{
			root = Components.Create<PredictionRoot>( startEnabled: false );
			root.EntitySource = this;
		}

		root.Enabled = true;

		OnProxyEntityCreated( root );

		return proxy;
	}
}
