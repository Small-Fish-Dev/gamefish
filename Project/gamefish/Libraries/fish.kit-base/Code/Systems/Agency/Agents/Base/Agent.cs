using System.Text.Json.Serialization;

namespace GameFish;

/// <summary>
/// Something capable of control over other objects. <br />
/// It may be a player(real/fake) or an NPC.
/// </summary>
[Icon( "psychology" )]
[EditorHandle( Icon = "psychology" )]
public abstract partial class Agent : ModuleEntity, ISimulate
{
	// mister anderson
	protected const int AGENT_ORDER = DEFAULT_ORDER - 1999;

	/// <summary>
	/// Is this owned by a player?
	/// </summary>
	[Title( "Is Player" )]
	[Property, ReadOnly, JsonIgnore]
	[Feature( AGENT ), Order( AGENT_ORDER )]
	protected virtual bool InspectorIsPlayer => IsPlayer;

	/// <summary>
	/// Is this meant to be owned by a player?
	/// </summary>
	public virtual bool IsPlayer { get; }

	[Title( "Identity" )]
	[Feature( AGENT ), Group( ID )]
	[Property, ReadOnly, JsonIgnore]
	protected Identity InspectorIdentity => Identity;

	public abstract Identity Identity { get; protected set; }

	public virtual Connection Connection => Connection.Host;

	/// <summary>
	/// If NPC/Bot: always true. ('cause they in the matrix or some shit) <br />
	/// If <see cref="Client"/>: if the connection exists and is active.
	/// </summary>
	[Title( "Connected" )]
	[ReadOnly, JsonIgnore]
	[Property, Feature( AGENT )]
	protected bool InspectorConnected => Connected;

	/// <summary>
	/// If NPC/Bot: always true. ('cause they in the matrix or some shit) <br />
	/// If <see cref="Client"/>: if the connection exists and is active.
	/// </summary>
	public virtual bool Connected => true;

	/// <summary>
	/// If NPC/Bot: always false. <br />
	/// If <see cref="Client"/>: if our <see cref="Identity"/> has the specified connection.
	/// </summary>
	public virtual bool CompareConnection( Connection cn )
		=> false;

	public override string ToString()
	{
		var str = $"{GetType().ToSimpleString( includeNamespace: false )}";

		if ( !DisplayName.IsBlank() )
			str = $"{str}:\"{DisplayName}\"";

		return str;
	}

	/// <summary>
	/// A nice display name.
	/// </summary>
	public virtual string DisplayName => null;

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( CanSimulate() )
			FrameSimulate( Time.Delta );
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		if ( CanSimulate() )
			FixedSimulate( Time.Delta );
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		if ( Networking.IsHost )
			TryDropPawn( Pawn );
	}

	public virtual bool CanSimulate()
		=> InGame && this.IsOwner();

	public virtual void FrameSimulate( in float deltaTime )
	{
		if ( Pawn.IsValid() )
			Pawn.FrameSimulate( in deltaTime );
	}

	public virtual void FixedSimulate( in float deltaTime )
	{
		if ( Pawn.IsValid() )
			Pawn.FixedSimulate( in deltaTime );
	}

	/// <returns> A random default spawn point's transform(if any). </returns>
	public virtual Transform? FindSpawnPoint()
	{
		if ( GameManager.TryGetInstance( out var gm ) )
			return gm.FindSpawnPoint( this );

		var allSpawnPoints = Scene?.GetAll<SpawnPoint>();

		if ( allSpawnPoints is null || !allSpawnPoints.Any() )
			return null;

		return allSpawnPoints.PickRandom()?.WorldTransform;
	}
}
