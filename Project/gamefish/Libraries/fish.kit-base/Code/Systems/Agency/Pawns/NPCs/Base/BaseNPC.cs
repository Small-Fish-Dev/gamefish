namespace GameFish;

/// <summary>
/// A pawn designed to be autonomous.
/// </summary>
[EditorHandle( Icon = "🤖" )]
public abstract partial class BaseNPC : BasePawn
{
	/// <summary>
	/// Is this NPC able to function?
	/// </summary>
	public virtual bool IsCapable => this.IsValid() && IsAlive;
}
