using System;

namespace GameFish;

/// <summary>
/// Persists between scenes so that it can track important stuff.
/// <br />
/// Only one can exist at a time as duplicates destroy themselves.
/// <br /> <br />
/// <b> USAGE: </b> Put this on the root of a prefab that is
/// referenced by the <see cref="Essential"/> component in your system scene.
/// <br /> <br />
/// <b> TIP: </b> You can put various other components in your
/// session prefab and they will persist too.
/// <br /> <br />
/// <b> TIP: </b> Override this component to add sync'd vars.
/// </summary>
[Icon( "assignment" )]
public partial class Session : Singleton<Session>
{
	protected const int SESSION_ORDER = DEBUG_ORDER - 555;

	/// <summary>
	/// Enables automatically starting the session immediately after being created.
	/// </summary>
	[Property]
	[Feature( SESSION ), Order( SESSION_ORDER )]
	public bool AutoStart { get; set; } = true;

	/// <summary>
	/// Lets you know if the session has been started at some point.
	/// </summary>
	[Sync( SyncFlags.FromHost )]
	public bool IsActive { get; protected set; }

	/// <summary>
	/// Starts a session if one does not already exist.
	/// </summary>
	/// <param name="prefab"> The session prefab to use. </param>
	/// <param name="s"> The session component(or null). </param>
	/// <returns> If a new(not existing) session could be started. </returns>
	public static bool TryCreate( PrefabFile prefab, out Session s )
	{
		// Don't replace the session on accident.
		if ( TryGetInstance( out s ) )
			return false;

		if ( !Networking.IsHost )
			return false;

		if ( !prefab.IsValid() )
		{
			Print.Warn( $"Missing/invalid session prefab:[{prefab}]" );
			return false;
		}

		if ( !prefab.TrySpawn( out var sObj ) || !sObj.Components.TryGet( out s ) )
		{
			Print.Warn( $"Couldn't find {typeof( Session )} on session prefab:[{prefab}]. Destroying." );
			sObj.Destroy();
			return false;
		}

		Print.InfoFrom( typeof( Session ), $"New session:[{s}]" );

		return true;
	}

	protected override void OnEnabled()
	{
		if ( !Networking.IsHost )
			return;

		// Prevent multiple session instances.
		if ( TryGetInstance( out var sesh ) && sesh != this )
		{
			this.Warn( "Tried to create a duplicate! Self-destructing." );
			DestroyGameObject();
			return;
		}

		if ( !GameObject.IsValid() )
			return;

		// Persist the object(in most cases).
		GameObject.Flags |= GameObjectFlags.DontDestroyOnLoad;

		base.OnEnabled();

		if ( AutoStart )
			Start();
	}

	/// <summary>
	/// Called to reset persistent session data.
	/// </summary>
	public virtual void Flush()
	{
		try
		{
			ISessionEvent.Post( i => i?.OnSessionFlush( this ) );
		}
		catch ( Exception e )
		{
			this.Warn( $"{nameof( Flush )} exception: {e}" );
		}
	}

	public void Start()
	{
		if ( !Networking.IsHost )
			return;

		if ( IsActive || !this.InGame() )
			return;

		this.Log( "Starting session." );

		IsActive = true;

		try
		{
			OnSessionStart();

			ISessionEvent.Post( i => i?.OnSessionStart( this ) );
		}
		catch ( Exception e )
		{
			this.Warn( $"{nameof( Start )} exception: {e}" );
		}
	}

	protected virtual void OnSessionStart()
	{
	}

	/// <summary>
	/// Clean up the session.
	/// </summary>
	public void Stop()
	{
		if ( !Networking.IsHost )
			return;

		if ( !IsActive || !this.InGame() )
			return;

		IsActive = false;

		this.Log( "Stopping session." );

		OnSessionStop();

		try
		{
			ISessionEvent.Post( i => i?.OnSessionStop( this ) );
		}
		catch ( Exception e )
		{
			this.Warn( $"{nameof( Stop )} exception: {e}" );
		}
	}

	protected virtual void OnSessionStop()
	{
	}
}
