using System;

namespace GameFish;

/// <summary>
/// Moves directly towards the target.
/// </summary>
public partial class ActorChasingBehavior : ActorBehavior
{
	/// <summary> Multiply movement speed by this amount while chasing. </summary>
	[Property, Feature( EVASION )]
	public virtual float ChasingHaste { get; set; } = 1.15f;

	[Property, Feature( EVASION )]
	public virtual float ChaseWaveAmplitude { get; set; } = 0.3f;

	[Property, Feature( EVASION )]
	public virtual float ChaseWaveFrequency { get; set; } = 4f;

	/// <summary>
	/// The progression of the wave varies randomly by this amount.
	/// </summary>
	[Property, Feature( EVASION )]
	public virtual FloatRange ChaseWaveNoise { get; set; } = new( 0.0f, 1.0f );

	protected float ChaseWaveTime { get; set; }

	public override void Perform( in float deltaTime )
	{
		if ( !Controller.IsValid() )
			return;

		RestoreSeed();

		var noiseMin = ChaseWaveNoise.Min.Min( ChaseWaveNoise.Max );
		var noiseMax = ChaseWaveNoise.Min.Max( ChaseWaveNoise.Max );

		ChaseWaveTime += Time.Delta * Random.Float( noiseMin, noiseMax );
	}

	public override Vector3? GetDestination( Equipment equip = null )
	{
		if ( LastKnownTargetPosition is not Vector3 targetPos )
			return null;

		// Keep a slight distance if they are in front of you.
		if ( ActiveEquip?.PrimaryFunction is var primary && primary.IsValid() )
			return targetPos += targetPos.Direction( WorldPosition ) * primary.IdealRange.Min;

		return targetPos;
	}

	public override void PreMove( in float deltaTime, in Vector3? dest, in float speed, ref Vector3 wishVel )
	{
		if ( LastKnownTargetPosition is null )
			return;

		// Move a bit unpredictably.
		const float pi2 = MathF.PI * 2f;

		var time = MathF.Abs( ChaseWaveTime - Seed ).UnsignedMod( pi2 );

		var height = MathF.Sin( time * ChaseWaveFrequency ) * ChaseWaveAmplitude;
		var wave = EyeRotation.Right * height;

		wishVel += wave;
	}
}
