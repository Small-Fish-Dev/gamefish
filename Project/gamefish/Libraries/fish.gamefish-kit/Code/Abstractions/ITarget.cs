namespace GameFish;

/// <summary>
/// Indicates that this is meant to target a <see cref="GameObject"/>.
/// </summary>
public interface ITarget
{
	public GameObject Target { get; set; }
}
