namespace GameFish;

partial class Client
{
	/// <summary>
	/// Tells you if the client is aiming and if so by what angle.
	/// </summary>
	/// <param name="aim"> The current aim delta. </param>
	/// <returns> If we should be able to aim. </returns>
	public virtual bool TryGetAim( out Angles aim )
	{
		// Must be ours and with no input focus.
		if ( !IsLocal || IFocus.Aiming )
		{
			aim = Angles.Zero;
			return false;
		}

		aim = Input.AnalogLook;
		return true;
	}

	/// <summary>
	/// Tells you if the client is aiming and if so by what angle.
	/// </summary>
	/// <param name="aim"> The current aim delta. </param>
	/// <returns> If we should be able to aim. </returns>
	public static bool TryGetLocalAim( out Angles aim )
	{
		if ( Local.IsValid() )
			return Local.TryGetAim( out aim );

		aim = Angles.Zero;
		return false;
	}

	/// <summary>
	/// Tells you if the client is moving and if so in what direction.
	/// </summary>
	/// <param name="moveDir"> The current movement direction. </param>
	/// <returns> If we should be able to move. </returns>
	public virtual bool TryGetMove( out Vector3 moveDir )
	{
		// Must be ours and with no input focus.
		if ( !IsLocal || IFocus.Moving )
		{
			moveDir = Vector3.Zero;
			return false;
		}

		moveDir = Input.AnalogMove.ClampLength( 1f );
		return true;
	}

	/// <summary>
	/// Tells you if the client is moving and if so in what direction.
	/// </summary>
	/// <param name="moveDir"> The current movement direction. </param>
	/// <returns> If we should be able to move. </returns>
	public static bool TryGetLocalMove( out Vector3 moveDir )
	{
		if ( Local.IsValid() )
			return Local.TryGetMove( out moveDir );

		moveDir = Vector3.Zero;
		return false;
	}

	/// <returns> If this action/key is allowed and was pressed. </returns>
	public virtual bool IsButtonPressed( in string code, in bool isKeyboard )
	{
		if ( IFocus.Action )
			return false;

		return isKeyboard
			? Input.Keyboard.Pressed( code )
			: Input.Pressed( code );
	}

	/// <returns> If this action/key is allowed and being pressed. </returns>
	public virtual bool IsButtonDown( in string code, in bool isKeyboard, in bool warnMissing = true )
	{
		if ( IFocus.Action )
			return false;

		return isKeyboard
			? Input.Keyboard.Down( code )
			: Input.Down( code, complainOnMissing: warnMissing );
	}
}
