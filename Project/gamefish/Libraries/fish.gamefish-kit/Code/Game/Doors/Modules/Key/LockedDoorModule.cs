using GameFish;

namespace GameFish;

/// <summary>
/// Allows you to unlock this door with a <see cref="DoorKey"/>.
/// </summary>
[Icon( "sim_card" )]
public class LockedDoorModule : DoorModule
{
	protected const int KEY_ORDER = DOOR_ORDER - 100;

	/// <summary>
	/// What type of key does this respond to?
	/// </summary>
	[Title( "Type" )]
	[Property, WideMode]
	[Order( KEY_ORDER )]
	[Feature( DOOR ), Group( KEY )]
	public KeyType UnlockType { get; set; } = KeyType.Color;

	/// <summary>
	/// Require this key color.
	/// </summary>
	[EnumButtonGroup]
	[Title( "Color" )]
	[Property, WideMode]
	[Order( KEY_ORDER )]
	[Feature( DOOR ), Group( KEY )]
	[ShowIf( nameof( UnlockType ), KeyType.Color )]
	public KeyColor UnlockColor { get; set; }

	/// <summary>
	/// Require this key ID.
	/// </summary>
	[Title( "ID" )]
	[Property, WideMode]
	[Order( KEY_ORDER )]
	[Feature( DOOR ), Group( KEY )]
	[ShowIf( nameof( UnlockType ), KeyType.ID )]
	public string UnlockID { get; set; }

	public bool TryUnlock( DoorKey key )
	{
		if ( !key.IsValid() )
			return false;

		// Door must be locked.
		if ( !Door.IsValid() || !Door.IsLocked )
			return false;

		if ( key.UnlockType != UnlockType )
			return false;

		if ( UnlockType is KeyType.Color )
			if ( key.UnlockColor != UnlockColor )
				return false;

		if ( UnlockType is KeyType.ID )
			if ( key.UnlockID != UnlockID )
				return false;

		Door.IsLocked = false;

		return true;
	}
}
