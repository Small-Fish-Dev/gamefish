namespace GameFish;

[DefaultValue( Ignore )]
public enum LogicInputHandling
{
	/// <summary>
	/// Values input to this are not considered.
	/// </summary>
	Ignore,

	/// <summary>
	/// Input values will be used if provided.
	/// </summary>
	Prefer,

	/// <summary>
	/// Only input values are considered, never any others.
	/// </summary>
	Require,
}
