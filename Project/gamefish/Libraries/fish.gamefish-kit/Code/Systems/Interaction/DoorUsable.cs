using GameFish;
using GameFish.Razor;

namespace GameFish;

/// <summary>
/// Lets players open/close <see cref="Door"/>.
/// </summary>
public partial class DoorUsable : UsableModule
{
	protected const int DOOR_ORDER = USE_ORDER - 50;

	public override bool IsParent( ModuleEntity comp )
		=> comp is Door;

	/// <summary>
	/// If true: opening the door is allowed.
	/// <br /> <br />
	/// <b> NOTE: </b> The door may still prevent it.
	/// </summary>
	[Property]
	[Title( "Opening" )]
	[Order( DOOR_ORDER )]
	[Feature( USE ), Group( DOOR )]
	public bool AllowOpening { get; set; } = true;

	/// <summary>
	/// If true: closing the door is allowed.
	/// <br /> <br />
	/// <b> NOTE: </b> The door may still prevent it.
	/// </summary>
	[Property]
	[Title( "Closing" )]
	[Order( DOOR_ORDER )]
	[Feature( USE ), Group( DOOR )]
	public bool AllowClosing { get; set; } = true;

	[Property]
	[Title( "Name" )]
	[Order( DOOR_ORDER )]
	[Feature( USE ), Group( DOOR )]
	public string DoorName { get; set; } = "Door";

	/// <summary>
	/// The thing what open and close.
	/// </summary>
	[Property]
	[InputAction]
	[Order( DOOR_ORDER )]
	[Title( "Component" )]
	[Feature( USE ), Group( DOOR )]
	public Door Door
	{
		get => _target ??= Parent as Door;
		set => _target = value;
	}

	protected Door _target;

	protected override void OnUpdate()
	{
		base.OnUpdate();

		// DebugInput();
	}

	public override bool IsUsable( Pawn pawn )
	{
		if ( !Door.IsValid() )
			return false;

		if ( Door.IsOpened )
			return AllowClosing;

		if ( Door.IsClosed )
			return AllowOpening;

		return base.IsUsable( pawn );
	}

	protected override void OnUse( Pawn pawn )
	{
		base.OnUse( pawn );

		if ( Door.IsValid() )
			Door.TryToggle();
	}

	public override IEnumerable<DisplayText> GetDisplayLines()
	{
		if ( !InGame )
			return null;

		if ( !Door.IsValid() )
			return null;

		if ( Door.IsLocked )
			return GetDoorLines( "Locked" );

		if ( !IsUsable( Client.Local?.Pawn ) )
			return null;

		if ( Door.IsOpened && Door.CanClose() )
			return GetDoorLines( "Close" );

		if ( Door.IsClosed && Door.CanOpen() )
			return GetDoorLines( "Open" );

		return null;
	}

	protected List<DisplayText> GetDoorLines( string text )
	{
		var name = DoorName.IsBlank() ? "Door" : DoorName;
		var nameLine = new DisplayText( name, DisplayElement.Heading1 );

		List<DisplayText> lines = [nameLine];

		if ( !text.IsBlank() )
			lines.Add( new( text, DisplayElement.Title ) );

		return lines;
	}
}
