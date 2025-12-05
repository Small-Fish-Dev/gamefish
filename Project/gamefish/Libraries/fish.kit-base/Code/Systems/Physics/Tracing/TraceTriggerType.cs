namespace GameFish;

/// <summary>
/// How should a trace consider triggers?
/// </summary>
public enum TraceTriggerHitType
{
	/// <summary> Don't hit triggers. </summary>
	Ignore,
	/// <summary> Hit both solids and triggers. </summary>
	Include,
	/// <summary> Only hit triggers, never solids. </summary>
	Exclusive
}
