namespace GameFish;

/// <summary>
/// The status of a process that expects a result.
/// </summary>
public enum AttemptStatus
{
	/// <summary>
	/// The process is ongoing. A result has not been determined.
	/// </summary>
	Active,

	/// <summary>
	/// The attempt has failed for some reason.
	/// </summary>
	Failure,

	/// <summary>
	/// The attempt has succeeded.
	/// </summary>
	Success
}
