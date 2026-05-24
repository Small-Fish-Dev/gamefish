using System.Text.Json.Serialization;
using GameFish;

namespace GameFish;

partial class Door
{
	/// <summary>
	/// If enabled: print state events to console.
	/// </summary>
	[Property]
	[Order( DOOR_DEBUG_ORDER )]
	[Title( "Logging (state)" )]
	[Feature( DOOR ), Group( DEBUG )]
	protected bool DebugStateLogging { get; set; } = false;

	/// <summary>
	/// If enabled: print logical events to console.
	/// </summary>
	[Property]
	[Order( DOOR_DEBUG_ORDER )]
	[Title( "Logging (logic)" )]
	[Feature( DOOR ), Group( DEBUG )]
	protected bool DebugLogicLogging { get; set; } = false;

	/// <summary>
	/// <inheritdoc cref="State" />
	/// <br /> <br />
	/// <b> NOTE: </b> This setting is not saved.
	/// </summary>
	[Title( "State" )]
	[Property, JsonIgnore]
	[Order( DOOR_DEBUG_ORDER )]
	[Feature( DOOR ), Group( DEBUG )]
	[ShowIf( nameof( InGame ), true )]
	protected DoorState InspectorState
	{
		get => State;
		set => State = value;
	}

	/// <summary>
	/// <inheritdoc cref="IsLocked" />
	/// <br /> <br />
	/// <b> NOTE: </b> This setting is not saved.
	/// </summary>
	[Title( "Is Locked" )]
	[Property, JsonIgnore]
	[Order( DOOR_DEBUG_ORDER )]
	[Feature( DOOR ), Group( DEBUG )]
	[ShowIf( nameof( InGame ), true )]
	protected bool InspectorIsLocked
	{
		get => IsLocked;
		set => IsLocked = value;
	}
}
