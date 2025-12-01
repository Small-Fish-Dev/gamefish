using System;
using System.Text.Json.Serialization;

namespace GameFish;

partial class PawnView
{
	/// <summary>
	/// If true: log perspective changes in console.
	/// </summary>
	[Property]
	[Title( "Debug" )]
	[Feature( MODES )]
	public bool DebugMode { get; set; }

	/// <summary>
	/// The current view mode.
	/// </summary>
	[Property]
	[Title( "Current" )]
	[Feature( MODES )]
	public virtual ViewMode Mode
	{
		get => _mode.IsValid() ? _mode
			: _mode = Modes.FirstOrDefault();

		set
		{
			var oldMode = _mode != value ? value : null;
			_mode = value;

			OnSetMode( value, oldMode );
		}
	}

	protected ViewMode _mode;

	/// <summary>
	/// The designated first person mode(if any). <br />
	/// Some modes may support first person but not be first person primarily.
	/// </summary>
	[Property]
	[Title( "First Person" )]
	[Feature( MODES )]
	public virtual FirstPersonViewMode FirstPersonMode
	{
		get => _firstPersonMode.IsValid() ? _firstPersonMode
			: Modes?.FirstOrDefault( mode => mode?.AllowFirstPerson is true ) as FirstPersonViewMode;

		set => _firstPersonMode = value;
	}

	protected FirstPersonViewMode _firstPersonMode;

	public IEnumerable<ViewMode> Modes => GetModules<ViewMode>();

	public virtual void CycleMode( in int dir )
	{
		var modes = GetModules<ViewMode>().ToList();

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
	protected virtual void OnSetMode( ViewMode newMode, ViewMode oldMode = null )
	{
		if ( DebugMode )
			this.Log( $"Set Mode: {newMode}" );

		if ( newMode.IsValid() )
			newMode.OnModeEnter( previousMode: oldMode );
	}

	/// <summary>
	/// Attempts to put this view into its designated first person mode(if any).
	/// </summary>
	public virtual bool TryEnterFirstPerson()
	{
		if ( FirstPersonMode.IsValid() )
		{
			Mode = FirstPersonMode;
			return true;
		}

		return false;
	}
}
