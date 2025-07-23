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

	public const string CUSTOM = "Custom View";
	public const int CUSTOM_ORDER = 204;

	public enum Perspective
	{
		/// <summary> From the eye position. </summary>
		[Icon( "face" )]
		FirstPerson,

		/// <summary> Over-view. </summary>
		[Icon( "videocam" )]
		ThirdPerson,

		/// <summary> Like noclip. </summary>
		[Icon( "flight" )]
		FreeCam,

		/// <summary> Frozen camera. </summary>
		[Icon( "rectangle" )]
		Fixed,

		/// <summary> Uses ActionGraph. </summary>
		[Icon( "auto_awesome" )]
		Custom
	}

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

	/// <summary>
	/// 
	/// </summary>
	/// <returns> The currently active mode. </returns>
	public virtual Perspective GetMode() => Mode;

	[Property, WideMode]
	[Feature( VIEW ), Group( PERSPECTIVE )]
	public Dictionary<Perspective, bool> ModeList { get; set; } = new()
	{
		[Perspective.FirstPerson] = true,
		[Perspective.ThirdPerson] = true,
	};

	private bool IsModeEnabled( Perspective mode )
		=> ModeList is not null && ModeList.TryGetValue( mode, out var bAllow ) && bAllow;

	public List<Perspective> GetAllowedModes()
		=> Enum.GetValues<Perspective>()
			.Where( IsModeEnabled )
			.ToList();

	/// <summary>
	/// Used to manage pawn model fade and view model visibility.
	/// </summary>
	[Property]
	[Title( "Fade Range" )]
	[Feature( VIEW ), Group( FIRST_PERSON ), Order( FIRST_PERSON_ORDER )]
	public FloatRange FirstPersonRange { get; set; } = new( 5f, 20f );

	/// <summary>
	/// Called per update whenever <see cref="Mode"/> is set to "Custom".
	/// </summary>
	[Property]
	[Feature( VIEW ), Group( CUSTOM ), Order( CUSTOM_ORDER )]
	public Action<BasePawn, PawnView> CustomUpdateAction { get; set; }

	public void CycleMode( in int dir )
	{
		var modes = GetAllowedModes();

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
		this.Log( "Set Perspective: " + newMode );

		if ( newMode != Perspective.FirstPerson )
			ToggleViewModel( false );

		StartTransition();
	}

	protected virtual void OnPerspectiveUpdate( in float deltaTime )
	{
		switch ( _mode )
		{
			case Perspective.FirstPerson:
				OnFirstPersonModeUpdate( in deltaTime );
				break;

			case Perspective.ThirdPerson:
				OnThirdPersonModeUpdate( in deltaTime );
				break;

			case Perspective.FreeCam:
				OnFreeCamModeUpdate( in deltaTime );
				break;

			case Perspective.Fixed:
				OnFixedModeUpdate( in deltaTime );
				break;

			case Perspective.Custom:
				OnCustomModeUpdate( in deltaTime );
				break;
		}
	}

	protected virtual void OnFirstPersonModeUpdate( in float deltaTime )
	{
		var pawn = Pawn;

		if ( !pawn.IsValid() )
			return;

		Relative = new();

		UpdateViewModel( in deltaTime );
	}

	protected virtual void OnThirdPersonModeUpdate( in float deltaTime )
	{
		var pawn = Pawn;

		if ( !pawn.IsValid() )
			return;

		var tPos = Vector3.Backward * 150f;

		Relative = new( tPos, Rotation.Identity );
	}

	protected virtual void OnFreeCamModeUpdate( in float deltaTime )
	{
		this.Warn( $"Mode {Perspective.FreeCam} is not yet implemented." );
	}

	protected virtual void OnFixedModeUpdate( in float deltaTime )
	{
	}

	protected virtual void OnCustomModeUpdate( in float deltaTime )
	{
		try
		{
			CustomUpdateAction( Pawn, this );
		}
		catch ( Exception e )
		{
			this.Warn( e );
		}
	}
}
