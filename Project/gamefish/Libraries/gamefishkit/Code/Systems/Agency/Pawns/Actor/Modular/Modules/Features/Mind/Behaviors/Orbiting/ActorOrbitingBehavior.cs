using System.Diagnostics.CodeAnalysis;
using Sandbox.Movement;

namespace GameFish;

/// <summary>
/// Circle around the target
/// </summary>
public partial class ActorOrbitingBehavior : ActorBehavior
{
	/// <summary> How often their strafe lasts. </summary>
	[Range( -1f, 1f ), Step( 1f )]
	[Property, Feature( EVASION )]
	public FloatRange OrbitDuration { get; set; } = new( 4, 7 );

	/// <summary> If they're moving left or right. (null = random) </summary>
	[Range( -1f, 1f ), Step( 1f )]
	[Property, Feature( EVASION )]
	public float? OrbitDirection { get; set; }

	/// <summary> They'll close in at this speed. </summary>
	[Property, Feature( EVASION )]
	public float OrbitDecay { get; set; }

	/// <summary> They'll try to stay within this range. </summary>
	[Property, Feature( EVASION )]
	public FloatRange OrbitRange { get; set; } = new( 400f, 700f );

	/// <summary> How often to reverse the direction of orbit. </summary>
	[Property, Feature( EVASION )]
	public FloatRange OrbitReverseFrequency { get; set; } = new( 1f, 4f );

	/// <summary>
	/// The current distance we're deciding to orbit.
	/// </summary>
	[Property, ReadOnly]
	[Feature( EVASION )]
	public float OrbitalDistance { get; set; }

	public TimeUntil NextReverseOrbit { get; set; }

	public override void OnSelect( ActorBehavior oldBehavior = null )
	{
		if ( Target?.Center is not Vector3 center )
		{
			Mind?.TrySetDefaultState();
			return;
		}

		OrbitDirection ??= Random.CoinFlip ? -1f : 1f;
		OrbitalDistance = center.Distance( WorldPosition );
	}

	public override void Perform( in float deltaTime )
	{
		if ( !Target.IsValid() )
		{
			TryStop();
			return;
		}

		if ( OrbitDecay != 0f )
			OrbitalDistance -= OrbitDecay * Time.Delta;

		OrbitalDistance = OrbitalDistance.Clamp( OrbitRange );

		if ( ActiveEquip?.IdealRange is FloatRange range )
			OrbitalDistance = OrbitalDistance.Clamp( range );

		if ( OrbitReverseFrequency.Max != 0f && NextReverseOrbit )
		{
			OrbitDirection = (OrbitDirection ?? 1f) * -1f;
			NextReverseOrbit = Random.Float( OrbitReverseFrequency.Min, OrbitReverseFrequency.Max );
		}
	}

	public override Vector3? GetDestination( Equipment equip = null )
	{
		if ( Target?.WorldPosition is not Vector3 targetPos )
			return null;

		var lookDir = WorldPosition.Direction( targetPos );
		var moveDir = Rotation.LookAt( lookDir ).Right * (OrbitDirection ?? 1f);

		// DebugOverlay.Line( WorldPosition, destPos, Color.Orange, duration: 1f );

		return targetPos + (moveDir * OrbitalDistance);
	}
}
