namespace GameFish;

partial class Client
{
	/// <summary>
	/// Tells you if the client can aim, and if so how we're aiming.
	/// </summary>
	/// <param name="aim"> The aim delta this frame. </param>
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

	public static bool TryGetLocalAim( out Angles aim )
	{
		if ( Local.IsValid() )
			return Local.TryGetAim( out aim );

		aim = Angles.Zero;
		return false;
	}
}
