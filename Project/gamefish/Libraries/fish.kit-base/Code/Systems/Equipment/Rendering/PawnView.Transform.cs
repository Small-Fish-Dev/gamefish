namespace GameFish;

partial class PawnView
{
	/// <summary>
	/// Updates and then returns the world transform of this view.
	/// </summary>
	/// <returns> Where the camera should be positioned. </returns>
	public virtual Transform GetViewTransform()
	{
		UpdateTransform();
		return WorldTransform;
	}

	protected virtual void UpdateTransform()
	{
		EnsureValidHierarchy();
		UpdateModeTransform();
	}

	/// <summary>
	/// Sets this view's transform according to the curent mode.
	/// </summary>
	protected virtual void UpdateModeTransform()
	{
		switch ( Mode )
		{
			case Perspective.FirstPerson:
				SetFirstPersonModeTransform();
				break;

			case Perspective.ThirdPerson:
				SetThirdPersonModeTransform();
				break;

			case Perspective.FreeCam:
				SetFreeCamModeTransform();
				break;

			case Perspective.Fixed:
				SetFixedModeTransform();
				break;

			case Perspective.Manual:
				SetManualModeTransform();
				break;

			case Perspective.Custom:
				SetCustomModeTransform();
				break;
		}
	}

	/// <summary>
	/// Sets the transform safely using <see cref="Relative"/> and with transition support.
	/// </summary>
	protected virtual void SetRelativeTransform()
	{
		var pawn = Pawn;

		if ( !pawn.IsValid() )
			return;

		if ( Previous is Offset prevOffset )
		{
			// Smoothed transitioning.
			var offsLerped = prevOffset.LerpTo( Relative, TransitionFraction );
			var tLerped = offsLerped.ToWorld( pawn.EyeTransform );

			TrySetPosition( tLerped.Position );
			TrySetRotation( tLerped.Rotation );
		}
		else
		{
			// No transitioning.
			var tRelative = Relative.ToWorld( pawn.EyeTransform );

			TrySetPosition( tRelative.Position );
			TrySetRotation( tRelative.Rotation );
		}
	}

	protected virtual void SetFirstPersonModeTransform()
	{
		SetRelativeTransform();
	}

	protected virtual void SetThirdPersonModeTransform()
	{
		SetRelativeTransform();
	}

	protected virtual void SetFreeCamModeTransform()
	{
	}

	protected virtual void SetFixedModeTransform()
	{
	}

	protected virtual void SetManualModeTransform()
	{
	}

	protected virtual void SetCustomModeTransform()
	{
	}
}
