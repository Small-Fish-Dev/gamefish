using System;

namespace GameFish;

/// <summary>
/// Moves sideways.
/// </summary>
public partial class ActorStrafingBehavior : ActorBehavior
{
	/*
	/// <summary> How often they initiate a strafe. </summary>
	[Property]
	[Feature( EVASION )]
	public FloatRange StrafeFrequency { get; set; } = new( 1f, 2f );
	*/

	public override void Perform( in float deltaTime )
	{
	}

	public override Vector3? GetDestination( Equipment equip = null )
	{
		/*var dir = Vector3.Random
			.SubtractDirection( Vector3.Up )
			.WithZ( 0f )
			.Normal;*/

		var dir = EyeRotation.Right * MathF.Sin( Time.Now % MathF.PI * 5f );

		return dir * 1000f;
	}
}
