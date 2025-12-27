namespace GameFish;

partial class Client
{
	[Sync]
	public float InputForward
	{
		get => _vForward.Clamp( -1f, 1f );
		set => _vForward = value.Clamp( -1f, 1f );
	}

	[Sync]
	public float InputHorizontal
	{
		get => _vHorizontal.Clamp( -1f, 1f );
		set => _vHorizontal = value.Clamp( -1f, 1f );
	}

	protected float _vForward;
	protected float _vHorizontal;

	public override void FrameSimulate( in float deltaTime )
	{
		UpdateVehicleInput();

		base.FrameSimulate( deltaTime );
	}

	protected virtual void UpdateVehicleInput()
	{
		var vMove = Input.AnalogMove;

		var accelDir = vMove.x;
		var steerDir = vMove.y;

		if ( Input.Down( "Forward" ) )
			accelDir += 1f;

		if ( Input.Down( "Backward" ) )
			accelDir -= 1f;

		if ( Input.Down( "Left" ) )
			steerDir += 1f;

		if ( Input.Down( "Right" ) )
			steerDir -= 1f;

		InputForward = accelDir.Clamp( -1f, 1f );
		InputHorizontal = steerDir.Clamp( -1f, 1f );
	}

	/// <summary>
	/// Tells you if the client is aiming and if so by what angle.
	/// </summary>
	/// <param name="aim"> The current aim delta. </param>
	/// <returns> If we should be able to aim. </returns>
	public virtual bool TryGetAim( out Angles aim )
	{
		// Must be ours and with no input focus.
		if ( !IsLocal || IControls.BlockAiming )
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
		if ( !IsLocal || IControls.BlockMoving )
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
		if ( IControls.BlockActions )
			return false;

		return isKeyboard
			? Input.Keyboard.Pressed( code )
			: Input.Pressed( code );
	}

	/// <returns> If this action/key is allowed and being pressed. </returns>
	public virtual bool IsButtonDown( in string code, in bool isKeyboard, in bool warnMissing = true )
	{
		if ( IControls.BlockActions )
			return false;

		return isKeyboard
			? Input.Keyboard.Down( code )
			: Input.Down( code, complainOnMissing: warnMissing );
	}

	/// <returns> If this action/key is allowed and being released. </returns>
	public virtual bool IsButtonReleased( in string code, in bool isKeyboard )
	{
		if ( IControls.BlockActions )
			return false;

		return isKeyboard
			? Input.Keyboard.Released( code )
			: Input.Released( code );
	}
}
