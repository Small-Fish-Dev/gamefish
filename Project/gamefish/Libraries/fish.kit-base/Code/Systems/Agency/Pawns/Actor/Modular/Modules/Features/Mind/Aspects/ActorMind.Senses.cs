namespace GameFish;

partial class ActorMind
{
	public virtual float GetVisionAngle( in float defaultAngle )
		=> State?.GetVisionAngle( in defaultAngle ) ?? defaultAngle;

	public virtual float GetVisionDistance( in float defaultDistance )
		=> State?.GetVisionDistance( in defaultDistance ) ?? defaultDistance;

	public virtual float GetVisionFrequency( in float defaultFrequency )
		=> State?.GetVisionFrequency( in defaultFrequency ) ?? defaultFrequency;
}
