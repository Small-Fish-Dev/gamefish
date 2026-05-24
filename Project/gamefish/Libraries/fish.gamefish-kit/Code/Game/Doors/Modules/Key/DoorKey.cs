namespace GameFish;

/// <summary>
/// Allows you to unlock a door that has a <see cref="LockedDoorModule"/>.
/// </summary>
[Icon( "vpn_key" )]
[Title( "Door Key" )]
[EditorHandle( Icon = "💳" )]
public class DoorKey : Pickup
{
	protected const int KEY_ORDER = DEFAULT_ORDER - 1000;

	/// <summary>
	/// Unlock doors requiring key type.
	/// </summary>
	[Property]
	[Title( "Type" )]
	[Order( KEY_ORDER )]
	[Feature( DOOR ), Group( KEY )]
	public KeyType UnlockType { get; set; }

	/// <summary>
	/// Unlock doors needing this key color.
	/// </summary>
	[EnumButtonGroup]
	[Title( "Color" )]
	[Property, WideMode]
	[Order( KEY_ORDER )]
	[Feature( DOOR ), Group( KEY )]
	[ShowIf( nameof( UnlockType ), KeyType.Color )]
	public KeyColor UnlockColor { get; set; }

	/// <summary>
	/// Unlock doors needing this key ID.
	/// </summary>
	[Title( "ID" )]
	[Property, WideMode]
	[Order( KEY_ORDER )]
	[Feature( DOOR ), Group( KEY )]
	[ShowIf( nameof( UnlockType ), KeyType.ID )]
	public string UnlockID { get; set; } = "key_01";

	protected override void OnPickup( Player pl )
	{
		if ( !this.InGame() || !pl.IsValid() )
			return;

		var locks = Scene.GetAll<Door>()
			.Select( door => door.GetModule<LockedDoorModule>() )
			.Where( l => l.IsValid() && l.Active );

		foreach ( var l in locks )
			l.TryUnlock( this );
	}
}
