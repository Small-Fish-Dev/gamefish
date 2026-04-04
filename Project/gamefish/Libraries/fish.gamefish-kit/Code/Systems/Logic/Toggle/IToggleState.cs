using System;

namespace GameFish;

/// <summary>
/// Indicates a primary on/off state and lets you change that.
/// <br /> <br />
/// <b> NOTE: </b> Supports <see cref="ToggleCommand"/>.
/// </summary>
[Icon( "touch_app" )]
public interface IToggleState
{
	public bool IsOn { get; }
	public virtual bool IsOff => !IsOn;

	/// <summary>
	/// Indicates if a state would be valid. <br />
	/// Usually also prevents it from being set.
	/// </summary>
	/// <returns> If that state could hypothetically be set. </returns>
	public bool IsStateAllowed( in bool isOn );

	/// <summary>
	/// Tries to set this to on/off.
	/// </summary>
	/// <param name="isOn"> The instructed state. </param>
	public void SetState( in bool isOn );
}

partial class Library
{
	// It's an extension so that it will always function the same.

	/// <summary>
	/// Attempts to safely set the state.
	/// </summary>
	/// <returns> If the state was affected. </returns>
	public static bool TrySetState( this IToggleState state, in ToggleCommand cmd )
	{
		if ( state is null )
			return false;

		if ( state is not Component c || !c.IsValid() )
			return false;

		var bState = cmd switch
		{
			ToggleCommand.Enable => true,
			ToggleCommand.Disable => false,
			_ => !state.IsOn
		};

		return state.TrySetState( in bState );
	}

	/// <summary>
	/// Attempts to safely set the state.
	/// </summary>
	/// <returns> If the state was affected. </returns>
	public static bool TrySetState( this IToggleState state, in bool bState )
	{
		if ( state is null )
			return false;

		if ( state is not Component c || !c.IsValid() )
			return false;

		try
		{
			if ( state.IsOn == bState )
				return false;

			if ( !state.IsStateAllowed( bState ) )
				return false;

			state.SetState( bState );

			return state.IsOn == bState;
		}
		catch ( Exception e )
		{
			Print.WarnFrom( state, $"{nameof( TrySetState )} exception: {e}" );
			return false;
		}
	}
}
