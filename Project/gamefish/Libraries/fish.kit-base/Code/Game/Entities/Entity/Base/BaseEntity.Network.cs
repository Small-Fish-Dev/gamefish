using System;

namespace GameFish;

partial class BaseEntity
{
	protected const string NETWORKING = "📶 Networking";
	protected const int NETWORK_ORDER = -999999;

	protected const string NETWORK_INFO =
		"These are this component's intended networking settings.\n\n" +
		"If networking is enabled then you should keep this component on an object(such as a child) separate from other networked components or their settings may conflict!";

	/// <summary>
	/// If enabled: network this object <c>OnStart</c> using these settings.
	/// </summary>
	[Property]
	[Title( "Automatic" )]
	[Feature( ENTITY ), Order( NETWORK_ORDER )]
	[ToggleGroup( nameof( NetworkAutomatically ), Label = NETWORKING )]
	public bool NetworkAutomatically
	{
		get => _netAuto || IsNetworkingForced;
		set => _netAuto = value;
	}

	protected bool _netAuto = false;

	/// <summary>
	/// If true: the component is forcing this to always be networked.
	/// </summary>
	[Property]
	[Title( $"Forced (by Component)" )]
	[Feature( ENTITY ), Order( NETWORK_ORDER )]
	[ShowIf( nameof( HasForcedNetworking ), true )]
	[ToggleGroup( nameof( NetworkAutomatically ), Label = NETWORKING )]
	protected bool HasForcedNetworking => IsNetworkingForced;

	/// <returns> If <see cref="NetworkAutomatically"/> should always be true. </returns>
	protected virtual bool IsNetworkingForced => false;

	[Property]
	[Title( "Id" )]
	[Feature( ENTITY ), Order( NETWORK_ORDER )]
	[InfoBox( NETWORK_INFO, Tint = EditorTint.Blue, Icon = "wifi" )]
	[ToggleGroup( nameof( NetworkAutomatically ) )]
	public Guid NetworkId => Id;

	/// <summary>
	/// When to network this object(if ever). <br />
	/// Used by the <see cref="UpdateNetworking"/> method.
	/// </summary>
	[Property, ReadOnly]
	[Title( "Network Mode" )]
	[Feature( ENTITY ), Order( NETWORK_ORDER )]
	[ToggleGroup( nameof( NetworkAutomatically ) )]
	public NetworkMode NetworkingMode => NetworkingModeDefault;
	protected virtual NetworkMode NetworkingModeDefault => NetworkMode.Object;

	/// <summary>
	/// Who the object can/does belong to. <br />
	/// Used by the <see cref="UpdateNetworking"/> method.
	/// </summary>
	[Property, ReadOnly]
	[Title( "Transfer Mode" )]
	[Feature( ENTITY ), Order( NETWORK_ORDER )]
	[ToggleGroup( nameof( NetworkAutomatically ) )]
	public OwnerTransfer NetworkTransferMode => NetworkTransferModeDefault;
	protected virtual OwnerTransfer NetworkTransferModeDefault => OwnerTransfer.Fixed;

	/// <summary>
	/// What to do upon losing a network owner. <br />
	/// Used by the <see cref="UpdateNetworking"/> method.
	/// </summary>
	[Property, ReadOnly]
	[Title( "Orphaned Mode" )]
	[Feature( ENTITY ), Order( NETWORK_ORDER )]
	[ToggleGroup( nameof( NetworkAutomatically ) )]
	public NetworkOrphaned NetworkOrphanedMode => NetworkOrphanedModeDefault;
	protected virtual NetworkOrphaned NetworkOrphanedModeDefault => NetworkOrphaned.Destroy;

	/// <summary>
	/// The connection owning this entity.
	/// </summary>
	[Property, InlineEditor]
	[Header( "Debug" )]
	[Title( "Owner" )]
	[ShowIf( nameof( PlayingScene ), true )]
	[Feature( ENTITY ), Order( NETWORK_ORDER )]
	[ToggleGroup( nameof( NetworkAutomatically ) )]
	public Connection NetworkOwner => Network?.Owner;

	/// <summary>
	/// The default owning connection to assign upon network setup.
	/// Will be called <see cref="OnStart"/> if <see cref="NetworkAutomatically"/> is enabled.
	/// </summary>
	/// <returns> The connection to assign in the <see cref="SetupNetworking"/> method. </returns>
	public virtual Connection DefaultNetworkOwner => Network?.Owner ?? Connection.Host;

	/// <returns> If <see cref="SetupNetworking"/> is allowed to execute. </returns>
	protected virtual bool AllowNetworkSetup
		=> Network is not null && (Network.IsCreator || Network.IsOwner);

	protected override void OnStart()
	{
		base.OnStart();

		if ( NetworkAutomatically )
			SetupNetworking();
	}

	/// <summary>
	/// By default this networks the object using <see cref="DefaultNetworkOwner"/>. <br />
	/// Called <see cref="OnStart"/> if <see cref="NetworkAutomatically"/> is enabled.
	/// </summary>
	protected virtual void SetupNetworking( bool force = false )
	{
		if ( !AllowNetworkSetup )
			return;

		UpdateNetworking( DefaultNetworkOwner );
	}

	/// <summary>
	/// Update this object's networking according to the related properties(by default). <br />
	/// You may use <see cref="DefaultNetworkOwner"/> to respect this component's preference.
	/// </summary>
	/// <param name="cn"> The connection to assign this object to(if any). </param>
	public virtual void UpdateNetworking( Connection cn )
	{
		if ( !this.IsValid() )
			return;

		// this.Log( $"assigning ownership to Connection:[{cn}]" );

		GameObject.NetworkSetup(
			cn: cn,
			orphanMode: NetworkOrphanedMode,
			ownerTransfer: NetworkTransferMode,
			netMode: NetworkingMode,
			ignoreProxy: false
		);
	}
}
