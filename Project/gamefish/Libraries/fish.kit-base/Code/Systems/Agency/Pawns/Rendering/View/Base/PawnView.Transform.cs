using System.Threading.Tasks;

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

	public virtual void UpdateTransform()
	{
		UpdateModeTransform();
	}

	/// <summary>
	/// Sets the transform safely using <see cref="Relative"/> and with transition support.
	/// </summary>
	protected virtual void SetRelativeTransform()
	{
		var pawn = Pawn;

		if ( !pawn.IsValid() )
			return;

		var tEye = EyeTransform;

		if ( PreviousTransform is Transform prevTrans )
		{
			// Smoothed transitioning.
			var tRelative = Relative.ToWorld( tEye );
			var tLerped = prevTrans.LerpTo( tRelative, TransitionFraction );

			TrySetPosition( tLerped.Position );
			TrySetRotation( tLerped.Rotation );
		}
		else if ( PreviousOffset is Offset prevOffset )
		{
			// Smoothed transitioning.
			var offsLerped = prevOffset.LerpTo( Relative, TransitionFraction );
			var tLerped = offsLerped.ToWorld( tEye );

			TrySetPosition( tLerped.Position );
			TrySetRotation( tLerped.Rotation );
		}
		else
		{
			// No transitioning.
			var tRelative = Relative.ToWorld( tEye );

			TrySetPosition( tRelative.Position );
			TrySetRotation( tRelative.Rotation );
		}
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
