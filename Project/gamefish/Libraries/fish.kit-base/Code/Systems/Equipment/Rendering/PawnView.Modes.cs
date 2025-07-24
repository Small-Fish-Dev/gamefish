using System;

namespace GameFish;

partial class PawnView
{
	public const string PERSPECTIVE = "Perspective";

	public const string FIRST_PERSON = "First Person";
	public const int FIRST_PERSON_ORDER = 200;

	public const string THIRD_PERSON = "Third Person";
	public const int THIRD_PERSON_ORDER = 201;

	public const string FREE_CAM = "Free Cam";
	public const int FREE_CAM_ORDER = 202;

	public const string FIXED = "Fixed View";
	public const int FIXED_ORDER = 203;

	public const string MANUAL = "Manual View";
	public const int MANUAL_ORDER = 204;

	public const string CUSTOM = "Custom View";
	public const int CUSTOM_ORDER = 205;

	public enum Perspective
	{
		/// <summary> From the eye. </summary>
		[Icon( "face" )]
		FirstPerson,

		/// <summary> A modern Lakitu. </summary>
		[Icon( "videocam" )]
		ThirdPerson,

		/// <summary> Like noclip. </summary>
		[Icon( "flight" )]
		FreeCam,

		/// <summary> Frozen camera. </summary>
		[Icon( "rectangle" )]
		Fixed,

		/// <summary> Hard-coded override. </summary>
		[Icon( "code" )]
		Manual,

		/// <summary> Uses ActionGraph. </summary>
		[Icon( "auto_awesome" )]
		Custom
	}

	/// <summary>
	/// If true: log perspective changes in console.
	/// </summary>
	[Property]
	[Feature( VIEW ), Group( PERSPECTIVE )]
	public bool DebugMode { get; set; }

	[Property]
	[Feature( VIEW ), Group( PERSPECTIVE )]
	public Perspective DefaultMode
	{
		get => _defaultMode;
		set
		{
			_defaultMode = value;

			if ( this.InEditor() )
				Mode = value;
		}
	}

	protected Perspective _defaultMode;

	[Sync]
	[Property]
	[Feature( VIEW ), Group( PERSPECTIVE )]
	protected Perspective Mode
	{
		get => _mode;
		set
		{
			if ( _mode == value )
				return;

			_mode = value;
			OnSetPerspective( in value );
		}
	}

	protected Perspective _mode = Perspective.FirstPerson;

	[Property, WideMode]
	[Title( "Allowed Modes" )]
	[Feature( VIEW ), Group( PERSPECTIVE )]
	public Dictionary<Perspective, bool> ModeList { get; set; } = new()
	{
		[Perspective.FirstPerson] = true,
		[Perspective.ThirdPerson] = true,
	};

	public virtual bool IsModeEnabled( Perspective mode )
		=> ModeList is not null && ModeList.TryGetValue( mode, out var bAllow ) && bAllow;

	public virtual IEnumerable<Perspective> GetAllowedModes()
		=> Enum.GetValues<Perspective>()?.Where( IsModeEnabled ) ?? [];

	/// <summary>
	/// Used to manage pawn model fade and view model visibility.
	/// </summary>
	[Property]
	[Title( "Fade Range" )]
	[Feature( VIEW ), Group( FIRST_PERSON ), Order( FIRST_PERSON_ORDER )]
	public FloatRange FirstPersonRange { get; set; } = new( 5f, 20f );

	/// <summary>
	/// Called whenever <see cref="Mode"/> is set to "Custom".
	/// </summary>
	[Property]
	[Feature( VIEW ), Group( CUSTOM ), Order( CUSTOM_ORDER )]
	public Action<BasePawn, PawnView> CustomSetAction { get; set; }

	/// <summary>
	/// Called per update whenever <see cref="Mode"/> is "Custom".
	/// </summary>
	[Property]
	[Feature( VIEW ), Group( CUSTOM ), Order( CUSTOM_ORDER )]
	public Action<BasePawn, PawnView> CustomUpdateAction { get; set; }

	public virtual void CycleMode( in int dir )
	{
		var modes = GetAllowedModes().ToList();

		if ( modes.Count <= 0 )
			return;

		var iMode = modes.IndexOf( Mode );
		var iNext = (iMode + dir) % modes.Count;

		Mode = modes.ElementAtOrDefault( iNext );
	}

	/// <summary>
	/// Called when <see cref="Mode"/> has been changed.
	/// This is also called whenever it's set in the editor.
	/// </summary>
	protected virtual void OnSetPerspective( in Perspective newMode )
	{
		if ( DebugMode )
			this.Log( "Set Perspective: " + newMode );

		if ( newMode != Perspective.FirstPerson )
			ToggleViewModel( false );

		switch ( _mode )
		{
			case Perspective.FirstPerson:
				OnFirstPersonModeSet();
				break;

			case Perspective.ThirdPerson:
				OnThirdPersonModeSet();
				break;

			case Perspective.FreeCam:
				OnFreeCamModeSet();
				break;

			case Perspective.Fixed:
				OnFixedModeSet();
				break;

			case Perspective.Manual:
				OnManualModeSet();
				break;

			case Perspective.Custom:
				OnCustomModeSet();
				break;
		}

		StartTransition();
	}

	protected virtual void OnFirstPersonModeSet()
	{
	}

	protected virtual void OnThirdPersonModeSet()
	{
	}

	protected virtual void OnFreeCamModeSet()
	{
	}

	protected virtual void OnFixedModeSet()
	{
	}

	protected virtual void OnManualModeSet()
	{
	}

	protected virtual void OnCustomModeSet()
	{
		try
		{
			CustomSetAction( Pawn, this );
		}
		catch ( Exception e )
		{
			this.Warn( e );
		}
	}
}
