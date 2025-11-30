namespace GameFish;

partial class MentalState
{
	public virtual float GetVisionAngle( in float angle )
		=> angle;

	public virtual float GetVisionDistance( in float distance )
		=> distance;

	public virtual float GetVisionFrequency( in float frequency )
		=> frequency;

	public virtual void OnTargetVisible( BasePawn target, in Vector3? at = null )
	{
	}
}
