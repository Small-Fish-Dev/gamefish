using GameFish;

namespace Fishbox;

partial class ShooterController
{
	[Property]
	[Feature( BADASS )]
	[Order( BADASS_ORDER )]
	[ToggleGroup( nameof( FocusEnabled ), Label = FOCUS )]
	public virtual bool FocusEnabled { get; set; } = true;

	[Property]
	[InputAction]
	[Title( "Input" )]
	[Feature( BADASS )]
	[Order( BADASS_ORDER )]
	[ToggleGroup( nameof( FocusEnabled ) )]
	public virtual string FocusInput { get; set; } = "Attack2";

	[Sync]
	public bool IsFocusing
	{
		get => _isFocusing;
		protected set
		{
			if ( _isFocusing == value )
				return;

			_isFocusing = value;
			OnSetIsFocusing( in value );
		}
	}

	protected bool _isFocusing = false;

	protected virtual void OnSetIsFocusing( in bool isFocusing )
	{
	}

	public virtual bool ShouldFocus()
	{
		if ( !IsAlive )
			return false;

		return Input.Down( FocusInput );
	}
}
