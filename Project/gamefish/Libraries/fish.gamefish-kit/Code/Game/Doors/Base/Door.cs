namespace GameFish;

/// <summary>
/// Something that opens, closes and can be locked.
/// </summary>
[Icon( "door_front" )]
[EditorHandle( Icon = "🚪" )]
public partial class Door : DynamicEntity
{
	protected const int DOOR_ORDER = DEFAULT_ORDER - 1000;
	protected const int LOGIC_ORDER = DOOR_ORDER + 100;

	protected const int DOOR_DEBUG_ORDER = DOOR_ORDER - 5;

	protected const int DOOR_ANIMATION_ORDER = DOOR_ORDER + 10;
	protected const int DOOR_TRANSFORM_ORDER = DOOR_ORDER + 20;
	protected const int DOOR_SOUND_ORDER = DOOR_ORDER + 50;

	/// <summary>
	/// Plays when this starts opening.
	/// </summary>
	[Property]
	[Title( "Opening" )]
	[Feature( DOOR ), Group( SOUNDS ), Order( DOOR_SOUND_ORDER )]
	public SoundEvent OpeningSound { get; set; }

	/// <summary>
	/// Plays when this is fully open.
	/// </summary>
	[Property]
	[Title( "Opened" )]
	[Feature( DOOR ), Group( SOUNDS ), Order( DOOR_SOUND_ORDER )]
	public SoundEvent OpenedSound { get; set; }

	/// <summary>
	/// Plays when this starts closing.
	/// </summary>
	[Property]
	[Title( "Closing" )]
	[Feature( DOOR ), Group( SOUNDS ), Order( DOOR_SOUND_ORDER )]
	public SoundEvent ClosingSound { get; set; }

	/// <summary>
	/// Plays when this starts closing.
	/// </summary>
	[Property]
	[Title( "Closed" )]
	[Feature( DOOR ), Group( SOUNDS ), Order( DOOR_SOUND_ORDER )]
	public SoundEvent ClosedSound { get; set; }

	/// <summary>
	/// Plays when it can open because it's locked.
	/// </summary>
	[Property]
	[Title( "Locked" )]
	[Feature( DOOR ), Group( SOUNDS ), Order( DOOR_SOUND_ORDER )]
	public SoundEvent LockedSound { get; set; }

	/// <summary>
	/// Typically prevents the door from being opened while closed.
	/// </summary>
	[Sync]
	public bool IsLocked
	{
		get => _isLocked is true;
		set
		{
			if ( _isLocked.HasValue && _isLocked.Value == value )
				return;

			var wasLocked = _isLocked;
			_isLocked = value;

			OnSetIsLocked( in value, in wasLocked );
		}
	}

	protected bool? _isLocked;

	public override bool IsDestructible => false;

	public IEnumerable<DoorModule> DoorModules => GetModules<DoorModule>() ?? [];

	protected override void OnEnabled()
	{
		base.OnEnabled();

		Tags?.Add( TAG_DOOR );
	}

	protected override void OnStart()
	{
		OnStateStart();
		OnAnimationStart();

		base.OnStart();
	}

	protected virtual void OnSetIsLocked( in bool isLocked, in bool? wasLocked )
	{
		if ( !InGame )
			return;

		if ( isLocked )
			OnLocked();
		else
			OnUnlocked();
	}

	protected virtual void OnLocked()
	{
		foreach ( var m in DoorModules )
			m.OnLocked();
	}

	protected virtual void OnUnlocked()
	{
		foreach ( var m in DoorModules )
			m.OnUnlocked();
	}

	/// <summary>
	/// To be called when the door has just opened.
	/// </summary>
	protected virtual void PlayDoorSound( SoundEvent snd )
	{
		if ( !snd.IsValid() )
			return;

		if ( IsProxy )
			return;

		var obj = ModelObject.AsValid() ?? GameObject;

		if ( !obj.IsValid() )
			return;

		BroadcastSound( snd, SoundSettings.FromLocal( obj, default ) );
	}

	/// <summary>
	/// To be called when the door has fully opened.
	/// </summary>
	protected virtual void PlayOpenedEffects()
		=> PlayDoorSound( OpenedSound );

	/// <summary>
	/// To be called when the door has just opened.
	/// </summary>
	protected virtual void PlayOpeningEffects()
		=> PlayDoorSound( OpeningSound );

	/// <summary>
	/// To be called when the door has just closed.
	/// </summary>
	protected virtual void PlayClosedEffects()
		=> PlayDoorSound( ClosedSound );

	/// <summary>
	/// To be called when the door has just closed.
	/// </summary>
	protected virtual void PlayClosingEffects()
		=> PlayDoorSound( ClosingSound );
}
