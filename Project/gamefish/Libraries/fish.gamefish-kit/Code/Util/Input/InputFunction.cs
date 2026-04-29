using System.Text.Json.Serialization;

namespace GameFish;

/// <summary>
/// Input settings for a key/button including a cooldown.
/// <b> NOTE: </b> Ideal for functions meant to have a delay between each activation.
/// </summary>
public struct InputFunction
{
	/// <inheritdoc cref="InputSetting" />
	[KeyProperty]
	[InlineEditor( Label = false )]
	public InputSetting Setting { get; set; }

	/// <summary>
	/// The default delay before the function can be activated again.
	/// </summary>
	[KeyProperty]
	[Range( 0f, 5f, clamped: false )]
	public float Cooldown { get; set; } = 0.5f;

	/// <inheritdoc cref="InputSetting.Action" />
	[Hide, JsonIgnore, ReadOnly]
	public readonly string Action => Setting.Action;

	/// <inheritdoc cref="InputSetting.Mode" />
	[Hide, JsonIgnore, ReadOnly]
	public readonly InputMode Mode => Setting.Mode;

	/// <inheritdoc cref="InputSetting.IsActive" />
	[Hide, JsonIgnore, ReadOnly]
	public readonly bool IsActive => Setting.IsActive;

	/// <inheritdoc cref="InputSetting.IsHeld" />
	[Hide, JsonIgnore, ReadOnly]
	public readonly bool IsHeld => Setting.IsHeld;

	/// <inheritdoc cref="InputSetting.IsPressed" />
	[Hide, JsonIgnore, ReadOnly]
	public readonly bool IsPressed => Setting.IsPressed;

	/// <inheritdoc cref="InputSetting.IsReleased" />
	[Hide, JsonIgnore, ReadOnly]
	public readonly bool IsReleased => Setting.IsReleased;

	public InputFunction() { }

	public InputFunction( in string action, in InputMode mode )
	{
		Setting = new( in action, in mode );
	}

	public InputFunction( in string action, in InputMode mode, in float cooldown )
	{
		Setting = new( in action, in mode );
		Cooldown = cooldown;
	}

	/// <summary>
	/// Re-applies a cooldown if it has ended.
	/// </summary>
	/// <param name="timer"> The cooldown timer. </param>
	/// <param name="delay"> The optional cooldown override. </param>
	/// <returns> If a cooldown was applied. </returns>
	public readonly bool TryActivate( ref TimeUntil timer, in float? delay = null )
	{
		if ( !timer )
			return false;

		timer = delay ?? Cooldown;
		return true;
	}

	/// <summary>
	/// Re-applies a cooldown if it has ended.
	/// </summary>
	/// <param name="timer"> The cooldown timer. </param>
	/// <param name="delay"> The optional cooldown override. </param>
	/// <returns> If a cooldown was applied. </returns>
	public readonly bool TryActivate( ref RealTimeUntil timer, in float? delay = null )
	{
		if ( !timer )
			return false;

		timer = delay ?? Cooldown;
		return true;
	}

	/// <summary>
	/// Re-applies a cooldown if it has ended.
	/// </summary>
	/// <param name="timer"> The cooldown timer. </param>
	/// <param name="delay"> The optional cooldown override. </param>
	/// <returns> If a cooldown was applied. </returns>
	public readonly bool TryActivate( ref TimeSince timer, in float? delay = null )
	{
		if ( timer < (delay ?? Cooldown) )
			return false;

		timer = 0f;
		return true;
	}

	/// <summary>
	/// Re-applies a cooldown if it has ended.
	/// </summary>
	/// <param name="timer"> The cooldown timer. </param>
	/// <param name="delay"> The optional cooldown override. </param>
	/// <returns> If a cooldown was applied. </returns>
	public readonly bool TryActivate( ref RealTimeSince timer, in float? delay = null )
	{
		if ( timer < (delay ?? Cooldown) )
			return false;

		timer = 0f;
		return true;
	}
}
